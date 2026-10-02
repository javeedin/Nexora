namespace Nexora.BuildingBlocks.Idempotency;

/// <summary>A response captured for replay.</summary>
/// <param name="StatusCode">HTTP status.</param>
/// <param name="ContentType">Content type, if any.</param>
/// <param name="Body">Response body.</param>
public sealed record StoredResponse(int StatusCode, string? ContentType, byte[] Body);

/// <summary>Outcome of claiming an idempotency key.</summary>
public enum ClaimOutcome
{
    /// <summary>First use: the caller must execute the request, then complete or release the key.</summary>
    Claimed,

    /// <summary>Same key and request still executing elsewhere.</summary>
    InProgress,

    /// <summary>Same key used for a different request.</summary>
    Mismatch,

    /// <summary>Already executed: replay <see cref="ClaimResult.Response"/>.</summary>
    Completed,
}

/// <summary>Result of <see cref="IIdempotencyStore.ClaimAsync"/>.</summary>
/// <param name="Outcome">What happened.</param>
/// <param name="Response">The stored response when <see cref="ClaimOutcome.Completed"/>.</param>
public sealed record ClaimResult(ClaimOutcome Outcome, StoredResponse? Response = null);

/// <summary>Atomic storage for idempotency keys. Keys are already tenant-scoped by the caller.</summary>
public interface IIdempotencyStore
{
    /// <summary>Atomically claims <paramref name="key"/> for a request with <paramref name="fingerprint"/>.</summary>
    Task<ClaimResult> ClaimAsync(string key, string fingerprint, TimeSpan lockFor, CancellationToken cancellationToken);

    /// <summary>Stores the response of a claimed key for replay.</summary>
    Task CompleteAsync(string key, string fingerprint, StoredResponse response, TimeSpan keepFor, CancellationToken cancellationToken);

    /// <summary>Frees a claimed key (the request failed and may be retried).</summary>
    Task ReleaseAsync(string key, CancellationToken cancellationToken);
}
