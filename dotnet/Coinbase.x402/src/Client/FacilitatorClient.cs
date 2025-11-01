using Coinbase.x402.src.Client.Interfaces;
using Coinbase.x402.src.Models;
using Coinbase.x402.src.Models.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Coinbase.x402.src.Client
{
    public class FacilitatorClient : IFacilitatorClient
    {
        public const string DefaultFacilitatorURL = "https://x402.org/facilitator";
        public string URL { get; set; }
        public HttpClient HTTPClient { get; set; }
        public Func<Dictionary<string, Dictionary<string, string>>> CreateAuthHeaders { get; set; }

        public FacilitatorClient(FacilitatorConfig config = null)
        {
            if (config == null)
            {
                config = new FacilitatorConfig
                {
                    URL = DefaultFacilitatorURL
                };
            }

            var httpClient = new HttpClient();
            if (config.Timeout.HasValue)
            {
                httpClient.Timeout = config.Timeout.Value;
            }

            URL = config.URL;
            HTTPClient = httpClient;
            CreateAuthHeaders = config.CreateAuthHeaders;
        }


        /// <summary>
        /// Sends a payment verification request to the facilitator
        /// </summary>
        public async Task<VerificationResponse> VerifyAsync(PaymentPayload payload, PaymentRequirements requirements)
        {
            var reqBody = new
            {
                x402Version = 1,
                paymentPayload = payload,
                paymentRequirements = requirements
            };

            var jsonBody = JsonSerializer.Serialize(reqBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{URL}/verify")
            {
                Content = content
            };

            // Add auth headers if available
            if (CreateAuthHeaders != null)
            {
                var headers = CreateAuthHeaders();
                if (headers.ContainsKey("verify"))
                {
                    foreach (var header in headers["verify"])
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
            }

            var response = await HTTPClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to verify payment: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var verifyResp = JsonSerializer.Deserialize<VerificationResponse>(responseContent);

            return verifyResp;
        }

        /// <summary>
        /// Sends a payment settlement request to the facilitator
        /// </summary>
        public async Task<SettlementResponse> SettleAsync(PaymentPayload payload, PaymentRequirements requirements)
        {
            var reqBody = new
            {
                x402Version = 1,
                paymentPayload = payload,
                paymentRequirements = requirements
            };

            var jsonBody = JsonSerializer.Serialize(reqBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{URL}/settle")
            {
                Content = content
            };

            // Add auth headers if available
            if (CreateAuthHeaders != null)
            {
                var headers = CreateAuthHeaders();
                if (headers.ContainsKey("settle"))
                {
                    foreach (var header in headers["settle"])
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
            }

            var response = await HTTPClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to settle payment: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var settleResp = JsonSerializer.Deserialize<SettlementResponse>(responseContent);

            return settleResp;
        }

        public async Task<SupportedPayment> GetSupportedAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{URL}/supported");


            var response = await HTTPClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to get supported payment scheme: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var supported = JsonSerializer.Deserialize<SupportedPayment>(responseContent);

            return supported;
        }
    }
}
