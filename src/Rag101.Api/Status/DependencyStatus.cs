namespace Rag101.Api.Status;

public enum DependencyState
{
    Reachable,
    Unreachable,
    NotConfigured,
}

/// <summary>
/// The result of checking one dependency (MongoDB, Elasticsearch or the
/// language model). Detail is the server version when reachable and the
/// reason when unreachable.
/// </summary>
public record DependencyStatus(DependencyState State, string Detail)
{
    public static DependencyStatus Reachable(string detail) => new(DependencyState.Reachable, detail);

    public static DependencyStatus Unreachable(string reason) => new(DependencyState.Unreachable, reason);

    public static DependencyStatus NotConfigured() => new(DependencyState.NotConfigured, "");
}
