using Coinbase.x402.src;
using Coinbase.x402.src.Client;
using Coinbase.x402.src.Client.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coinbase.x402
{
    public static class X402Extension
    {

        public static IServiceCollection AddX402(this IServiceCollection services, Action<FacilitatorConfig> configure)
        {
            var config = new FacilitatorConfig();
            configure?.Invoke(config);

            services.AddHttpClient();
            services.AddSingleton(config);

            services.AddSingleton<IFacilitatorClient>(sp => new FacilitatorClient(sp.GetRequiredService<FacilitatorConfig>()));

            return services;
        }

    }
}
