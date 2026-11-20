using System.Diagnostics;

namespace Rag101.Api.Bootstrap;

/// <summary>
/// Stores the connection string in the macOS Keychain using the system tool
/// /usr/bin/security. You can run the same commands in a terminal:
///   security find-generic-password -s rag101 -a mongodb-connection -w
/// </summary>
public class MacKeychainCredentialStore : ICredentialStore
{
    private const string Service = "rag101";
    private const string Account = "mongodb-connection";

    public bool IsSupported => true;

    public string? Read()
    {
        var (exitCode, output) = RunSecurity("find-generic-password", "-s", Service, "-a", Account, "-w");

        // A non-zero exit code means the item does not exist.
        return exitCode == 0 ? output.Trim() : null;
    }

    public void Write(string connectionString)
    {
        // -U updates the item when it already exists.
        var (exitCode, _) = RunSecurity(
            "add-generic-password", "-U", "-s", Service, "-a", Account, "-w", connectionString);

        if (exitCode != 0)
        {
            throw new InvalidOperationException("The macOS Keychain refused to store the connection string.");
        }
    }

    private static (int ExitCode, string Output) RunSecurity(params string[] arguments)
    {
        var startInfo = new ProcessStartInfo("/usr/bin/security")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)!;
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, output);
    }
}
