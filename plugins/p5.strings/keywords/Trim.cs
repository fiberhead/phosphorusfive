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

using System.Linq;
using p5.exp;
using p5.core;

namespace p5.strings.keywords
{
    /// <summary>
    ///     Class wrapping the [trim] keyword in p5 lambda.
    /// </summary>
    public static class Trim
    {
        /// <summary>
        ///     The [trim] keyword, allows you to trim occurrencies of characters in a string
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "trim")]
        public static void lambda_trim (ApplicationContext context, ActiveEventArgs e)
        {
            // Making sure we clean up and remove all arguments passed in after execution
            using (new Utilities.ArgsRemover (e.Args)) {

                // Getting trim characters, defaulting to whitespace characters
                var characters = e.Args.GetExChildValue ("chars", context, " \r\n\t");

                // Returning length of constant or expression, converted to string if necessary
                e.Args.Value = XUtil.Single<string> (context, e.Args, true, "").Trim (characters.ToList().ToArray());
            }
        }
    }
}
