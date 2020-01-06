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

using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiftyOne.Pipeline.Examples.Caching
{
    class Program : ExampleBase<Program>
    {
        public static void Main(string[] args)
        {
            Main();
        }
        
        /// <summary>
        /// Run the example
        /// </summary>
        public override void RunExample()
        {
            Console.WriteLine("==========================================");
            Console.WriteLine("Test without a cache");
            // Create a new 'prime checker' engine. 
            var primeEngine = new PrimeCheckerEngineBuilder()
                .Build();
            // Run the example using the engine
            Run(primeEngine);

            Console.WriteLine("==========================================");
            Console.WriteLine("Test with a cache");
            // Create a new 'prime checker' engine.
            primeEngine = new PrimeCheckerEngineBuilder()
                // Add a cache with the default configuration
                .SetCache(new CacheConfiguration())
                .Build();
            // Run the example using the engine with a cache
            Run(primeEngine);

            Console.WriteLine("==========================================");
            Console.WriteLine($"This engine contains a " +
                $"{Constants.PRIMECHECKER_ENGINE_DELAY}ms delay to simulate " +
                $"complex processing. The first test above does not use " +
                $"a cache so both process calls take a similar length of time. " +
                $"In the second test, a cache is added, This means that on the " +
                $"second call to process the same number, the engine already " +
                $"has the result in it's cache due to the first call and can " +
                $"return that result immediately, avoiding the costly delay.");
        }

        /// <summary>
        /// Run the example using the specified engine.
        /// </summary>
        /// <param name="primeEngine">
        /// The engine to use in the pipeline.
        /// </param>
        private void Run(PrimeCheckerEngine primeEngine)
        {
            // Create a new pipeline using the prime checker engine
            // that was passed in
            using (var pipeline = new PipelineBuilder(new LoggerFactory())
                .AddFlowElement(primeEngine)
                .Build())
            {
                Random rnd = new Random();

                // Get a random integer
                int value = rnd.Next();

                // Determine if the value is prime or not.
                Process(value, pipeline);
                // Repeat the process with the same value.
                Process(value, pipeline);
            }
        }

        /// <summary>
        /// Process the specified value with the specified pipeline
        /// </summary>
        /// <param name="value">
        /// The value to feed into the pipeline as evidence.
        /// </param>
        /// <param name="pipeline">
        /// The pipeline to use to process the value.
        /// </param>
        private void Process(int value, IPipeline pipeline)
        {
            DateTime start = DateTime.UtcNow;

            bool prime = false;
            // Create a new flow data instance.
            var data = pipeline.CreateFlowData();
            // Set the specified value as the input evidence.
            data.AddEvidence(Constants.PRIMECHECKER_EVIDENCE_KEY, value);
            // Process the evidence.
            data.Process();
            // Read the result back from the flow data.
            prime = data.Get<IPrimeCheckerData>().IsPrime ?? false;

            // Calculate the time spent processing.
            var timeTaken = DateTime.UtcNow.Subtract(start);

            // Output a message displaying the number and whether 
            // it is prime or not along with the time taken.
            Console.WriteLine($"{value} {(prime ? "is" : "is not")} prime");
            Console.WriteLine($"Processing took {timeTaken.TotalMilliseconds}ms");
        }
    }
}
