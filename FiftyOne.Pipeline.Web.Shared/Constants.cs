using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.Pipeline.Web.Shared
{
    /// <summary>
    /// Static class containing various constants that are used by the 
    /// Pipeline web integration and/or are helpful to callers.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming",
        "CA1707:Identifiers should not contain underscores",
        Justification = "51Degrees coding style is for constant names " +
            "to be all-caps with an underscore to separate words.")]
    public static class Constants
    {
        /// <summary>
        /// The Content type that indicates the body contains a 
        /// URL encoded form.
        /// </summary>
        public static readonly string[] CONTENT_TYPE_FORM = 
        {
            "application/x-www-form-urlencoded",
            "multipart/form-data"
        };

        /// <summary>
        /// The HTTP method indicating this request was a POST.
        /// </summary>
        public const string METHOD_POST = "POST";
    }
}
