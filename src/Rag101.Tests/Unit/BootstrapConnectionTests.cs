using Rag101.Api.Bootstrap;

namespace Rag101.Tests.Unit;

public class BootstrapConnectionTests
{
    [Fact]
    public void Environment_variable_wins_over_the_credential_store()
    {
        var store = new InMemoryCredentialStore { Stored = "mongodb://from-store" };

        var bootstrap = new BootstrapConnection(store, environmentValue: "mongodb://from-environment");

        Assert.Equal("mongodb://from-environment", bootstrap.Value);
        Assert.Equal(BootstrapSource.Environment, bootstrap.Source);
    }

    [Fact]
    public void Credential_store_is_used_when_the_environment_variable_is_not_set()
    {
        var store = new InMemoryCredentialStore { Stored = "mongodb://from-store" };

        var bootstrap = new BootstrapConnection(store, environmentValue: null);

        Assert.Equal("mongodb://from-store", bootstrap.Value);
        Assert.Equal(BootstrapSource.CredentialStore, bootstrap.Source);
    }

    [Fact]
    public void Not_configured_when_neither_source_has_a_value()
    {
        var bootstrap = new BootstrapConnection(new InMemoryCredentialStore(), environmentValue: null);

        Assert.False(bootstrap.IsConfigured);
        Assert.Null(bootstrap.Value);
        Assert.Null(bootstrap.Source);
    }

    [Fact]
    public void Saving_stores_the_value_and_uses_it_immediately()
    {
        var store = new InMemoryCredentialStore();
        var bootstrap = new BootstrapConnection(store, environmentValue: null);

        bootstrap.Save("mongodb://new");

        Assert.Equal("mongodb://new", store.Stored);
        Assert.Equal("mongodb://new", bootstrap.Value);
        Assert.Equal(BootstrapSource.CredentialStore, bootstrap.Source);
        Assert.True(bootstrap.IsConfigured);
    }

    [Fact]
    public void A_value_from_the_environment_cannot_be_replaced()
    {
        var store = new InMemoryCredentialStore();
        var bootstrap = new BootstrapConnection(store, environmentValue: "mongodb://from-environment");

        Assert.False(bootstrap.CanSave);
        Assert.Throws<InvalidOperationException>(() => bootstrap.Save("mongodb://new"));
        Assert.Null(store.Stored);
    }

    [Fact]
    public void Saving_is_not_possible_without_a_credential_store()
    {
        var bootstrap = new BootstrapConnection(new UnsupportedCredentialStore(), environmentValue: null);

        Assert.False(bootstrap.IsConfigured);
        Assert.False(bootstrap.CanSave);
    }

    private class InMemoryCredentialStore : ICredentialStore
    {
        public string? Stored { get; set; }

        public bool IsSupported => true;

        public string? Read() => Stored;

        public void Write(string connectionString) => Stored = connectionString;
    }
}
