using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Coinbase.x402.src.Models.Core;

namespace Coinbase.x402.src.Models
{
    public class PaymentRequiredResponse
    {
        /// <summary>
        /// Protocol version identifier
        /// </summary>
        public int X402Version { get; set; }

        /// <summary>
        /// Human-readable error message explaining why payment is required
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Array of payment requirement objects defining acceptable payment methods
        /// </summary>
        public List<PaymentRequirements> Accepts { get; set; } = new List<PaymentRequirements>();

        /// <summary>
        /// Serialise and base64-encode for the X-PAYMENT-RESPONSE header.
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
                throw new InvalidOperationException("Unable to encode payment response header", e);
            }
        }

    }
}
