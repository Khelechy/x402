using Microsoft.Extensions.DependencyInjection;
using X402.Core.Roles;

namespace X402.AspNetCore;

/// <summary>
/// Extension methods for registering x402 services with the DI container.
/// </summary>
public static class X402ServiceExtensions
{
  /// <summary>
  /// Registers <see cref="IX402ResourceServer"/>, <see cref="IX402Client"/>, and
  /// <see cref="IX402Facilitator"/> as singleton services.
  /// Call this once from <c>Program.cs</c> before <c>app.UseX402Payment()</c>.
  /// </summary>
  public static IServiceCollection AddX402(this IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);

    services.AddSingleton<IX402ResourceServer, X402ResourceServer>();
    services.AddSingleton<IX402Client, X402Client>();
    services.AddSingleton<IX402Facilitator, X402Facilitator>();
    services.AddSingleton<X402PaymentEnforcer>();

    return services;
  }
}
