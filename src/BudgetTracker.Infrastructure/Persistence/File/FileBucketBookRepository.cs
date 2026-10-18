using System.Text.Json;
using BudgetTracker.Domain;

namespace BudgetTracker.Infrastructure;

public sealed class FileBucketBookRepository : IBucketBookRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _path;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public FileBucketBookRepository(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        _path = Path.Combine(directory, "book.json");
    }

    public async Task<BucketBook> GetAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            if (!File.Exists(_path))
            {
                return BucketBook.Empty();
            }

            await using var stream = File.OpenRead(_path);
            var record = await JsonSerializer.DeserializeAsync<BucketBookRecord>(stream, JsonOptions, cancellationToken)
                         ?? new BucketBookRecord();
            return record.ToBook();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveAsync(BucketBook book, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(book);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            var json = JsonSerializer.Serialize(BucketBookRecord.FromBook(book), JsonOptions);
            await File.WriteAllTextAsync(_path, json, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }
}
