using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coinbase.x402.src
{
    /// <summary>
    /// Supported blockchain networks
    /// </summary>
    public enum SupportedNetworks
    {
        Base,
        BaseSepolia,
        AvalancheFuji,
        Avalanche
    }

    public static class NetworkConstants
    {
        /// <summary>
        /// Mapping of EVM network names to their chain IDs
        /// </summary>
        public static readonly Dictionary<string, int> EvmNetworkToChainId = new Dictionary<string, int>
        {
            { "base-sepolia", 84532 },
            { "base", 8453 },
            { "avalanche-fuji", 43113 },
            { "avalanche", 43114 }
        };

        /// <summary>
        /// Mapping of SupportedNetworks enum to chain IDs
        /// </summary>
        public static readonly Dictionary<SupportedNetworks, int> NetworkToChainId = new Dictionary<SupportedNetworks, int>
        {
            { SupportedNetworks.BaseSepolia, 84532 },
            { SupportedNetworks.Base, 8453 },
            { SupportedNetworks.AvalancheFuji, 43113 },
            { SupportedNetworks.Avalanche, 43114 }
        };

        /// <summary>
        /// Mapping of SupportedNetworks enum to string identifiers
        /// </summary>
        public static readonly Dictionary<SupportedNetworks, string> NetworkToString = new Dictionary<SupportedNetworks, string>
        {
            { SupportedNetworks.BaseSepolia, "base-sepolia" },
            { SupportedNetworks.Base, "base" },
            { SupportedNetworks.AvalancheFuji, "avalanche-fuji" },
            { SupportedNetworks.Avalanche, "avalanche" }
        };

        /// <summary>
        /// Get chain ID from network enum
        /// </summary>
        public static int GetChainId(SupportedNetworks network)
        {
            return NetworkToChainId[network];
        }

        /// <summary>
        /// Get chain ID from network string
        /// </summary>
        public static int GetChainId(string network)
        {
            if (!EvmNetworkToChainId.ContainsKey(network))
            {
                throw new ArgumentException($"Unsupported network: {network}");
            }
            return EvmNetworkToChainId[network];
        }

        /// <summary>
        /// Get network string from enum
        /// </summary>
        public static string GetNetworkString(SupportedNetworks network)
        {
            return NetworkToString[network];
        }
    }
}
