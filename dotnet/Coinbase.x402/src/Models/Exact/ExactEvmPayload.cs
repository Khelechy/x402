using Coinbase.x402.src.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coinbase.x402.src.Models.Exact
{
    public class ExactEvmPayload : IPaymentPayload
    {
        public string Signature { get; set; }
        public ExactEvmPayloadAuthorization Authorization { get; set; }
    }
}
