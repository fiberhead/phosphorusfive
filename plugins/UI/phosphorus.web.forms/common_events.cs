/*
 * phosphorus five, copyright 2014 - Mother Earth, Jannah, Gaia
 * phosphorus five is licensed as mitx11, see the enclosed LICENSE file for details
 */

using System;
using System.Web;
using System.Text;
using System.Web.UI;
using System.Globalization;
using System.Security.Cryptography;
using phosphorus.core;
using phosphorus.lambda;
using phosphorus.ajax.widgets;

namespace phosphorus.web.forms
{
    /// <summary>
    /// helper to retrieve and change properties of widgets
    /// </summary>
    public static class common_events
    {
        /// <summary>
        /// returns the [name] property of widget with ID of the value of [pf.get-property] as first child of
        /// [pf.get-property], named [value]
        /// </summary>
        /// <param name="context"><see cref="phosphorus.Core.ApplicationContext"/> for Active Event</param>
        /// <param name="e">parameters passed into Active Event</param>
        [ActiveEvent (Name = "pf.get-widget-property")]
        private static void pf_get_widget_property (ApplicationContext context, ActiveEventArgs e)
        {
            Node findCtrl = new Node (string.Empty, e.Args.Value);
            context.Raise ("pf.find-control", findCtrl);
            Widget widget = findCtrl [0].Get<Widget> ();
            Node nameNode = e.Args.Find (
                delegate (Node idx) {
                    return idx.Name == "name";
            });
            if (widget.ElementType == "select" && nameNode.Get<string> () == "value") {

                // special treatment for select html elements
                foreach (var idxCtrl in widget.Controls) {
                    Widget idxWidget = idxCtrl as Widget;
                    if (idxWidget != null) {
                        if (idxWidget.HasAttribute ("selected")) {
                            e.Args.Insert (0, new Node ("value", idxWidget ["value"]));
                            break;
                        }
                    }
                }
            } else {
                e.Args.Insert (0, new Node ("value", widget [nameNode.Get<string> ()]));
            }
        }

        /// <summary>
        /// set a property of the widget with the ID of the value of [pf.set-widget-property] to the value of the [value] child node
        /// of [pf.set-widget-property]. the property to set, is given through the [name] child of [pf.set-widget-property]. the [value]
        /// node can also contain formatting parameters if you wish
        /// </summary>
        /// <param name="context"><see cref="phosphorus.Core.ApplicationContext"/> for Active Event</param>
        /// <param name="e">parameters passed into Active Event</param>
        [ActiveEvent (Name = "pf.set-widget-property")]
        private static void pf_set_widget_property (ApplicationContext context, ActiveEventArgs e)
        {
            Node findCtrl = new Node (string.Empty, e.Args.Value);
            context.Raise ("pf.find-control", findCtrl);
            Widget widget = findCtrl [0].Get<Widget> ();
            Node nameNode = e.Args.Find (
                delegate (Node idx) {
                    return idx.Name == "name";
            });
            Node valueNode = e.Args.Find (
                delegate (Node idx) {
                    return idx.Name == "value";
            });
            string value = valueNode.Get<string> ();
            if (Expression.IsExpression (value)) {
                var match = Expression.Create (value).Evaluate (valueNode);
                if (match.TypeOfMatch == Match.MatchType.Count) {
                    value = match.Count.ToString ();
                } else if (!match.IsSingleLiteral) {
                    throw new ArgumentException ("[pf.set-widget-property] can only take a single literal expression");
                } else {
                    value = match.GetValue (0).ToString ();
                }
            } else if (valueNode.Count > 0) {
                value = Expression.FormatNode (valueNode);
            }
            widget [nameNode.Get<string> ()] = value;
        }

        /// <summary>
        /// stores a cookie on disc, containing all the child nodes from underneath [value] underneath [pf.store-cookie],
        /// with the name of the cookie being the value of [pf.store-cookie]. the cookie will last for [duration] number
        /// of days, which is expected to be an integer. if no [duration] is given, the default value of 365 will be used
        /// value
        /// </summary>
        /// <param name="context"><see cref="phosphorus.Core.ApplicationContext"/> for Active Event</param>
        /// <param name="e">parameters passed into Active Event</param>
        [ActiveEvent (Name = "pf.set-cookie")]
        private static void pf_set_cookie (ApplicationContext context, ActiveEventArgs e)
        {
            // setting value of cookie
            Node convert = e.Args.Find (
                delegate (Node idx) {
                return idx.Name == "value";
            });

            if (convert != null) {

                // creating persistent cookie
                HttpCookie cookie = new HttpCookie (e.Args.Get<string> ());
                convert = convert.Clone ();
                context.Raise ("pf.nodes-2-code", convert);
                cookie.Value = HttpUtility.UrlEncode (convert.Get<string> ());

                // setting date of cookie
                Node durationNode = e.Args.Find (
                    delegate (Node idx) {
                    return idx.Name == "duration";
                });
                int duration = durationNode != null ? durationNode.Get<int> () : 365;
                cookie.Expires = DateTime.Now.Date.AddDays (duration);

                // making sure cookie is "secured" before we send it back to client
                cookie.HttpOnly = true;
                HttpContext.Current.Response.Cookies.Add (cookie);
            } else {

                // destroying existing persistent cookie, if it exists
                if (HttpContext.Current.Response.Cookies.Get (e.Args.Get<string> ()) != null)
                    HttpContext.Current.Response.Cookies [e.Args.Get<string> ()].Expires = DateTime.Now.Date.AddDays (-1);
            }
        }
        
        /// <summary>
        /// retrieves a cookie with the name given through the value of [pf.get-cookie], and returns it as [value]
        /// </summary>
        /// <param name="context"><see cref="phosphorus.Core.ApplicationContext"/> for Active Event</param>
        /// <param name="e">parameters passed into Active Event</param>
        [ActiveEvent (Name = "pf.get-cookie")]
        private static void pf_get_cookie (ApplicationContext context, ActiveEventArgs e)
        {
            string cookieName = e.Args.Get<string> ();
            HttpCookie cookie = HttpContext.Current.Request.Cookies.Get (cookieName);
            if (cookie != null) {
                string cookieValue = HttpUtility.UrlDecode (cookie.Value);

                if (!string.IsNullOrEmpty (cookieValue)) {

                    Node convertNode = new Node (string.Empty, cookieValue);
                    context.Raise ("pf.code-2-nodes", convertNode);
                    if (convertNode.Count > 0) {
                        Node valueNode = new Node ("value");
                        e.Args.Add (valueNode);
                        foreach (Node idx in convertNode.Children) {
                            valueNode.Add (idx);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// creates a hash out of the value from [pf.hash-string] and returns the hash value as the value of
        /// the first child of [pf.hash-string] named [value]
        /// </summary>
        /// <param name="context"><see cref="phosphorus.Core.ApplicationContext"/> for Active Event</param>
        /// <param name="e">parameters passed into Active Event</param>
        [ActiveEvent (Name = "pf.hash-string")]
        private static void pf_hash_string (ApplicationContext context, ActiveEventArgs e)
        {
            using (MD5 md5 = MD5.Create ()) {
                string hashValue = Convert.ToBase64String (md5.ComputeHash (Encoding.UTF8.GetBytes (e.Args.Get<string> ())));
                e.Args.Add (new Node ("value", hashValue));
            }
        }
    }
}
