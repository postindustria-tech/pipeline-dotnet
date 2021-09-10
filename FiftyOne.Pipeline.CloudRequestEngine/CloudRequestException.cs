using FiftyOne.Pipeline.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.Pipeline.CloudRequestEngine
{
    /// <summary>
    /// Contains details about exceptions that occur when making 
    /// requests to the cloud service.
    /// </summary>
    public class CloudRequestException : PipelineException
    {
        private Dictionary<string, string> _responseHeaders;

        /// <summary>
        /// The HTTP status code from the response.
        /// </summary>
        public int HttpStatusCode { get; private set; }

        /// <summary>
        /// All HTTP headers that were present in the response.
        /// </summary>
        public IReadOnlyDictionary<string, string> ResponseHeaders => _responseHeaders;

        /// <summary>
        /// Constructor
        /// </summary>
        public CloudRequestException()
        {
            _responseHeaders = new Dictionary<string, string>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        public CloudRequestException(string message) : base(message)
        {
            _responseHeaders = new Dictionary<string, string>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public CloudRequestException(string message, Exception innerException) : base(message, innerException)
        {
            _responseHeaders = new Dictionary<string, string>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="httpStatusCode"></param>
        /// <param name="responseHeaders"></param>
        public CloudRequestException(string message, 
            int httpStatusCode, 
            Dictionary<string, string> responseHeaders) : base(message)
        {
            HttpStatusCode = httpStatusCode;
            _responseHeaders = responseHeaders;
        }
    }
}
