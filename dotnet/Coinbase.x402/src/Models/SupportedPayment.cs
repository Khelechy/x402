using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coinbase.x402.src.Models
{
    public class SupportedPayment
    {
        public List<SupportedPaymentKind> Kinds { get; set; }
    }

    public class SupportedPaymentKind
    {
        public int X402Version { get; set; }

        public string Scheme { get; set; }

        public string Network { get; set; }

        public Dictionary<string, object> Extra { get; set; }
    }
}
