/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2019 51 Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY.
 *
 * This Original Work is licensed under the European Union Public Licence (EUPL) 
 * v.1.2 and is subject to its terms as set out below.
 *
 * If a copy of the EUPL was not distributed with this file, You can obtain
 * one at https://opensource.org/licenses/EUPL-1.2.
 *
 * The 'Compatible Licences' set out in the Appendix to the EUPL (as may be
 * amended by the European Commission) shall be deemed incompatible for
 * the purposes of the Work and the provisions of the compatibility
 * clause in Article 5 of the EUPL shall not apply.
 * 
 * If using the Work as, or as part of, a network application, by 
 * including the attribution notice(s) required under Article 5 of the EUPL
 * in the end user terms of the application under an appropriate heading, 
 * such notice(s) shall fulfill the requirements of that article.
 * ********************************************************************* */

using System;

namespace FiftyOne.Pipeline.Examples
{
    /// <summary>
    /// Base class for examples. 
    /// Ensures that all example classes have a default constructor and
    /// have consistent use of the RunExample method.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the example class.
    /// </typeparam>
    public abstract class ExampleBase<T> : IExample
        where T: ExampleBase<T>, new()
    {
        /// <summary>
        /// Main method for derived classes to call.
        /// </summary>
        public static void Main()
        {
            // Create a new instance of the example class.
            T p = new T();
            // Run the example
            p.RunExample();

            Console.WriteLine("==========================================");
            Console.WriteLine("Example complete. Press any key to exit.");
            // Wait for user to press a key.
            Console.ReadKey();
        }

        /// <summary>
        /// Implementations of this method in derived classes should 
        /// execute the example in full.
        /// </summary>
        /// <remarks>
        /// This method MUST NOT contain any code that requires interaction
        /// with the user. e.g. Console.ReadKey().
        /// This is because the method will also be run as an automated test. 
        /// In that scenario, there will be no user so the call will wait 
        /// indefinitely.
        /// </remarks>
        public abstract void RunExample();
    }
}
