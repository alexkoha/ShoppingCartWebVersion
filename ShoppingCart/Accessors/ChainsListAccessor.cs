using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ShoppingCart.Interfaces;
using ShoppingCart.Models;
using ShoppingCart.Resources;

namespace ShoppingCart.Accessors
{
    public class ChainsListAccessor : IXmlAccessor
    {
        private readonly string _xmlFileName;
        private string _xmlPath;

        public ChainsListAccessor()
        {
            _xmlFileName = "ChainsList.xml";
            _xmlPath = PathsInfo.XmlPath;
        }

        public string GetFileName()
        {
            return _xmlFileName;
        }
        public string GetFilePath()
        {
            return _xmlPath;
        }

        public IEnumerable<Chain> GetChainsList()
        {
            var doc = XDocument.Load(_xmlPath + "\\" + _xmlFileName);
            var node = doc.Element("Chains");

            if (node != null)
                return node.Elements("Chain").
                    Select(item => 
                        new Chain(
                            item.Element("Id")?.Value,
                            item.Element("ChainId")?.Value, 
                            item.Element("ChainName")?.Value, 
                            item.Element("XmlFileName")?.Value,
                            item.Element("Stores")?.Element("StoreId")?.Value,
                            item.Element("SubchainId")?.Value
                            )).ToList();

            throw new Exception("Cant find Chains Element");
        }
    }
}