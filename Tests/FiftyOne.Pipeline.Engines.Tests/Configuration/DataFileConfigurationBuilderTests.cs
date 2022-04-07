using FiftyOne.Pipeline.Engines.Configuration;
using FiftyOne.Pipeline.Engines.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.Pipeline.Engines.Tests.Configuration
{
    [TestClass]
    public class DataFileConfigurationBuilderTests
    {
        DataFileConfigurationBuilder _configBuilder = 
            new DataFileConfigurationBuilder();

        private class MyFormatter : IDataUpdateUrlFormatter
        {
            public Uri GetFormattedDataUpdateUri(IAspectEngineDataFile dataFile) { return null; }

            public string GetFormattedDataUpdateUrl(IAspectEngineDataFile dataFile) { return null; }
        }

        /// <summary>
        /// Ensure that setting the license key to null will also 
        /// disable the 'automatic update' and 'update on startup'
        /// flags.
        /// </summary>
        [TestMethod]
        public void LicenseKey_Null()
        {
            // Confirm the default state of auto update and 
            // update on startup.
            var initialState = _configBuilder.Build("test", true);

            Assert.IsTrue(initialState.AutomaticUpdatesEnabled,
                "Auto updates should be enabled by default");
            Assert.IsFalse(initialState.UpdateOnStartup, 
                "Update on startup should be disabled by default");

            // Enable update on startup then set the license key
            // to null before building another configuration
            // object.
            _configBuilder.SetUpdateOnStartup(true);
            _configBuilder.SetDataUpdateLicenseKey(null);
            var result = _configBuilder.Build("test", true);

            // Both features should now be disabled.
            Assert.AreEqual(0, result.DataUpdateLicenseKeys.Count);
            Assert.IsFalse(result.AutomaticUpdatesEnabled,
                "Auto updates should be disabled");
            Assert.IsFalse(result.UpdateOnStartup,
                "Update on startup should be disabled");
        }

        /// <summary>
        /// Check that the configuration methods for setting the 
        /// URL formatter are working as intended.
        /// </summary>
        [TestMethod]
        public void VerifyFormatter()
        {
            // Confirm the default state of auto update and 
            // update on startup.
            var initialState = _configBuilder.Build("test", true);

            Assert.IsNull(initialState.UrlFormatter,
                "Url formatter should be null by default");

            // Enable update on startup then set the license key
            // to null before building another configuration
            // object.
            _configBuilder.SetDataUpdateUrlFormatter(new MyFormatter());
            var result = _configBuilder.Build("test", true);

            Assert.IsInstanceOfType(result.UrlFormatter, typeof(MyFormatter),
                "Url formatter should be of the type specified");

            // Set 'use formatter' to true. This shouldn't change anything.
            _configBuilder.SetDataUpdateUseUrlFormatter(true);
            result = _configBuilder.Build("test", true);

            Assert.IsInstanceOfType(result.UrlFormatter, typeof(MyFormatter),
                "Url formatter should be of the type specified");

            // Finally, set 'use formatter' to false. This should
            // set the formatter to null.
            _configBuilder.SetDataUpdateUseUrlFormatter(false);
            result = _configBuilder.Build("test", true);

            Assert.IsNull(result.UrlFormatter,
                "Url formatter should be null after calling " +
                "SetDataUpdateUseUrlFormatter(false)");
        }
    }
}
