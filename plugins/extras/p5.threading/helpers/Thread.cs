/*
 * Phosphorus Five, copyright 2014 - 2016, Thomas Hansen, phosphorusfive@gmail.com
 * Phosphorus Five is licensed under the terms of the MIT license, see the enclosed LICENSE file for details
 */

using sys = System.Threading;
using System.Collections.Generic;
using p5.exp;
using p5.core;

/// <summary>
///     Main namespace for helper classes in regards to threading in Phosphorus Five
/// </summary>
namespace p5.threading.helpers
{
    /// <summary>
    ///     Class encapsulating a thread in Phosphorus Five
    /// </summary>
    public class Thread
    {
        // Actual thread wrapped by this instance
        private sys.Thread _thread;

        /// <summary>
        ///     Initializes a new instance of the <see cref="p5.threading.helpers.Thread"/> class.
        /// </summary>
        /// <param name="context">Application Context</param>
        /// <param name="lambda">Lambda object to execute in thread context</param>
        public Thread (ApplicationContext context, Node lambda)
        {
            Context = context;
            Lambda = lambda;
            _thread = new sys.Thread (Execute);
        }

        /// <summary>
        ///     Gets the Application Context that current thread is executing within
        /// </summary>
        /// <value>Application Context thread executes within</value>
        public ApplicationContext Context {
            get;
            private set;
        }

        /// <summary>
        ///     Returns the lambda thread is actually evaluating
        /// </summary>
        /// <value>The lambda object thread is evaluating</value>
        public Node Lambda {
            get;
            private set;
        }

        /// <summary>
        ///     Starts execution of lambda object on different thread
        /// </summary>
        public void Start ()
        {
            _thread.Start ();
        }

        /// <summary>
        ///     Waits for thread to finish, or number of milliseconds, whichever occurs first
        /// </summary>
        /// <param name="milliseconds">Milliseconds to wait, if -1, then join will wait "forever"</param>
        public void Join (int milliseconds = -1)
        {
            if (milliseconds == -1)
                _thread.Join ();
            else
                _thread.Join (milliseconds);
        }

        /// <summary>
        ///     Helper to retrieve all [fork] objects within scope
        /// </summary>
        /// <returns>The fork objects</returns>
        /// <param name="context">Application Context</param>
        /// <param name="args">Active Event arguments</param>
        public static IEnumerable<Node> GetForkObjects (ApplicationContext context, Node args)
        {
            // Checking if we have an argument in value, at which point argument is evaluated and
            // gives basis for lambda object to evaluate on different thread
            if (args.Value == null) {

                // Executing current scope only
                yield return args;
            } else {

                // Iterating each result from expression or value
                foreach (var idxResult in XUtil.Iterate<Node> (context, args)) {

                    // Returning each iterated result
                    yield return idxResult;
                }
            }
        }

        /*
         * Entrance method for Thread to start executing once initialized. Simply evaluates its lambda object.
         * Notice, [eval-immutable], which allows for [wait] to give access to nodes outside of actual thread, while
         * forcing [fork] without [wait] to create a Cloned instance of lambda upon creation
         */
        private void Execute ()
        {
            Context.RaiseLambda ("eval-mutable", Lambda);
        }
    }
}