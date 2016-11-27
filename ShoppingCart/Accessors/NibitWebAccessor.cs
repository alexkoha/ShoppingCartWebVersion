using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using HtmlAgilityPack;
using ShoppingCart.Models;

namespace ShoppingCart.Accessors
{
    public class NibitWebAccessor
    {
        private Url _urlPage;
        private readonly Url _urlMainPage;

        public NibitWebAccessor()
        {
            _urlMainPage = new Url("http://matrixcatalog.co.il/");
        }

        private IEnumerable<String> GetUrls(Chain chain)
        {
            HtmlWeb hw = new HtmlWeb();
            _urlPage = new Url(_urlMainPage.Value + $"NBCompetitionRegulations.aspx" +
                               $"?fileType=pricefull&code={chain.ChainId}{chain.SubchainId}{chain.StoreId}");

            HtmlDocument doc = hw.Load(_urlPage.Value);
            return doc.DocumentNode.Descendants("a")
                .Select(a => a.GetAttributeValue("href", null))
                .Where(url => !String.IsNullOrEmpty(url));
        }

        public string GetNewerXmlFileLink(Chain chain)
        {
            var listOfUrls = GetUrls(chain);
            var ofUrls = listOfUrls as IList<string> ?? listOfUrls.ToList();
            if (ofUrls.Any(url => !String.IsNullOrEmpty(url)) )
            {
                var urlXmlFile = ofUrls.Where(element=>element.Contains(chain.ChainId))
                    .Single(url=>url.Contains("CompetitionRegulationsFiles\\latest\\"))
                    .Replace("\\" , "/");
                return "http://matrixcatalog.co.il/"+ urlXmlFile;
            }
            return null;
        }

    }

}
