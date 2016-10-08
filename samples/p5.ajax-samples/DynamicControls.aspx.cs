/*
 * Phosphorus Five, copyright 2014 - 2016, Thomas Hansen, mr.gaia@gaiasoul.com
 * 
 * This file is part of Phosphorus Five.
 *
 * Phosphorus Five is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License version 3, as published by
 * the Free Software Foundation.
 *
 *
 * Phosphorus Five is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Phosphorus Five.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * If you cannot for some reasons use the GPL license, Phosphorus
 * Five is also commercially available under Quid Pro Quo terms. Check 
 * out our website at http://gaiasoul.com for more details.
 */

using System;
using System.Linq;
using System.Web.UI;
using p5.ajax.core;

namespace p5.samples
{
    using p5 = p5.ajax.widgets;

    public partial class DynamicControls : AjaxPage
    {
        private static int _next = 1;
        protected p5.Container List;
        protected p5.Void Txt;
        protected p5.Void Update;

        private string CurrentEdit
        {
            get { return ViewState ["CurrentEdit"] as string; }
            set { ViewState ["CurrentEdit"] = value; }
        }

        protected override void OnPreRender (EventArgs e)
        {
            if (CurrentEdit != null)
                Update.RemoveAttribute ("disabled");
            else
                Update ["disabled"] = null;
            base.OnPreRender (e);
        }

        [WebMethod]
        protected void item_onclick (p5.Literal sender, EventArgs e)
        {
            if (sender.innerValue == "Are you sure?") {
                List.RemoveControlPersistent (sender);
                CurrentEdit = null;
                Txt["value"] = "";
            } else {
                Txt ["value"] = sender.innerValue;
                CurrentEdit = sender.ID;
                sender.innerValue = "Are you sure?";
            }
        }

        [WebMethod]
        protected void append_onclick (p5.Void btn, EventArgs e)
        {
            CurrentEdit = null;
            var widget = List.CreatePersistentControl<p5.Literal> ("x" + (_next ++), List.Controls.Count);
            widget.Element = "li";
            widget.RenderType = p5.Widget.RenderingType.open;
            widget ["onclick"] = "item_onclick";
            widget.innerValue = Txt ["value"];
        }

        [WebMethod]
        protected void insert_top_onclick (p5.Void btn, EventArgs e)
        {
            CurrentEdit = null;
            var widget = List.CreatePersistentControl<p5.Literal> ("x" + (_next ++), 0);
            widget.Element = "li";
            widget.RenderType = p5.Widget.RenderingType.open;
            widget ["onclick"] = "item_onclick";
            widget.innerValue = Txt ["value"];
        }

        [WebMethod]
        protected void insert_at_random_onclick (p5.Void btn, EventArgs e)
        {
            CurrentEdit = null;
            var widget = List.CreatePersistentControl<p5.Literal> ("x" + (_next ++), new Random ().Next (0, List.Controls.Count));
            widget.Element = "li";
            widget.RenderType = p5.Widget.RenderingType.open;
            widget ["onclick"] = "item_onclick";
            widget.innerValue = Txt ["value"];
        }

        [WebMethod]
        protected void replace_random_onclick (p5.Void btn, EventArgs e)
        {
            CurrentEdit = null;
            if (List.Controls.Count == 0) {
                Txt ["value"] = "Nothing to replace!!";
            } else {
                var which = new Random ().Next (0, List.Controls.Count);
                List.RemoveControlPersistentAt (which);

                var widget = List.CreatePersistentControl<p5.Literal> ("x" + (_next ++), which);
                widget.Element = "li";
                widget.RenderType = p5.Widget.RenderingType.open;
                widget ["onclick"] = "item_onclick";
                widget.innerValue = Txt ["value"];
            }
        }

        [WebMethod]
        protected void love_bomb_onclick (p5.Void btn, EventArgs e)
        {
            CurrentEdit = null;
            var rnd = new Random ();
            foreach (var idx in List.GetChildControls<p5.Literal> ()) {
                if (rnd.Next (0, 3) == 1) {
                    idx.innerValue = "I like turtles!";
                    idx ["class"] = "turtles";
                }
            }
        }

        [WebMethod]
        protected void harvest_love_onclick (p5.Void btn, EventArgs e)
        {
            CurrentEdit = null;
            var toRemove = List.GetChildControls<p5.Literal> ().Where (idx => idx.innerValue.Contains ("turtles")).Cast<Control> ().ToList ();
            foreach (var idx in toRemove) {
                List.RemoveControlPersistent (idx);
            }
        }

        [WebMethod]
        protected void update_onclick (p5.Void btn, EventArgs e)
        {
            var liter = (p5.Literal) List.FindControl (CurrentEdit);
            liter.innerValue = Txt ["value"];
            CurrentEdit = null;
        }
    }
}