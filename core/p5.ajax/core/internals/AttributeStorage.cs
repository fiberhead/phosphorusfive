/*
 * Phosphorus Five, copyright 2014 - 2015, Thomas Hansen, phosphorusfive@gmail.com
 * Phosphorus Five is licensed under the terms of the MIT license, see the enclosed LICENSE file for details
 */

using System;
using System.Collections.Generic;
using System.Web.UI;
using p5.ajax.widgets;

namespace p5.ajax.core.internals
{
    /// <summary>
    ///     Class used to encapsulate all attributes for widgets
    /// </summary>
    internal class AttributeStorage
    {
        private readonly List<Attribute> _dynamicallyAddedThisRequest = new List<Attribute> ();
        private readonly List<Attribute> _dynamicallyRemovedThisRequest = new List<Attribute> ();
        private readonly List<Attribute> _formDataThisRequest = new List<Attribute> ();

        // All of these will have values added and removed automatically during the request
        // depending upon when and how attributes are added and removed
        private readonly List<Attribute> _originalValue = new List<Attribute> ();

        // This is never touched after viewstate is loaded
        private readonly List<Attribute> _preViewState = new List<Attribute> ();
        private readonly List<Attribute> _viewStatePersisted = new List<Attribute> ();
        private readonly List<Attribute> _viewStatePersistedRemoved = new List<Attribute> ();

        /// <summary>
        ///     Determines whether this instance has the attribute with the specified name
        /// </summary>
        /// <returns><c>true</c> if this instance has the attribute with the specified key; otherwise, <c>false</c></returns>
        /// <param name="name">The name of the attribute you wish to retrieve the value of</param>
        public bool HasAttribute (string name)
        {
            return GetAttributeInternal (name) != null;
        }

        /// <summary>
        ///     Returns the value of the attribute with the specified name
        /// </summary>
        /// <returns>The value of the attribute</returns>
        /// <param name="name">The name of the attribute you wish to retrieve the value of</param>
        public string GetAttribute (string name)
        {
            var atr = GetAttributeInternal (name);
            if (atr != null)
                return atr.Value;
            return null;
        }

        /// <summary>
        ///     Changes or creates the value of the attribute with the specified name
        /// </summary>
        /// <param name="name">The name of the attribute you wish to change the value of</param>
        /// <param name="value">The new value of attribute</param>
        public void ChangeAttribute (string name, string value)
        {
            // changing attribute, but first storing old value
            StoreOldValue (name);
            SetAttributeInternal (_dynamicallyAddedThisRequest, name, value);

            // removing from all other lists
            RemoveAttributeInternal (_dynamicallyRemovedThisRequest, name);
            RemoveAttributeInternal (_formDataThisRequest, name);
            RemoveAttributeInternal (_viewStatePersistedRemoved, name);
            RemoveAttributeInternal (_viewStatePersisted, name);
        }

        /// <summary>
        ///     Removes the attribute with the specified name
        /// </summary>
        /// <param name="name">The name of the attribute you wish to remove</param>
        public void RemoveAttribute (string name, bool serializeToClient = true)
        {
            if (FindAttribute (_dynamicallyAddedThisRequest, name) != null) {

                // Attribute was added this request, simply removing the add
                RemoveAttributeInternal (_dynamicallyAddedThisRequest, name);
            } else {

                // Changing attribute, but first storing old value, but only if caller says we should
                if (serializeToClient) {
                    StoreOldValue (name);
                    SetAttributeInternal (_dynamicallyRemovedThisRequest, name, null);
                }

                // Removing from all other lists
                RemoveAttributeInternal (_dynamicallyAddedThisRequest, name);
                RemoveAttributeInternal (_formDataThisRequest, name);
                RemoveAttributeInternal (_viewStatePersistedRemoved, name);
                RemoveAttributeInternal (_viewStatePersisted, name);
            }
        }

        /// <summary>
        ///     Returns all attribute keys
        /// </summary>
        /// <value>The names of all attributes stored in this instance</value>
        public IEnumerable<string> Keys {
            get {
                /// \todo refactor, too much repetition
                Dictionary<string, bool> _alreadySen = new Dictionary<string, bool> ();
                foreach (var idx in this._dynamicallyAddedThisRequest) {
                    yield return idx.Name;
                    _alreadySen [idx.Name] = true;
                }
                foreach (var idx in this._formDataThisRequest) {
                    if (!_alreadySen.ContainsKey (idx.Name)) {
                        yield return idx.Name;
                        _alreadySen [idx.Name] = true;
                    }
                }
                foreach (var idx in this._originalValue) {
                    if (!_alreadySen.ContainsKey (idx.Name)) {
                        yield return idx.Name;
                        _alreadySen [idx.Name] = true;
                    }
                }
                foreach (var idx in this._preViewState) {
                    if (!_alreadySen.ContainsKey (idx.Name)) {
                        yield return idx.Name;
                        _alreadySen [idx.Name] = true;
                    }
                }
                foreach (var idx in this._viewStatePersisted) {
                    if (!_alreadySen.ContainsKey (idx.Name)) {
                        yield return idx.Name;
                        _alreadySen [idx.Name] = true;
                    }
                }
            }
        }

