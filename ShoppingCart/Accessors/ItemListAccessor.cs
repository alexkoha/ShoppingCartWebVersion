using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ShoppingCart.Interfaces;
using ShoppingCart.Models;
using ShoppingCart.Resources;

namespace ShoppingCart.Accessors
{
    public class ItemListAccessor : IXmlAccessor
    {
        private int _countItemId;
        private readonly string _xmlFileName;
        private readonly string _imagePath;
        private readonly string _xmlPath;

        public ItemListAccessor()
        {
            _xmlPath = PathsInfo.XmlPath;
            _imagePath = PathsInfo.ImagesPath;
            _xmlFileName = "ItemsList.xml";
            _countItemId = 0;
        }

        public string GetFileName()
        {
            return _xmlFileName;
        }
        public string GetFilePath()
        {
            return _xmlPath;
        }

        public IDictionary<ItemKey, IEnumerable<Item>> GetItemDictionary()
        {
            var itemsDictionary = new Dictionary<ItemKey, IEnumerable<Item>>();

            var doc = XDocument.Load(_xmlPath + "\\" + _xmlFileName);
            var node = doc.Elements("Items");

            if(node==null)
                throw new Exception("Cant find 'items' elements");

            foreach (var item in node.Elements("Item"))
            {
                itemsDictionary.Add(new ItemKey()
                {
                    ProductName = item.Element("ItemName")?.Value,
                    ImageFilePath = _imagePath + item.Element("ImageFileName")?.Value,
                    UnitQty = item.Element("UnitQty")?.Value,
                    ItemId = _countItemId++,
                    Quantity = 0
                },
                (from chain in item.Elements("Chain")
                    select new Item
                    {
                        ProductName = item.Element("ItemName")?.Value,
                        ItemCode = chain.Element("ItemCode")?.Value,
                        ChainId = chain.Element("ChainID")?.Value
                    }).ToList());
            }
            return itemsDictionary;
        }
    }
}