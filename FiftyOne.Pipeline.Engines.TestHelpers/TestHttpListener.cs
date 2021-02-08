using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace FiftyOne.Pipeline.Engines.TestHelpers
{
    /// <summary>
    /// Test HTTP Listener for serving static content, used in testing to mock
    /// a client website or provide a static response for a service.
    /// </summary>
    public static class TestHttpListener
    {
        /// <summary>
        /// Creates a simple HttpListener.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="token"></param>
        public static HttpListener SimpleListener(string url, CancellationToken token)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();

            // Handle requests
            Task listenTask = new TestHttpServer(listener).HandleIncomingConnections(token);
            listenTask.GetAwaiter();
            return listener;
        }

        /// <summary>
        /// Creates a simple HttpListener.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="pageData"></param>
        /// <param name="token"></param>
        public static HttpListener SimpleListener(string url, string pageData, CancellationToken token)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();

            // Handle requests
            Task listenTask = new TestHttpServer(listener, pageData).HandleIncomingConnections(token);
            listenTask.GetAwaiter();
            return listener;
        }

        /// <summary>
        /// Start a new TcpListener with the port as 0 so that the OS assigns an
        /// available port. Record this then close the listener. This ensures 
        /// that a 'randomly' generated port number is free.
        /// </summary>
        /// <returns></returns>
        public static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Any, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}
