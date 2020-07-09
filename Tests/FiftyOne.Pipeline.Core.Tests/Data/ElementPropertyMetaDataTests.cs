using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.Pipeline.Core.Tests.Data
{
    [TestClass]
    public class ElementPropertyMetaDataTests
    {
        private Mock<IFlowElement> _flowElement;

        [TestInitialize]
        public void Init()
        {
            _flowElement = new Mock<IFlowElement>();
        }

        /// <summary>
        /// Check that an property meta data instance with a null
        /// 'item properties' list will result in a null item
        /// property dictionary without any errors.
        /// </summary>
        [TestMethod]
        public void ElementPropertyMetaData_ItemPropertiesDictionaryNull()
        {
            ElementPropertyMetaData metaData =
                new ElementPropertyMetaData(_flowElement.Object, "test", typeof(string), true, "", null);

            Assert.IsNull(metaData.ItemPropertyDictionary);
        }

        /// <summary>
        /// Check that the dictionary returned by the 
        /// 'ItemPropertyDictionary' property is case insensitive.
        /// </summary>
        [TestMethod]
        public void ElementPropertyMetaData_ItemPropertiesDictionary()
        {
            List<IElementPropertyMetaData> itemProperties = new List<IElementPropertyMetaData>()
            {
                new ElementPropertyMetaData(_flowElement.Object, "prop1", typeof(string), true, "", null),
                new ElementPropertyMetaData(_flowElement.Object, "PROP2", typeof(string), true, "", null)
            };

            ElementPropertyMetaData metaData =
                new ElementPropertyMetaData(_flowElement.Object, "test", typeof(string), true, "", itemProperties);

            var dict = metaData.ItemPropertyDictionary;

            Assert.IsNotNull(dict);
            Assert.IsTrue(dict.ContainsKey("prop1"));
            Assert.IsTrue(dict.ContainsKey("prop2"));
            Assert.IsTrue(dict.ContainsKey("PROP1"));
            Assert.IsTrue(dict.ContainsKey("PROP2"));
        }

    }
}
