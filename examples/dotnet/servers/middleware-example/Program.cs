using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using X402.AspNetCore;
using X402.Core.Protocol.V2;
using X402.Core.Roles;
using X402.Mechanisms.Evm;
using X402.Mechanisms.Evm.Exact;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddX402();

var app = builder.Build();

var basicRequirements = new PaymentRequirements(
    Scheme: EvmSchemes.Exact,
    Network: EvmChains.BaseSepolia,
    Asset: "0x833589fcd6edb6e08f4c7c32d4f71b54bda02913",
    Amount: "1000",
    PayTo: "0x3333333333333333333333333333333333333333",
    MaxTimeoutSeconds: 300,
    Extra: new JsonObject
    {
      ["name"] = "USD Coin",
      ["version"] = "2"
    });

var premiumRequirements = new PaymentRequirements(
    Scheme: EvmSchemes.Exact,
    Network: EvmChains.BaseSepolia,
    Asset: "0x833589fcd6edb6e08f4c7c32d4f71b54bda02913",
    Amount: "10000",
    PayTo: "0x4444444444444444444444444444444444444444",
    MaxTimeoutSeconds: 300,
    Extra: new JsonObject
    {
      ["name"] = "USD Coin",
      ["version"] = "2"
    });

var server = app.Services.GetRequiredService<IX402ResourceServer>();
server.RegisterSchemeVerifier(EvmSchemes.Exact, EvmExactVerifier.Create());

app.UseX402Payment(options =>
{
  options.DefaultFacilitatorUrl = "https://facilitator.example.com";
  options.Protect("/api/basic", basicRequirements);
  options.Protect("/api/premium", premiumRequirements, facilitatorUrl: "https://premium-facilitator.example.com");
});

app.MapGet("/", () => Results.Ok(new
{
  example = "middleware-example",
  protectedRoutes = new[] { "/api/basic", "/api/premium" },
  unprotectedRoute = "/unprotected"
}));

app.MapGet("/unprotected", () => Results.Ok(new
{
  route = "/unprotected",
  paymentRequired = false
}));

app.MapGet("/api/basic", () => Results.Ok(new
{
  route = "/api/basic",
  tier = "basic",
  protection = "middleware"
}));

app.MapGet("/api/premium", () => Results.Ok(new
{
  route = "/api/premium",
  tier = "premium",
  protection = "middleware"
}));

app.Run();