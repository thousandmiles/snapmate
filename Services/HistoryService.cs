using LiteDB;
using SnapMate.Models;
using System.IO;

namespace SnapMate.Services;

/// <summary>
/// Provides persistence and retrieval of screenshot history using a local database.
/// </summary>
public interface IHistoryService
{
    /// <summary>
    /// Retrieves all screenshots from history, ordered by capture date (newest first).
    /// </summary>
    /// <returns>Collection of all screenshots.</returns>
    Task<IEnumerable<Screenshot>> GetAllAsync();

    /// <summary>
    /// Retrieves a specific screenshot by its ID.
    /// </summary>
    /// <param name="id">The screenshot ID.</param>
    /// <returns>The screenshot if found, null otherwise.</returns>
    Task<Screenshot?> GetByIdAsync(int id);

    /// <summary>
    /// Searches screenshots by title, tags, or OCR text.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <returns>Collection of matching screenshots.</returns>
    Task<IEnumerable<Screenshot>> SearchAsync(string query);

    /// <summary>
    /// Adds a new screenshot to the history.
    /// </summary>
    /// <param name="screenshot">The screenshot to add.</param>
    /// <returns>The ID of the newly added screenshot.</returns>
    Task<int> AddAsync(Screenshot screenshot);

    /// <summary>
    /// Deletes a screenshot from history by ID.
    /// </summary>
    /// <param name="id">The screenshot ID to delete.</param>
    /// <returns>True if deleted, false if not found.</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Clears all screenshots from history.
    /// </summary>
    Task ClearAllAsync();
}

/// <summary>
/// Implementation of history service using LiteDB as the storage backend.
/// Database is stored in the user's AppData folder.
/// </summary>
public class HistoryService : IHistoryService
{
    private readonly string _dbPath;

    public HistoryService()
    {
        var appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SnapMate");
        Directory.CreateDirectory(appData);
        _dbPath = Path.Combine(appData, "history.db");
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Screenshot>> GetAllAsync()
    {
        return await Task.Run(() =>
        {
            using var db = new LiteDatabase(_dbPath);
            var collection = db.GetCollection<Screenshot>("screenshots");
            return collection.Query().OrderByDescending(x => x.CapturedAt).ToList();
        });
    }

    /// <inheritdoc />
    public async Task<Screenshot?> GetByIdAsync(int id)
    {
        return await Task.Run(() =>
        {
            using var db = new LiteDatabase(_dbPath);
            var collection = db.GetCollection<Screenshot>("screenshots");
            return collection.FindById(id);
        });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Screenshot>> SearchAsync(string query)
    {
        return await Task.Run(() =>
        {
            using var db = new LiteDatabase(_dbPath);
            var collection = db.GetCollection<Screenshot>("screenshots");

            return collection.Query()
                .Where(x => x.Title != null && x.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                             x.Tags != null && x.Tags.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                             x.OcrText != null && x.OcrText.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
        });
    }

    /// <inheritdoc />
    public async Task<int> AddAsync(Screenshot screenshot)
    {
        return await Task.Run(() =>
        {
            using var db = new LiteDatabase(_dbPath);
            var collection = db.GetCollection<Screenshot>("screenshots");
            return collection.Insert(screenshot);
        });
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        return await Task.Run(() =>
        {
            using var db = new LiteDatabase(_dbPath);
            var collection = db.GetCollection<Screenshot>("screenshots");
            return collection.Delete(id);
        });
    }

    /// <inheritdoc />
    public async Task ClearAllAsync()
    {
        await Task.Run(() =>
        {
            using var db = new LiteDatabase(_dbPath);
            db.DropCollection("screenshots");
        });
    }
}
