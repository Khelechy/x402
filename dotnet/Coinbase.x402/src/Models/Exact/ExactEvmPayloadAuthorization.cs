using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coinbase.x402.src.Models.Exact
{
    public class ExactEvmPayloadAuthorization
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Value { get; set; }
        public string ValidAfter { get; set; }
        public string ValidBefore { get; set; }
        public string Nonce { get; set; }
    }
}
