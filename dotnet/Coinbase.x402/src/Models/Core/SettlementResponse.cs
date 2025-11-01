using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Coinbase.x402.src.Models.Core
{
    /// <summary>
    /// Settlement response header that gets base64-encoded into X-PAYMENT-RESPONSE.
    /// Matches the structure of Go SettleResponse and TypeScript SettleResponse.
    /// </summary>
    public class SettlementResponse
    {
        /// <summary>
        /// Whether the settlement was successful.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public bool Success { get; set; }

        /// <summary>
        /// Error reason if it fails.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string ErrorReason { get; set; }

        /// <summary>
        /// Transaction hash of the settled payment.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string Transaction { get; set; }

        /// <summary>
        /// Network ID where the settlement occurred.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string Network { get; set; }

        /// <summary>
        /// Wallet address of the person who made the payment (can be null).
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string Payer { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SettlementResponse() { }

        /// <summary>
        /// Constructor with all fields.
        /// </summary>
        public SettlementResponse(bool success, string transaction, string network, string payer)
        {
            Success = success;
            Transaction = transaction;
            Network = network;
            Payer = payer;
        }
    }
}
