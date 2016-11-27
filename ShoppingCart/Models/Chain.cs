using System;

namespace ShoppingCart.Models
{
    public class Chain : IEquatable<Chain>
    {
        public Chain(string id,string chainId, string chainName, string chainXmlFileName, string storeId, string subchainId)
        {
            Id = id;
            ChainId = chainId;
            ChainName = chainName;
            ChainXmlFileName = chainXmlFileName;
            StoreId = storeId;
            SubchainId = subchainId;
        }

        public string Id { get; }
        public string ChainId { get; }
        public string SubchainId { get; }
        public string ChainName { get; }
        public string ChainXmlFileName { get; }
        public string StoreId { get; }

        public bool Equals(Chain other)
        {
            return (ChainId == other.ChainId || Id==other.Id);
        }
        public override bool Equals(object obj)
        {
            var other = obj as Chain;
            return other != null && (ChainId == other.ChainId || Id == other.Id);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}