using System.Runtime.InteropServices;
using System.Text;

namespace Rag101.Api.Bootstrap;

/// <summary>
/// Stores the connection string in the Windows Credential Manager as a
/// "generic credential". You can see it in Control Panel, Credential Manager,
/// Windows Credentials, under the name rag101:mongodb-connection.
/// Windows has no command-line tool that reads a secret back, so this class
/// calls the Windows API directly.
/// </summary>
public class WindowsCredentialStore : ICredentialStore
{
    private const string TargetName = "rag101:mongodb-connection";
    private const int GenericCredential = 1;
    private const int PersistOnThisMachine = 2;

    public bool IsSupported => true;

    public string? Read()
    {
        if (!CredRead(TargetName, GenericCredential, 0, out var credentialPointer))
        {
            return null;
        }

        try
        {
            var credential = Marshal.PtrToStructure<Credential>(credentialPointer);
            var bytes = new byte[credential.CredentialBlobSize];
            Marshal.Copy(credential.CredentialBlob, bytes, 0, bytes.Length);
            return Encoding.Unicode.GetString(bytes);
        }
        finally
        {
            CredFree(credentialPointer);
        }
    }

    public void Write(string connectionString)
    {
        var bytes = Encoding.Unicode.GetBytes(connectionString);
        var blob = Marshal.AllocHGlobal(bytes.Length);

        try
        {
            Marshal.Copy(bytes, 0, blob, bytes.Length);
            var credential = new Credential
            {
                Type = GenericCredential,
                TargetName = TargetName,
                CredentialBlobSize = bytes.Length,
                CredentialBlob = blob,
                Persist = PersistOnThisMachine,
                UserName = "rag101",
            };

            if (!CredWrite(ref credential, 0))
            {
                throw new InvalidOperationException(
                    "The Windows Credential Manager refused to store the connection string.");
            }
        }
        finally
        {
            Marshal.FreeHGlobal(blob);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct Credential
    {
        public int Flags;
        public int Type;
        public string TargetName;
        public string? Comment;
        public long LastWritten;
        public int CredentialBlobSize;
        public IntPtr CredentialBlob;
        public int Persist;
        public int AttributeCount;
        public IntPtr Attributes;
        public string? TargetAlias;
        public string UserName;
    }

    [DllImport("advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredRead(string target, int type, int flags, out IntPtr credential);

    [DllImport("advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredWrite(ref Credential credential, int flags);

    [DllImport("advapi32.dll")]
    private static extern void CredFree(IntPtr credential);
}
