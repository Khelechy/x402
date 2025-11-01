using Coinbase.x402.src.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Coinbase.x402.src.Models.Core
{
    public class PaymentPayload
    {
        /// <summary>
        /// Protocol version identifier (must be 1)
        /// </summary>
        public int X402Version { get; set; }

        /// <summary>
        /// Payment scheme identifier (e.g., "exact")
        /// </summary>
        public string Scheme { get; set; }

        /// <summary>
        /// Blockchain network identifier (e.g., "base-sepolia", "ethereum-mainnet")
        /// </summary>
        public string Network { get; set; }

        /// <summary>
        /// Payment data object
        /// </summary>
        public IPaymentPayload Payload { get; set; }

        /// <summary>
        /// Serialise and base64-encode for the X-PAYMENT header.
        /// </summary>
        public string EncodeToBase64String()
        {
            try
            {
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                return Convert.ToBase64String(bytes);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Unable to encode payment header", e);
            }
        }

        /// <summary>
        /// Decode from the header.
        /// </summary>
        public static PaymentPayload DecodePaymentPayloadFromBase64(string encodedPaymentPayload)
        {
            try
            {
                byte[] decoded = Convert.FromBase64String(encodedPaymentPayload);
                string json = Encoding.UTF8.GetString(decoded);
                return JsonSerializer.Deserialize<PaymentPayload>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Unable to decode payment header", e);
            }
        }
    }
}
