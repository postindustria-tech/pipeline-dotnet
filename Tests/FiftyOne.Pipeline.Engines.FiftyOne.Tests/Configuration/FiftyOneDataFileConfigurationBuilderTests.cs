using FiftyOne.Pipeline.Engines.FiftyOne.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FiftyOne.Pipeline.Engines.FiftyOne.Tests.Configuration
{
    [TestClass]
    public class FiftyOneDataFileConfigurationBuilderTests
    {
        /// <summary>
        /// Check that when a new data file config builder is created, license keys
        /// are not carried over from any previous instances.
        /// This can happen if the list is initialized from a static constant which
        /// is not explicitely copied to a new list.
        /// </summary>
        [TestMethod]
        public void UrlNotReused()
        {
            // Arrange
            var builder1 = new FiftyOneDataFileConfigurationBuilder();
            builder1.SetDataUpdateLicenseKey("some test key");

            // Act
            var builder2 = new FiftyOneDataFileConfigurationBuilder();
            var config2 = builder2.Build("some file", false);

            // Assert
            Assert.AreEqual(0, config2.DataUpdateLicenseKeys.Count);
        }
    }
}