        /// <summary>
        ///     Sets the state the attribute has, before ViewState is loaded
        /// </summary>
        /// <param name="name">Name of attribute</param>
        /// <param name="value">Value of attribute</param>
        internal void SetAttributePreViewState (string name, string value)
        {
            SetAttributeInternal (_preViewState, name, value);
        }

        /// <summary>
        ///     Sets the attribute value as read from your HTTP POST parameters
        /// </summary>
        /// <param name="name">Name of attribute</param>
        /// <param name="value">Value of attribute</param>
        internal void SetAttributeFormData (string name, string value)
        {
            // Adding attribute to form data list
            SetAttributeInternal (_formDataThisRequest, name, value);

            // Removing from all other lists
            RemoveAttributeInternal (_dynamicallyRemovedThisRequest, name);
            RemoveAttributeInternal (_dynamicallyAddedThisRequest, name);
            RemoveAttributeInternal (_viewStatePersistedRemoved, name);
            RemoveAttributeInternal (_viewStatePersisted, name);
        }

        /// <summary>
        ///     Loads the attribute values from the ViewState object given
        /// </summary>
        /// <param name="viewStateObject">The ViewState object</param>
        internal void LoadFromViewState (object viewStateObject)
        {
            if (viewStateObject == null)
                return;

            var vals = viewStateObject as string[][];
            foreach (var idx in vals) {
                _viewStatePersisted.Add (new Attribute (idx [0], idx [1]));
            }
        }

        /// <summary>
        ///     Loads the attributes that are removed from ViewState
        /// </summary>
        /// <param name="viewstateObject">Viewstate object</param>
        internal void LoadRemovedFromViewState (object viewstateObject)
        {
            if (viewstateObject == null)
                return;

            var vals = viewstateObject as string[];
            foreach (var idx in vals) {
                _viewStatePersistedRemoved.Add (new Attribute (idx));
            }
        }

        /// <summary>
        ///     Returns an object intended to be put into the ViewState back to caller
        /// </summary>
        /// <returns>The attribute changes in ViewState format</returns>
        internal object SaveToViewState ()
        {
            var atrs = new List<Attribute> ();

            // First add all that are dynamically added
            atrs.AddRange (_dynamicallyAddedThisRequest);

            // Then add all that are already in the viewstate
            atrs.AddRange (_viewStatePersisted);

            // Then removing all that has the same value as when they were created before viewstate was being tracked
            atrs.RemoveAll (
                delegate (Attribute idx) { return _preViewState.Exists (idxPre => idxPre.Name == idx.Name && idxPre.Value == idx.Value); });

            // Nothing to return
            if (atrs.Count == 0)
                return null;

            // Returning attributes
            var retVal = new string[atrs.Count][];
            for (var idx = 0; idx < atrs.Count; idx++) {
                retVal [idx] = new string[2];
                retVal [idx] [0] = atrs [idx].Name;
                retVal [idx] [1] = atrs [idx].Value;
            }
            return retVal;
        }

        /// <summary>
        ///     Returns an object intended to be put into your ViewState containing all the removed attributes back to caller
        /// </summary>
        /// <returns>The removed attributes</returns>
        internal object SaveRemovedToViewState ()
        {
            var atrs = new List<Attribute> ();
            atrs.AddRange (_dynamicallyRemovedThisRequest);
            atrs.AddRange (_viewStatePersistedRemoved);

            if (atrs.Count == 0)
                return null;

            var retVal = new string[atrs.Count];
            for (var idx = 0; idx < atrs.Count; idx++) {
                retVal [idx] = atrs [0].Name;
            }
            return retVal;
        }

