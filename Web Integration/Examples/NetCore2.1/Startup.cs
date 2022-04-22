/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2022 51 Degrees Mobile Experts Limited, Davidson House,
 * Forbury Square, Reading, Berkshire, United Kingdom RG1 3EU.
 *
 * This Original Work is licensed under the European Union Public Licence
 * (EUPL) v.1.2 and is subject to its terms as set out below.
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

using FiftyOne.Pipeline.Core.Configuration;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Math;
using FiftyOne.Pipeline.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;


/// @example NetCore2.1/Startup.cs
/// ASP.NET core example
/// 
/// This example shows how to:
/// 
/// 1. Set up configuration options to add elements to the 51Degrees Pipeline.
/// ```{json}
/// {
///     "PipelineOptions": {
///         "Elements": [
///             {
///                 "BuilderName": "math"
///             }
///         ]
///     }
/// }
/// ```
/// 
/// 2. Add the configuration file to the server's configuration (if the file is
/// not the usual appsettings.json).
/// ```{cs}
/// public class Startup
/// {
///     ...
///     public void ConfigureServices(IServiceCollection services)
///     {
///         Configuration = new ConfigurationBuilder()
///         .SetBasePath(Directory.GetCurrentDirectory() + "/AppData")
///         .AddJsonFile("pipeline.json")
///         .Build();
///         ...
/// ```
/// 
/// 3. Add builders and the Pipeline to the server's services.
/// ```{cs}
/// public class Startup
/// {
///     ...
///     public void ConfigureServices(IServiceCollection services)
///     {
///         ...
///         services.AddSingleton<MathElementBuilder, MathElementBuilder;
///         services.AddFiftyOne<PipelineBuilder>(Configuration);
///         ...
/// ```
/// 
/// 4. Configure the server to use the Pipeline which has just been set up.
/// ```{cs}
/// public class Startup
/// {
///     ...
///     public void Configure(IApplicationBuilder app, IHostingEnvironment env)
///     {
///         app.UseFiftyOne();
///         ...
/// ```
/// 
/// 5. Inject the `IFlowDataProvider` into a controller.
/// ```{cs}
/// public class HomeController : Controller
/// {
///     private IFlowDataProvider _flow;
///     public HomeController(IFlowDataProvider flow)
///     {
///         _flow = flow;
///     }
///     ...
/// }
/// ```
/// 
/// 6. Use the results contained in the flow data to display something on a
/// page view.
/// ```{cs}
/// public class HomeController : Controller
/// {
///     ...
///     public IActionResult Index()
///     {
///         var math = _flow.GetFlowData().Get(MathElement.math);
///         ViewData["Message"] = $"{math.Operation} = {math.Result}";
///         return View();
///     }
///     ...
/// ```
/// 
/// ## Controller
/// @include Controllers/HomeController.cs
/// 
/// ## Startup

namespace Example
{
    public class Startup
    {
        IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory() + "/AppData")
                .AddJsonFile("pipeline.json")
                .Build();

            services.AddSingleton<MathElementBuilder, MathElementBuilder>();

            services.AddFiftyOne<PipelineBuilder>(Configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseFiftyOne();
            app.UseMvc(routes =>
            routes.MapRoute(
                name: "default",
                template: "{controller=Home}/{action=Index}/{id?}"));
        }
    }
}
