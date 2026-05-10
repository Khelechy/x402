# x402 .NET SDK

.NET implementation of the [x402 protocol](https://x402.org) — HTTP 402 Payment Required with
EVM cryptocurrency micropayments.

> **Status: work-in-progress.** Core, ASP.NET Core middleware, and EVM Exact mechanism are
> implemented and tested. SVM mechanism, extensions, and NuGet packaging are upcoming.

---

## What is x402?

x402 lets an HTTP server require a cryptocurrency payment before serving a response.
The standard flow:

1. Client requests a resource → server replies `402 Payment Required` + payment details in header.
2. Client creates a signed payment payload and retries with an `X-PAYMENT` header.
3. Server (or a facilitator) verifies the signature off-chain, serves the resource, then settles on-chain.

---

## Packages

| Package | Target | Description |
|---|---|---|
| `X402.Core` | `net8.0`, `net10.0` | Protocol models, roles, HTTP header codec, hooks |
| `X402.AspNetCore` | `net8.0`, `net10.0` | ASP.NET Core middleware and DI extensions |
| `X402.Mechanisms.Evm` | `net8.0`, `net10.0` | EVM Exact mechanism — EIP-712 + EIP-3009 off-chain verifier |

---

## Quick Start

### Resource server (ASP.NET Core)

```csharp
// Program.cs
using X402.AspNetCore;
using X402.Core.Protocol.V2;
using X402.Mechanisms.Evm;
using X402.Mechanisms.Evm.Exact;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddX402();           // registers IX402ResourceServer, IX402Client, IX402Facilitator

var app = builder.Build();

var usdcOnBase = new PaymentRequirements(
    Scheme:            EvmSchemes.Exact,
    Network:           EvmChains.Base,
    Asset:             "0x833589fcd6edb6e08f4c7c32d4f71b54bda02913",  // USDC on Base
    Amount:            "1000000",                                        // 1 USDC (6 decimals)
    PayTo:             "0xYourWalletAddress",
    MaxTimeoutSeconds: 300,
    Extra:             System.Text.Json.Nodes.JsonNode.Parse(
                           """{"name":"USD Coin","version":"2"}""")
                       as System.Text.Json.Nodes.JsonObject
);

// register the off-chain verifier for evm-exact
var server = app.Services.GetRequiredService<IX402ResourceServer>();
server.RegisterSchemeVerifier(EvmSchemes.Exact, EvmExactVerifier.Create());

app.UseX402Payment(opts =>
    opts.Protect("/api/premium", usdcOnBase));

app.MapGet("/api/premium/data", () => "You paid!");

app.Run();
```

### Facilitator-backed verification via `AddX402(...)`

If you want verification to delegate to an external facilitator instead of the local verifier,
configure it at DI registration time:

```csharp
using X402.AspNetCore;
using X402.Mechanisms.Evm;

builder.Services.AddX402(options =>
{
    options
        .UseHttpFacilitator("https://x402.org/facilitator")
        .RegisterFacilitatedScheme(EvmSchemes.Exact);
});
```

This keeps the ASP.NET Core integration the same (`UseX402Payment(...)`, `.RequireX402Payment(...)`,
or `[RequireX402Payment(...)]`) while switching verification for the registered scheme to the
configured facilitator client.

### Route annotations (usability)

You can now protect routes where they are declared instead of only using path-prefix
registration in middleware options.

Minimal API endpoint annotation:

```csharp
app.MapGet("/api/premium/inline", () => "paid endpoint")
   .RequireX402Payment(usdcOnBase);
```

MVC controller/action attribute:

```csharp
using X402.AspNetCore;
using X402.Mechanisms.Evm;

[ApiController]
[Route("api/[controller]")]
public sealed class PremiumController : ControllerBase
{
    [HttpGet("report")]
    [RequireX402Payment(
        scheme: EvmSchemes.Exact,
        network: EvmChains.Base,
        asset: "0x833589fcd6edb6e08f4c7c32d4f71b54bda02913",
        amount: "1000000",
        payTo: "0xYourWalletAddress",
        tokenName: "USD Coin",
        tokenVersion: "2")]
    public IActionResult GetReport() => Ok("paid report");
}
```

This is additive with middleware route maps. If one layer has already verified payment,
the other layer skips duplicate verification for the same request.

### Facilitator URL configuration

You can set a facilitator URL in middleware route policy so each protected route can carry
where verification/settlement should be delegated.

Default for all protected routes:

```csharp
app.UseX402Payment(opts =>
{
    opts.DefaultFacilitatorUrl = "https://facilitator.example.com";
    opts.Protect("/api/premium", usdcOnBase);
});
```

Per-route override:

```csharp
app.UseX402Payment(opts =>
{
    opts.DefaultFacilitatorUrl = "https://facilitator.example.com";

    opts.Protect("/api/premium", usdcOnBase);
    opts.Protect(
        path: "/api/enterprise",
        requirements: usdcOnBase,
        facilitatorUrl: "https://enterprise-facilitator.example.com");
});
```

How this works today:

- `DefaultFacilitatorUrl` is a global fallback in route options.
- `facilitatorUrl` on `Protect(...)` is a route-level override.
- Middleware and route annotations verify locally unless you configure facilitated schemes via `AddX402(...)`.
- If you want to call an external facilitator endpoint directly, use `HttpFacilitatorTransport` and pass the same URL to `VerifyViaHttpAsync(...)` / `SettleViaHttpAsync(...)`.

Direct facilitator call example:

```csharp
using X402.Core.Protocol.V2;
using X402.Core.Transport.Http.Adapters;

var facilitatorUrl = "https://facilitator.example.com";

// Usually this comes from PAYMENT-RESPONSE header decoding.
PaymentPayload payload = /* your decoded payload */;

using var httpClient = new HttpClient();
var transport = new HttpFacilitatorTransport(httpClient);

// 1) Verify with external facilitator
var verify = await transport.VerifyViaHttpAsync(payload, facilitatorUrl);
if (!verify.IsValid)
{
    Console.WriteLine($"Verification failed: {verify.InvalidReason}");
    return;
}

// 2) Settle with external facilitator
var settle = await transport.SettleViaHttpAsync(payload, facilitatorUrl);
if (!settle.Success)
{
    Console.WriteLine($"Settlement failed: {settle.ErrorReason}");
    return;
}

Console.WriteLine($"Settled tx: {settle.Transaction} on {settle.Network}");
```

Tip: if you store `facilitatorUrl` in route policy (`DefaultFacilitatorUrl` or per-route `facilitatorUrl`),
reuse that same value when you instantiate and call `HttpFacilitatorTransport`.

### Verifying a payment manually

```csharp
using X402.Core.Transport.Http;
using X402.Core.Protocol.V2;
using X402.Mechanisms.Evm.Exact;

// Decode the X-PAYMENT header from the incoming request
var payload = HeaderCodec.Decode<PaymentPayload>(request.Headers["X-PAYMENT"]);

var result = EvmExactVerifier.Verify(payload);
if (!result.IsValid)
    Console.WriteLine($"Rejected: {result.InvalidReason}");
else
    Console.WriteLine($"Accepted — payer: {result.Payer}");
```

---

## Architecture

```
┌──────────────────────────────────────────────┐
│              Your Application                │
└──────────────────────────────────────────────┘
              │
   ┌──────────┼──────────┐
   ▼          ▼          ▼
[Client]  [Server]  [Facilitator]          X402.Core — framework-agnostic
   │          │          │
   └──────────┴──────────┘
              │
              ▼
┌──────────────────────────────────────────────┐
│     ASP.NET Core Middleware (optional)       │  X402.AspNetCore
│   UseX402Payment() · AddX402()               │
└──────────────────────────────────────────────┘
              │
              ▼
┌──────────────────────────────────────────────┐
│         Mechanisms (pluggable)               │
│   X402.Mechanisms.Evm — evm-exact            │
│   X402.Mechanisms.Svm — svm-exact (planned)  │
└──────────────────────────────────────────────┘
```

### Key design principles

- **Framework-agnostic core** — `X402.Core` has zero framework dependencies; works with any host.
- **Pluggable mechanisms** — register any scheme via `RegisterSchemeVerifier(scheme, handler)`.
- **Transport layer** — `HeaderCodec` handles `X-PAYMENT` / `X-PAYMENT-RESPONSE` Base64-JSON encoding.
- **Lifecycle hooks** — attach `IServerHooks` / `IClientHooks` / `IFacilitatorHooks` for logging, metrics, and custom logic without modifying core behaviour.

---

## Project layout

```
dotnet/
├── X402.slnx
├── src/
│   ├── X402.Core/                  Protocol models (V1/V2), roles, header codec, hooks
│   ├── X402.AspNetCore/            Middleware, DI extensions
│   └── X402.Mechanisms.Evm/        EIP-712 hasher, ECDSA verifier, evm-exact verifier
└── tests/
    ├── X402.Core.Tests/            61 tests
    ├── X402.AspNetCore.Tests/       9 tests
    └── X402.Mechanisms.Evm.Tests/  11 tests
```

---

## Build & test

From the `dotnet/` directory:

```bash
dotnet build X402.slnx
dotnet test  X402.slnx
```

- `X402.Core` and `X402.AspNetCore` multi-target `net8.0` and `net10.0`.
- Tests target `net10.0`.
- `X402.Mechanisms.Evm` depends on [Nethereum](https://nethereum.com) for EIP-712 ABI encoding and secp256k1 ECDSA recovery.

---

## Supported networks & tokens

Any EVM chain is supported via CAIP-2 identifiers. Well-known constants are in `EvmChains`:

| Constant | CAIP-2 |
|---|---|
| `EvmChains.Mainnet` | `eip155:1` |
| `EvmChains.Base` | `eip155:8453` |
| `EvmChains.BaseSepolia` | `eip155:84532` |
| `EvmChains.Optimism` | `eip155:10` |
| `EvmChains.ArbitrumOne` | `eip155:42161` |
| `EvmChains.Sepolia` | `eip155:11155111` |

---

## Roadmap

- [x] Phase 0 — solution scaffolding
- [x] Phase 1 — protocol models (V1/V2), version detection, header codec
- [x] Phase 2 — core roles (Client / ResourceServer / Facilitator) + lifecycle hooks
- [x] Phase 3 — HTTP transport adapters
- [x] Phase 4 — ASP.NET Core middleware (`UseX402Payment`, `AddX402`)
- [x] Phase 5 — EVM Exact mechanism (EIP-712 hashing, EIP-3009 ECDSA verification)
- [ ] Phase 6 — Extensions (payment identifier, EIP-2612 gas sponsoring, offer/receipt)
- [ ] Phase 7 — SVM (Solana) mechanism
- [ ] Phase 8 — Integration / e2e test harness
- [ ] Phase 9 — NuGet packaging and release