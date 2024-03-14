using FiftyOne.Pipeline.Engines.TestHelpers;
using FiftyOne.Pipeline.JavaScriptBuilder.FlowElement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using System.Text;

namespace FiftyOne.Pipeline.JavaScript.Tests
{
    [TestClass]
    public class JavaScriptBuilderElementTemplateTests: JavaScriptBuilderElementTestsBase
    {
        private WebApplication webApp { get; set; }
        private CancellationTokenSource webAppKillSwitch { get; set; }

        [TestInitialize]
        public override async Task Init()
        {
            await base.Init();

            var builder = WebApplication.CreateBuilder();
            webApp = builder.Build();

            webApp.MapGet("/{*rest}", (string rest) =>
            """
            <!DOCTYPE>
            <html>
              <head>
                <title>Test Page</title>
              </head>
              <body>
              </body>
            </html>
            """
            );

            webApp.MapPost("/51dpipeline/json", () =>
                "{}"
            );

            webApp.Urls.Add(ClientServerUrl);

            webAppKillSwitch = new CancellationTokenSource();
            await webApp.StartAsync(webAppKillSwitch.Token);
        }

        private static string BuildXHRJS(string dstUrl, string method = "POST", string postData = "dummy", bool acceptJson = false)
        {
            StringBuilder s = new();
            s.Append("xhr = new XMLHttpRequest();");
            s.Append($"xhr.open('{method}', '{dstUrl}', true);");
            if (acceptJson)
            {
                s.Append("xmlhttp.setRequestHeader(\"Accept\", \"application/json;charset=UTF-8\");");
            }
            s.Append($"xhr.send('{postData}');");
            return s.ToString();
        }


        [TestMethod]
        public void JavaScriptBuilderTemplate_VerifyInterception()
        {
            bool testDone = false;
            OnRequestSent = e => {
                var p = e.RequestPostData;
                testDone = true;
            };
            IJavaScriptExecutor js = Driver;
            var q = js.ExecuteScript(BuildXHRJS($"{ClientServerUrl}/51dpipeline/json"));
            Assert.IsTrue(testDone);
        }

        [TestMethod]
        //[Timeout(20000)]
        public void JavaScriptBuilderTemplate_ValidateSetCookieBlockCall()
        {
            string propName = "javascriptalpha";
            string propCode = "// begin\ndocument.cookie = \"51D_alpha=\" + 42;\nalpha_set = true;\n// end\n";
            string testCode = "alpha_set";

            JObject jsonData = new() {
                { "device", new JObject { { propName, propCode } } },
                { "javascriptProperties", new JArray { $"device.{propName}" } },
            };

            //int firstColon = ClientServerUrl.IndexOf(":");
            //var _javaScriptBuilderElement =
            //    new JavaScriptBuilderElementBuilder(_loggerFactory)
            //    .SetMinify(false)
            //    .SetProtocol(ClientServerUrl.Substring(0, firstColon))
            //    .SetHost(ClientServerUrl.Substring(firstColon + 3))
            //    .Build();
            //var flowData = new Mock<IFlowData>();
            //Configure(flowData, jsonData);

            //IJavaScriptBuilderElementData result = null;
            //flowData.Setup(d => d.GetOrAdd(
            //    It.IsAny<ITypedKey<IJavaScriptBuilderElementData>>(),
            //    It.IsAny<Func<IPipeline, IJavaScriptBuilderElementData>>()))
            //    .Returns<ITypedKey<IJavaScriptBuilderElementData>, Func<IPipeline, IJavaScriptBuilderElementData>>((k, f) =>
            //    {
            //        result = f(flowData.Object.Pipeline);
            //        return result;
            //    });

            //_javaScriptBuilderElement.Process(flowData.Object);

            //// JS aquired, now test

            //string postData = null;
            //bool completed = false;
            //OnRequestSent = e =>
            //{
            //    if (e.RequestUrl.EndsWith("json"))
            //    {
            //        postData = e.RequestPostData;
            //    }
            //    if (e.RequestUrl.EndsWith("completed"))
            //    {
            //        completed = true;
            //    }
            //};
            //string additionalCode
            //    = "; fod.complete(function (data) { "
            //    + BuildXHRJS($"{ClientServerUrl}/51dpipeline/completed")
            //    + " });";

            //IJavaScriptExecutor js = Driver;
            //js.ExecuteScript(result.JavaScript + additionalCode);

            //Assert.IsNotNull(postData);

            //while (!completed)
            //{
            //    bool hrPrinted = false;
            //    var entries = Driver.Manage().Logs.GetLog(LogType.Browser);
            //    foreach (var entry in entries)
            //    {
            //        if (!hrPrinted)
            //        {
            //            Console.WriteLine("----- ----- -----");
            //            hrPrinted = true;
            //        }
            //        Console.WriteLine(entry.ToString());
            //    }
            //    Thread.Sleep(1000);
            //}

            //var controlResult = js.ExecuteScript(testCode);
            //Assert.IsNotNull(controlResult);
            //Assert.IsInstanceOfType<bool>(controlResult);
            //Assert.IsTrue((bool)controlResult);
        }

        [TestCleanup]
        public override async Task Cleanup()
        {
            await base.Cleanup();
            webAppKillSwitch.Cancel();
            await webApp.StopAsync();
        }
    }
}
