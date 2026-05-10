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

var weatherRequirements = new PaymentRequirements(
    Scheme: EvmSchemes.Exact,
    Network: EvmChains.BaseSepolia,
    Asset: "0x833589fcd6edb6e08f4c7c32d4f71b54bda02913",
    Amount: "10000",
    PayTo: "0x1111111111111111111111111111111111111111",
    MaxTimeoutSeconds: 300,
    Extra: new JsonObject
    {
      ["name"] = "USD Coin",
      ["version"] = "2"
    });

var server = app.Services.GetRequiredService<IX402ResourceServer>();
server.RegisterSchemeVerifier(EvmSchemes.Exact, EvmExactVerifier.Create());

app.MapGet("/", () => Results.Ok(new
{
  example = "minimal-api-example",
  protectedRoute = "/weather"
}));

app.MapGet("/weather", (HttpContext context) =>
{
  var payer = context.Items.TryGetValue(X402HttpContextKeys.Payer, out var value)
      ? value?.ToString()
      : null;

  return Results.Ok(new
  {
    city = "Lagos",
    forecast = "clear",
    temperatureC = 29,
    paidBy = payer,
    protection = "minimal-api"
  });
})
.RequireX402Payment(weatherRequirements);

app.Run();