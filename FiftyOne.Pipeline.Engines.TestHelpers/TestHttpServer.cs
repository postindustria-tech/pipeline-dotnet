using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FiftyOne.Pipeline.Engines.TestHelpers
{
    /// <summary>
    /// An HTTP server to use during testing.
    /// </summary>
    public class TestHttpServer
    {
        /// <summary>
        /// The HttpListener used to monitor for requests.
        /// </summary>
        public HttpListener Listener { get; private set; }

        /// <summary>
        /// The content returned by the HttpServer.
        /// </summary>
        public string PageData { get; private set; } =
            "<!DOCTYPE>" +
            "<html>" +
            "  <head>" +
            "    <title>Test Page</title>" +
            "  </head>" +
            "  <body>" +
            "  </body>" +
            "</html>";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="listener">The HttpListener used to monitor for requests.</param>
        public TestHttpServer(HttpListener listener)
        {
            Listener = listener;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="listener">The HttpListener used to monitor for requests.</param>
        /// <param name="pageData">The content returned by the HttpServer.</param>
        public TestHttpServer(HttpListener listener, string pageData)
        {
            Listener = listener;
            PageData = pageData;
        }

        /// <summary>
        /// Handle connections, either server the page data as content or
        /// shutdown the server if cancellation has been requested.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task HandleIncomingConnections(CancellationToken token)
        {
            bool runServer = true;

            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (runServer)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await Listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                // If cancellation requested then shutdown server.
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine("Shutdown requested");
                    runServer = false;
                }

                // Write the response info
                byte[] data = Encoding.UTF8.GetBytes(PageData);
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                // Write out to the response stream (asynchronously), then close it
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
            }
        }
    }
}
