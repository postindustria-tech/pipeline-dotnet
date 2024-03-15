using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Core.TypedMap;
using FiftyOne.Pipeline.JavaScriptBuilder.Data;
using FiftyOne.Pipeline.JavaScriptBuilder.FlowElement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
        private JObject jsonData { get; set; } = new();
        private string fullJS { get; set; } = "";

        [TestInitialize]
        public override async Task Init()
        {
            await base.Init();

            var builder = WebApplication.CreateBuilder();
            webApp = builder.Build();

            webApp.MapGet("/51dpipeline/js", () => Results.Content(fullJS, "text/javascript"));
            webApp.MapGet("/{*rest}", (string rest) =>
            Results.Content(
            """
            <!DOCTYPE html>
            <html>
              <head>
                <title>Test Page</title>
              </head>
              <script src="/51dpipeline/js">
              </script>
              <body>
                Did the script already finish??
              </body>
            </html>
            """
            , "text/html"));
            webApp.MapPost("/51dpipeline/json", () => jsonData);
            webApp.MapPost("/51dpipeline/completed", () => Results.Empty);

            webApp.Use((ctx, next) =>
            {
                ctx.Response.Headers["Access-Control-Allow-Origin"] = "*";
                return next();
            });

            webApp.Urls.Add(ClientServerUrl);

            webAppKillSwitch = new CancellationTokenSource();
            await webApp.StartAsync(webAppKillSwitch.Token);

            Driver.Manage().Cookies.DeleteAllCookies();
            // Navigate to the client site.
            Driver.Navigate().GoToUrl(ClientServerUrl);
        }

        private static string BuildXHRJS(string dstUrl, string method = "POST", string postData = "dummy", bool acceptJson = false)
        {
            StringBuilder s = new();
            s.Append("xhr = new XMLHttpRequest();");
            s.Append($"xhr.open('{method}', '{dstUrl}', true);");
            if (acceptJson)
            {
                s.Append("xhr.setRequestHeader(\"Accept\", \"application/json;charset=UTF-8\");");
            }
            s.Append($"xhr.send('{postData}');");
            return s.ToString();
        }


        [TestMethod]
        [Timeout(20000)]
        public void JavaScriptBuilderTemplate_VerifyInterception()
        {
            bool testDone = false;
            OnRequestSent = e => {
                var p = e.RequestPostData;
                testDone = true;
            };
            IJavaScriptExecutor js = Driver;
            var q = js.ExecuteScript(BuildXHRJS($"{ClientServerUrl}51dpipeline/json"));
            while (!testDone)
            {
                DumpNewLogs();
                Thread.Sleep(1000);
            }
            DumpNewLogs();
            Assert.IsTrue(testDone);
        }

        [DataTestMethod]
        [Timeout(20000)]
        [DataRow("javascriptalpha", "", "\"51D_alpha=\" + 42", "", "51D_alpha=42")] // plain plus
        [DataRow("javascriptbeta", "if(true){", "\"51D_beta=\" + 29", "}", "51D_beta=29")] // plus in block
        [DataRow("gammajavascript", "", "\"51D_gamma=\" + \"zoomies\"", ", a=7", "51D_gamma=zoomies")] // plus in comma
        [DataRow("kappajavascript", "let q = x => x(3); q(e => ", "\"51D_kappa=\" + e", ")", "51D_kappa=3")] // plus in lambda
        [DataRow("omegajavascript", "let q = x => x(7); q(e => ", "`51D_omega=${e}`", ")", "51D_omega=7")] // template in lambda
        public void JavaScriptBuilderTemplate_ValidateSetCookieBlockCall(
            string propName,
            string prefixJunk,
            string propSetExpr,
            string suffixJunk,
            string expectedData)
        {

            string propSetFlag = $"{propName}_snippet_set_block_called_51d";
            string propCode =
                $"""
                console.log("starting snippet");
                {prefixJunk}document.cookie = {propSetExpr}{suffixJunk}
                ;
                window.{propSetFlag} = true;
                console.log("leaving snippet");
                """;
            string testCode = $"return window.{propSetFlag};";

            jsonData = new() {
                { "device", new JObject { { propName, propCode } } },
                { "javascriptProperties", new JArray { $"device.{propName}" } },
            };

            int firstColon = ClientServerUrl.IndexOf(":");
            var _javaScriptBuilderElement =
                new JavaScriptBuilderElementBuilder(LoggerFactory)
                .SetMinify(false)
                .SetProtocol(ClientServerUrl.Substring(0, firstColon))
                .SetHost(ClientServerUrl.Substring(firstColon + 3))
                .Build();
            var flowData = new Mock<IFlowData>();
            Configure(flowData, jsonData);

            IJavaScriptBuilderElementData result = null;
            flowData.Setup(d => d.GetOrAdd(
                It.IsAny<ITypedKey<IJavaScriptBuilderElementData>>(),
                It.IsAny<Func<IPipeline, IJavaScriptBuilderElementData>>()))
                .Returns<ITypedKey<IJavaScriptBuilderElementData>, Func<IPipeline, IJavaScriptBuilderElementData>>((k, f) =>
                {
                    result = f(flowData.Object.Pipeline);
                    return result;
                });

            _javaScriptBuilderElement.Process(flowData.Object);

            // JS aquired, now test

            string postData = null;
            bool completed = false;
            OnRequestSent = e =>
            {
                if (e.RequestUrl.EndsWith("json"))
                {
                    postData = e.RequestPostData;
                }
                if (e.RequestUrl.EndsWith("completed"))
                {
                    completed = true;
                }
            };
            string additionalCode
                = "; window.onload = (e) => { fod.complete(function (data) { "
                + BuildXHRJS($"{ClientServerUrl}51dpipeline/completed", acceptJson: true)
                + " }); };";

            fullJS = result.JavaScript + additionalCode;
            // Reload page to trigger script execution
            Driver.Manage().Cookies.DeleteAllCookies();
            Driver.Navigate().GoToUrl(ClientServerUrl);

            while (postData == null)
            {
                DumpNewLogs();
                Thread.Sleep(1000);
            }
            Assert.IsTrue(postData.Contains(expectedData), $"[{postData}] does not contain [{expectedData}]");
            while (!completed)
            {
                DumpNewLogs();
                Thread.Sleep(1000);
            }

            IJavaScriptExecutor js = Driver;
            while (true)
            {
                var objResult = js.ExecuteScript(testCode);
                if (objResult is bool newResult)
                {
                    Assert.IsTrue(newResult);
                    break;
                }
                DumpNewLogs();
                Thread.Sleep(1000);
            };

            int cookieCount = Driver.Manage().Cookies.AllCookies.Count;
            if (cookieCount > 0)
            {
                Assert.Inconclusive($"Detected cookies after script completion: {cookieCount}");
            }
        }

        private void DumpNewLogs()
        {
            bool hrPrinted = false;
            var entries = Driver.Manage().Logs.GetLog(LogType.Browser);
            foreach (var entry in entries)
            {
                if (!hrPrinted)
                {
                    Console.WriteLine("----- ----- -----");
                    hrPrinted = true;
                }
                Console.WriteLine($"[BROWSER] > ${entry}");
            }
        }

        [TestCleanup]
        public override async Task Cleanup()
        {
            await base.Cleanup();
            webAppKillSwitch.Cancel();
            await webApp.StopAsync();
            jsonData.RemoveAll();
        }
    }
}
