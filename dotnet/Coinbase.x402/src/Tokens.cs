using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coinbase.x402.src
{
    public class TokenInfo
    {
        public string HumanName { get; set; }
        public string Address { get; set; }
        public string Name { get; set; }
        public int Decimals { get; set; }
        public string Version { get; set; }
    }

    public static class TokenConfiguration
    {
        private static readonly Dictionary<string, string> NetworkToId = new Dictionary<string, string>
        {
            { "base-sepolia", "84532" },
            { "base", "8453" },
            { "avalanche-fuji", "43113" },
            { "avalanche", "43114" }
        };

        private static readonly Dictionary<string, List<TokenInfo>> KnownTokens = new Dictionary<string, List<TokenInfo>>
        {
            {
                "84532", new List<TokenInfo>
                {
                    new TokenInfo
                    {
                        HumanName = "usdc",
                        Address = "0x036CbD53842c5426634e7929541eC2318f3dCF7e",
                        Name = "USDC",
                        Decimals = 6,
                        Version = "2"
                    }
                }
            },
            {
                "8453", new List<TokenInfo>
                {
                    new TokenInfo
                    {
                        HumanName = "usdc",
                        Address = "0x833589fCD6eDb6E08f4c7C32D4f71b54bdA02913",
                        Name = "USD Coin", // needs to be exactly what is returned by name() on contract
                        Decimals = 6,
                        Version = "2"
                    }
                }
            },
            {
                "43113", new List<TokenInfo>
                {
                    new TokenInfo
                    {
                        HumanName = "usdc",
                        Address = "0x5425890298aed601595a70AB815c96711a31Bc65",
                        Name = "USD Coin",
                        Decimals = 6,
                        Version = "2"
                    }
                }
            },
            {
                "43114", new List<TokenInfo>
                {
                    new TokenInfo
                    {
                        HumanName = "usdc",
                        Address = "0xB97EF9Ef8734C71904D8002F8b6Bc66Dd9c48a6E",
                        Name = "USDC",
                        Decimals = 6,
                        Version = "2"
                    }
                }
            }
        };

        /// <summary>
        /// Get the chain ID for a given network.
        /// Supports string encoded chain IDs and human readable networks.
        /// </summary>
        public static string GetChainId(string network)
        {
            // Try to parse as integer - if successful, it's already a chain ID
            if (int.TryParse(network, out _))
            {
                return network;
            }

            // Look up in network mapping
            if (!NetworkToId.ContainsKey(network))
            {
                throw new ArgumentException($"Unsupported network: {network}");
            }

            return NetworkToId[network];
        }

        /// <summary>
        /// Get the token name for a given chain and address
        /// </summary>
        public static string GetTokenName(string chainId, string address)
        {
            if (!KnownTokens.ContainsKey(chainId))
            {
                throw new ArgumentException($"Chain ID {chainId} not found");
            }

            var token = KnownTokens[chainId].FirstOrDefault(t => t.Address == address);
            if (token == null)
            {
                throw new ArgumentException($"Token not found for chain {chainId} and address {address}");
            }

            return token.Name;
        }

        /// <summary>
        /// Get the token version for a given chain and address
        /// </summary>
        public static string GetTokenVersion(string chainId, string address)
        {
            if (!KnownTokens.ContainsKey(chainId))
            {
                throw new ArgumentException($"Chain ID {chainId} not found");
            }

            var token = KnownTokens[chainId].FirstOrDefault(t => t.Address == address);
            if (token == null)
            {
                throw new ArgumentException($"Token not found for chain {chainId} and address {address}");
            }

            return token.Version;
        }

        /// <summary>
        /// Get the token decimals for a given chain and address
        /// </summary>
        public static int GetTokenDecimals(string chainId, string address)
        {
            if (!KnownTokens.ContainsKey(chainId))
            {
                throw new ArgumentException($"Chain ID {chainId} not found");
            }

            var token = KnownTokens[chainId].FirstOrDefault(t => t.Address == address);
            if (token == null)
            {
                throw new ArgumentException($"Token not found for chain {chainId} and address {address}");
            }

            return token.Decimals;
        }

        /// <summary>
        /// Get the default token address for a given chain and token type
        /// </summary>
        public static string GetDefaultTokenAddress(string chainId, string tokenType = "usdc")
        {
            if (!KnownTokens.ContainsKey(chainId))
            {
                throw new ArgumentException($"Chain ID {chainId} not found");
            }

            var token = KnownTokens[chainId].FirstOrDefault(t => t.HumanName == tokenType);
            if (token == null)
            {
                throw new ArgumentException($"Token type '{tokenType}' not found for chain {chainId}");
            }

            return token.Address;
        }
    }
}
