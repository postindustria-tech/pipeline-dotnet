using FiftyOne.Pipeline.Engines.Configuration;
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
    }
}
