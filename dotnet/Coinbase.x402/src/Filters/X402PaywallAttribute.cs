using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Coinbase.x402.src.Client.Interfaces;
using Coinbase.x402.src.Models;
using Coinbase.x402.src.Models.Core;

namespace Coinbase.x402.src.Filters
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class X402PaywallAttribute : Attribute, IAsyncActionFilter
    {
        private readonly PaymentRequirements _requirements;

        public X402PaywallAttribute(string scheme, string network, string maxAmountRequired, string resource, int maxTimeoutSeconds)
        {
            _requirements = new PaymentRequirements
            {
                Scheme = scheme,
                Network = network,
                MaxAmountRequired = maxAmountRequired,
                Resource = resource,
                MaxTimeoutSeconds = maxTimeoutSeconds
            };
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var facilitatorClient = context.HttpContext.RequestServices.GetRequiredService<IFacilitatorClient>();

            var paymentHeader = context.HttpContext.Request.Headers["X-PAYMENT"].FirstOrDefault();
            if (!string.IsNullOrEmpty(paymentHeader))
            {
                try
                {
                    var paymentPayload = PaymentPayload.DecodePaymentPayloadFromBase64(paymentHeader);
                    var verificationResponse = await facilitatorClient.VerifyAsync(paymentPayload, _requirements);

                    if (!verificationResponse.IsInvalid)
                    {
                        // Payment is valid, proceed to action
                        await next();
                        return;
                    }
                }
                catch (Exception)
                {
                    // If decoding or verification fails, treat as invalid
                }
            }

            // Payment invalid or missing, return 402
            var paymentRequiredResponse = new PaymentRequiredResponse
            {
                X402Version = 1,
                Error = "Payment required for this resource",
                Accepts = new List<PaymentRequirements> { _requirements }
            };

            var encodedResponse = paymentRequiredResponse.EncodeToBase64String();
            context.HttpContext.Response.Headers["X-PAYMENT-RESPONSE"] = encodedResponse;
            context.HttpContext.Response.StatusCode = 402;
            context.HttpContext.Response.ContentType = "application/json";
            await context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(paymentRequiredResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
            return;
        }
    }
}
