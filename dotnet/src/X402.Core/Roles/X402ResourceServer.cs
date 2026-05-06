using X402.Core.Protocol.V2;
using X402.Core.Roles.Errors;
using X402.Core.Roles.Hooks;

namespace X402.Core.Roles;

/// <summary>
/// Base implementation of the x402 ResourceServer role.
/// Provides handler registry, resource requirement management, and hook lifecycle.
/// </summary>
public class X402ResourceServer : IX402ResourceServer
{
  private readonly Dictionary<string, Func<PaymentPayload, Task<VerifyResponse>>> _verifiers = [];
  private readonly Dictionary<string, PaymentRequirements> _requirements = [];
  private IServerHooks? _hooks;

  public void RegisterSchemeVerifier(string scheme, Func<PaymentPayload, Task<VerifyResponse>> verifier)
  {
    _verifiers[scheme] = verifier;
  }

  public void Initialize(string resourcePath, PaymentRequirements requirements)
  {
    _requirements[resourcePath] = requirements;
  }

  public void RegisterHooks(IServerHooks hooks)
  {
    _hooks = hooks;
  }

  public async Task<VerifyResponse> VerifyPaymentAsync(PaymentPayload payload)
  {
    var context = new ServerHookContext { Payload = payload };

    try
    {
      // Execute before hooks
      if (_hooks != null)
      {
        await _hooks.BeforeVerifyAsync(context);
        if (context.ShouldAbort)
        {
          throw new VerifyError("ABORTED_BY_HOOK", "Verification aborted by before hook");
        }
      }

      // Lookup verifier for scheme
      if (!_verifiers.TryGetValue(payload.Accepted.Scheme, out var verifier))
      {
        throw new VerifyError("UNSUPPORTED_SCHEME", $"No verifier registered for scheme: {payload.Accepted.Scheme}");
      }

      // Verify payload
      context.VerifyResponse = await verifier(payload);

      // Execute after hooks
      if (_hooks != null)
      {
        await _hooks.AfterVerifyAsync(context);
      }

      return context.VerifyResponse;
    }
    catch (Exception ex)
    {
      context.Error = ex;
      if (_hooks != null)
      {
        await _hooks.OnVerifyFailureAsync(context);
      }

      if (ex is VerifyError)
      {
        throw;
      }

      throw new VerifyError("VERIFY_FAILED", ex.Message, null, ex.Message);
    }
  }

  public virtual Task<SettleResponse> SettlePaymentAsync(PaymentPayload payload)
  {
    // Subclasses should override to provide actual settlement logic
    throw new NotImplementedException("SettlePaymentAsync must be implemented by derived class");
  }

  public Task<PaymentRequirements?> GetRequirementsAsync(string resourcePath)
  {
    _requirements.TryGetValue(resourcePath, out var requirements);
    return Task.FromResult(requirements);
  }
}
