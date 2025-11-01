using Coinbase.x402.src.Models;
using Coinbase.x402.src.Models.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coinbase.x402.src.Client.Interfaces
{
    public interface IFacilitatorClient
    {
        /// <summary>
        /// Sends a payment verification request to the facilitator
        /// </summary>
        Task<VerificationResponse> VerifyAsync(PaymentPayload payload, PaymentRequirements requirements);

        /// <summary>
        /// Sends a payment settlement request to the facilitator
        /// </summary>
        Task<SettlementResponse> SettleAsync(PaymentPayload payload, PaymentRequirements requirements);

        /// <summary>
        /// Gets supported payment schemes and networks from the facilitator
        /// </summary>
        Task<SupportedPayment> GetSupportedAsync();

    }
}
