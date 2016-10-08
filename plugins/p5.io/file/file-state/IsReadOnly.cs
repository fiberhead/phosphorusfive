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

using System.IO;
using p5.core;
using p5.io.common;

namespace p5.io.file.file_state
{
    /// <summary>
    ///     Class to help check the read-only state of a file
    /// </summary>
    public static class IsReadOnly
    {
        /// <summary>
        ///     Returns the read-only state of the specified file(s)
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "file-is-read-only")]
        public static void file_is_read_only (ApplicationContext context, ActiveEventArgs e)
        {
            ObjectIterator.Iterate (context, e.Args, true, "read-file", delegate (string filename, string fullpath) {
                FileInfo info = new FileInfo (fullpath);
                e.Args.Add (filename, info.IsReadOnly);
            });
        }

        /// <summary>
        ///     Changes the read-only state of the specified file(s)
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "file-set-read-only")]
        [ActiveEvent (Name = "file-delete-read-only")]
        public static void file_set_read_only (ApplicationContext context, ActiveEventArgs e)
        {
            ObjectIterator.Iterate (context, e.Args, true, "modify-file", delegate (string filename, string fullpath) {
                FileInfo info = new FileInfo (fullpath);
                info.IsReadOnly = e.Name == "file-set-read-only";
            });
        }
    }
}
