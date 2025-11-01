using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coinbase.x402.src.Models.Core
{
    public class PaymentRequirements
    {
        /// <summary>
        /// e.g. "exact"
        /// </summary>
        [Required]
        public string Scheme { get; set; }

        /// <summary>
        /// e.g. "base-sepolia"
        /// </summary>
        [Required]
        public string Network { get; set; }

        /// <summary>
        /// uint256 in wei / atomic units
        /// </summary>
        [Required]
        public string MaxAmountRequired { get; set; }

        /// <summary>
        /// URL path the client is paying for
        /// </summary>
        [Required]
        public string Resource { get; set; }

        /// <summary>
        /// Human-readable description of the resource
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Expected response MIME
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// Optional JSON schema
        /// </summary>
        public Dictionary<string, object> OutputSchema { get; set; }

        /// <summary>
        /// Address (EVM / Solana etc.)
        /// </summary>
        public string PayTo { get; set; }

        /// <summary>
        /// Maximum time allowed for payment completion
        /// </summary>
        [Required]
        public int MaxTimeoutSeconds { get; set; }

        /// <summary>
        /// Token contract address / symbol
        /// </summary>
        public string Asset { get; set; }

        /// <summary>
        /// Scheme-specific
        /// </summary>
        public Dictionary<string, object> Extra { get; set; }
    }
}
