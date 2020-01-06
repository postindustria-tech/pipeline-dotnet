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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FiftyOne.Pipeline.Examples.Tests
{
    /// <summary>
    /// This test class ensures that all examples execute successfully.
    /// </summary>
    [TestClass]
    public class TestAllExamples
    {
        /// <summary>
        /// Test that all examples execute successfully.
        /// <para>
        /// Note that this test does not ensure the correctness of the 
        /// example, only that the example will run without crashing or 
        /// throwing any unhandled exceptions.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The <see cref="ExamplesToTest"/> property is used to get a list
        /// of all the examples. Each is then tested by calling the 
        /// <see cref="IExample.RunExample"/> method.
        /// </remarks>
        /// <param name="example">
        /// The example to test.
        /// </param>
        [DataTestMethod]
        [DynamicData("ExamplesToTest")]
        public void TestExamples(IExample example)
        {
            example.RunExample();
        }

        #region Internals
        private static object _examplesLock = new object();
        private static List<object[]> _examples = null;
        /// <summary>
        /// Get a list containing one instance of each example in 
        /// the repository.
        /// </summary>
        /// <remarks>
        /// Rather than requiring every new example to be added as a 
        /// reference and/or added to a list, we want a mechanism where
        /// any example that is added will automatically be picked up
        /// and tested.
        /// This achieves that goal but is somewhat fragile as it relies on 
        /// loading the DLLs containing the examples at runtime.
        /// </remarks>
        /// <remarks>
        /// The directory structure and names of DLLs containing the examples
        /// must follow these rules:
        /// 1. In the path to this test project, there must be a directory 
        /// called 'Tests' that sits in the top-level directory. 
        /// All example DLLs must be within that top-level directory.
        /// 2. All example DLLs start with the name 'FiftyOne.Pipeline.Examples.'
        /// 3. Examples to be tested must implement <see cref="IExample"/>.
        /// </remarks>
        public static IEnumerable<object[]> ExamplesToTest
        {
            get
            {
                if (_examples == null)
                {
                    lock (_examplesLock)
                    {
                        if (_examples == null)
                        {
                            _examples = new List<object[]>();
                            try
                            {
                                // Load the assemblies that contain the examples 
                                LoadExampleAssemblies();

                                // We can now iterate through all example assemblies.
                                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                                foreach (var assembly in assemblies.Where(a =>
                                    a.FullName.Contains("FiftyOne") &&
                                    a.FullName.Contains("Example")))
                                {
                                    // Exclude dynamic assemblies
                                    if (assembly.IsDynamic == false)
                                    {
                                        try
                                        {
                                            // Get any IExample implementations in the
                                            // assembly.
                                            var exampleTypes = assembly.GetTypes()
                                                .Where(t => t.IsClass && 
                                                    t.IsAbstract == false &&
                                                    typeof(IExample).IsAssignableFrom(t) &&
                                                    // Ignore ExampleError. We don't
                                                    // want to test that.
                                                    t.Equals(typeof(ExampleError)) == false);
                                            if (exampleTypes.Count() > 0)
                                            {
                                                // Create an instance of the example type
                                                // and add it to the list.
                                                _examples.AddRange(exampleTypes
                                                    .Select(t => new object[] { Activator.CreateInstance(t) as IExample }));
                                            }
                                        }
                                        // Catch type load exceptions when assembly can't be loaded 
                                        catch (ReflectionTypeLoadException) { }
                                    }
                                }
                            }
                            // If any exceptions occur then create an 
                            // ExampleError instance to feed them back to the 
                            // user.
                            catch (Exception ex)
                            {
                                _examples.Add(new object[] { new ExampleError(ex) });
                            }
                        }
                    }
                }
                return _examples;
            }
        }

        /// <summary>
        /// Load all example DLLs in the defined path.
        /// </summary>
        private static void LoadExampleAssemblies()
        {
            // Get the path to the executing assembly.
            string path = AppDomain.CurrentDomain.BaseDirectory;
            // Look for a 'Tests' directory.
            string testDir = $"{Path.DirectorySeparatorChar}Tests{Path.DirectorySeparatorChar}";
            // If there isn't one then throw an exception.
            if(path.Contains(testDir) == false)
            {
                throw new Exception($"Path '{path}' does not contain a " +
                    $"'Tests' directory as expected.");
            }
            // If we've found the 'Tests' directory then start from the 
            // directory above that.
            // This should be the repository root directory.
            path = path.Remove(path.LastIndexOf(testDir));

            // Get all DLLs in the root directory that match the
            // 51Degrees example pattern.
            var exampleDlls = Directory.GetFiles(path, 
                "FiftyOne.Pipeline.Examples.*.dll", SearchOption.AllDirectories).AsEnumerable();

            // Maintain a list of assemblies that have already been loaded
            // The same assembly being loaded twice will cause problems
            // (e.g. a debug version and a release version of the same assembly)
            List<string> alreadyLoaded =
                AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.FullName.Contains("FiftyOne.Pipeline.Examples"))
                    .Select(a => a.GetName().Name + ".dll").ToList();

            int loaded = 0;
            // Iterate through the possible DLLs to load.
            foreach (string dll in exampleDlls)
            {
                try
                {
                    // Check that the DLL in question has not already been loaded
                    if (alreadyLoaded.Any(a => dll.Contains(a)) == false)
                    {
                        // Load the DLL and add it to the list of loaded
                        // assemblies.
                        Assembly.LoadFile(dll);
                        alreadyLoaded.Add(Path.GetFileName(dll));
                        loaded++;
                    }
                }
                // Ignore exceptions loading DLLs. These may occur if we've
                // accidentally tried to load something we shouldn't have done.
                catch (ReflectionTypeLoadException) { }
                catch (FileLoadException) { }
            }
            // If no assemblies were loaded then we have no examples to test.
            // This will result in a fairly unhelpful error so throw a more
            // useful one here.
            if (loaded == 0)
            {
                throw new Exception($"No example DLLs found in path '{path}'. " +
                    $"This may be because the example projects have not been built. " +
                    $"To resolve this, rebuild the whole solution and try again.");
            }
        }

        /// <summary>
        /// Private class that is used to return an error message when
        /// loading the example DLLs fails for some reason.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The example DLLs are loaded through the 
        /// <see cref="ExamplesToTest"/> property.
        /// As this is activated by a <see cref="DynamicDataAttribute"/>,
        /// the execution of the property is handled by reflection.
        /// As such, any exceptions that occur as part of that execution
        /// result in a generic and unhelpful 'Exception has been thrown 
        /// by the target of an invocation' message.
        /// </para>
        /// <para>
        /// This situation is resolved by catching any exceptions that occur
        /// in <see cref="ExamplesToTest"/> and returning them as 
        /// <see cref="ExampleError"/> instances that will throw the 
        /// caught exception on the main thread, thus presenting the detail
        /// of the error message to the executer of the test.
        /// </para>
        /// </remarks>
        private class ExampleError : ExampleBase<ExampleError>
        {
            private Exception _ex;

            /// <summary>
            /// Default constructor
            /// </summary>
            public ExampleError() { }
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="ex">
            /// The exception to throw when <see cref="RunExample"/> is called
            /// </param>
            public ExampleError(Exception ex)
            {
                _ex = ex;
            }

            /// <summary>
            /// When the example is run, just throw a new exception 
            /// containing the original exception.
            /// </summary>
            public override void RunExample()
            {
                if (_ex != null)
                {
                    throw new Exception("Error when loading example types for testing", _ex);
                }
            }
        }
        #endregion
    }
}
