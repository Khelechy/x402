using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coinbase.x402.src
{
    public class FacilitatorConfig
    {
        public string URL { get; set; }
        public TimeSpan? Timeout { get; set; }
        public Func<Dictionary<string, Dictionary<string, string>>> CreateAuthHeaders { get; set; }
    }
}
