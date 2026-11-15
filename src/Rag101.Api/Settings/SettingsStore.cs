using MongoDB.Driver;
using Rag101.Api.Data;

namespace Rag101.Api.Settings;

/// <summary>Reads and writes the single settings document in MongoDB.</summary>
public class SettingsStore(MongoDatabaseProvider databaseProvider)
{
    public const string CollectionName = "settings";

    private IMongoCollection<AppSettings> Collection =>
        databaseProvider.GetDatabase().GetCollection<AppSettings>(CollectionName);

    /// <summary>Returns the stored settings, or empty ones when nothing was saved yet.</summary>
    public async Task<AppSettings> LoadAsync()
    {
        var stored = await Collection.Find(s => s.Id == AppSettings.DocumentId).FirstOrDefaultAsync();
        return stored ?? new AppSettings();
    }

    public async Task SaveAsync(AppSettings settings)
    {
        settings.Id = AppSettings.DocumentId;
        await Collection.ReplaceOneAsync(
            s => s.Id == AppSettings.DocumentId,
            settings,
            new ReplaceOptions { IsUpsert = true });
    }
}