        /// <summary>
        ///     Renders the attributes to the given HtmlTextWriter
        /// </summary>
        /// <param name="writer">Where to write the attributes</param>
        /// <param name="widget">Widget we are rendering for</param>
        internal void Render (HtmlTextWriter writer, Widget widget)
        {
            // Adding all changes
            var lst = new List<Attribute> ();
            lst.AddRange (_dynamicallyAddedThisRequest);
            lst.AddRange (_formDataThisRequest);
            lst.AddRange (_viewStatePersisted);

            // Adding all that existed before viewstate was being tracked, but ONLY if they do not exist in other lists
            foreach (var idx in _preViewState) {
                if (FindAttribute (lst, idx.Name) == null)
                    lst.Add (idx);
            }

            // Removing stuff that's not actually attributes, but still persisted here for convenience
            lst.RemoveAll (idx => idx.Name == "outerHTML" || idx.Name == "innerValue" || idx.Name == "Tag");

            // Rendering to html writer
            foreach (var idx in lst) {
                var name = idx.Name;
                string value;
                if (idx.Name.StartsWith ("on") && Utilities.IsLegalMethodName (idx.Value)) {
                    if (!widget.HasID)
                        throw new ArgumentException ("You cannot declare events on Widgets that does not render its ID attribute");
                    if (name.EndsWith ("_"))
                        continue; // "invisible" event, only used server-side
                    value = "p5.e(event)";
                } else {
                    if (name.EndsWith ("_"))
                        continue; // "invisible" attribute, only used server-side
                    value = idx.Value;
                }
                writer.Write (" ");
                if (value == null) {
                    writer.Write (@"{0}", name);
                } else {
                    writer.Write (@"{0}=""{1}""", name, value.Replace ("\"", "\\\""));
                }
            }
        }

        /// <summary>
        ///     Registers the changed attributes during this request into the given Manager object
        /// </summary>
        /// <param name="manager">Manager to render changes into</param>
        /// <param name="id">ID of widget that owns storage object</param>
        internal void RegisterChanges (Manager manager, string id)
        {
            // Adding up the ones that were deleted during this request
            foreach (var idx in _dynamicallyRemovedThisRequest) {
                manager.RegisterDeletedAttribute (id, idx.Name);
            }

            // Adding up our changes
            foreach (var idx in _dynamicallyAddedThisRequest) {

                // Finding old value, if any
                var oldAtr = FindAttribute (_originalValue, idx.Name);
                if (oldAtr != null) {
                    if (oldAtr.Value != idx.Value)
                        manager.RegisterWidgetChanges (id, idx.Name, idx.Value, oldAtr.Value);
                } else {
                    manager.RegisterWidgetChanges (id, idx.Name, idx.Value);
                }
            }
        }

        /*
         * Helper method to store the old value of attribute
         */
        private void StoreOldValue (string name)
        {
            // We only store old value the first time attribute is touched
            if (FindAttribute (_originalValue, name) == null) {

                // Storing old value
                var old = FindAttribute (_formDataThisRequest, name) ?? (FindAttribute (_viewStatePersisted, name) ?? FindAttribute (_preViewState, name));
                if (old != null) {

                    // "Deep copy"
                    _originalValue.Add (new Attribute (old.Name, old.Value));
                }
            }
        }

        /*
         * Helper method for retrieving attribute
         */
        private Attribute GetAttributeInternal (string name)
        {
            var added = FindAttribute (_dynamicallyAddedThisRequest, name);
            if (added != null)
                return added;

            var form = FindAttribute (_formDataThisRequest, name);
            if (form != null)
                return form;

            var viewStatePersisted = FindAttribute (_viewStatePersisted, name);
            if (viewStatePersisted != null)
                return viewStatePersisted;

            var viewStateRemoved = FindAttribute (_viewStatePersistedRemoved, name);
            if (viewStateRemoved != null)
                return null; // if attribute was removed during viewstate load, we DO NOT check values that were saved before viewstate was being tracked

            var removed = FindAttribute (_dynamicallyRemovedThisRequest, name);
            if (removed != null)
                return null; // if attribute was removed this request, we DO NOT check values that were saved before viewstate was being tracked

            // last resort ...
            var preViewState = FindAttribute (_preViewState, name);
            return preViewState;
        }

        /*
         * Helper method for changing or setting attribute
         */
        private static void SetAttributeInternal (List<Attribute> attributes, string name, string value)
        {
            attributes.RemoveAll (idx => idx.Name == name);
            attributes.Add (new Attribute (name, value));
        }

        /*
         * Helper method for removing attribute
         */
        private static void RemoveAttributeInternal (List<Attribute> attributes, string name)
        {
            attributes.RemoveAll (idx => idx.Name == name);
        }

        /*
         * Helper method for retrieving attribute
         */
        private static Attribute FindAttribute (List<Attribute> attributes, string name)
        {
            return attributes.Find (idx => idx.Name == name);
        }
    }
}