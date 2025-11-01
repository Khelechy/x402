using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coinbase.x402.src.Models
{
    public class VerificationResponse
    {
        public bool IsInvalid { get; set; }
        public string InvalidReason { get; set; }
        public string Payer { get; set; }
    }
}
