/*
 * Phosphorus Five, copyright 2014 - 2015, Thomas Hansen, phosphorusfive@gmail.com
 * Phosphorus Five is licensed under the terms of the MIT license, see the enclosed LICENSE file for details
 */

using System.IO;
using p5.core;
using p5.exp;

/// <summary>
///     Main namespace for everything related to folders.
/// 
///     The Active Events within this namespace, allows you to create, remove, view, and change, your folders, and the contents of the folders, 
///     within your Phosphorus Five installation.
/// </summary>
namespace p5.file.folder
{
    /// <summary>
    ///     Class to help create folders on disc.
    /// 
    ///     Encapsulates the [create-folder] Active Event, and its associated helper methods.
    /// </summary>
    public static class Create
    {
        /// <summary>
        ///     Creates zero or more folders on disc.
        /// 
        ///     If folder exists from before, then false is returned.
        /// 
        ///     Example that creates a "foo" folder, on root of your application;
        /// 
        ///     <pre>create-folder:foo</pre>
        /// </summary>
        /// <param name="context">Application context.</param>
        /// <param name="e">Parameters passed into Active Event.</param>
        [ActiveEvent (Name = "create-folder")]
        private static void create_folder (ApplicationContext context, ActiveEventArgs e)
        {
            // making sure we clean up and remove all arguments passed in after execution
            using (new Utilities.ArgsRemover (e.Args, true)) {

                // retrieving root folder
                var rootFolder = Common.GetRootFolder (context);

                // iterating through each folder caller wants to create
                foreach (var idx in Common.GetSource (e.Args, context)) {

                    // checking to see if folder already exists, and if it does, return "false" to caller
                    if (Directory.Exists (rootFolder + idx)) {

                        // folder already exists, returning back to caller that creation was unsuccessful
                        e.Args.Add (new Node (idx, false));
                    } else {

                        // folder didn't exist, creating it, and returning "true" back to caller, meaning "success"
                        Directory.CreateDirectory (rootFolder + idx);
                        e.Args.Add (new Node (idx, true));
                    }
                }
            }
        }
    }
}
