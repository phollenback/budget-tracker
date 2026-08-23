using System.Text.Json;
using BudgetTracker.Web.Models;

namespace BudgetTracker.Web.Services;

public sealed class FileBudgetWorksheetStore : IBudgetWorksheetStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _directory;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public FileBudgetWorksheetStore(IHostEnvironment environment)
    {
        _directory = Path.Combine(environment.ContentRootPath, "App_Data", "worksheets");
    }

    public async Task<BudgetForm> GetOrCreateAsync(DateOnly period, CancellationToken cancellationToken = default)
    {
        period = BudgetForm.FirstOfMonth(period);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(_directory);

            var path = PathFor(period);
            if (File.Exists(path))
            {
                return await ReadAsync(path, cancellationToken);
            }

            var form = new BudgetForm { Period = period };
            var latest = await FindLatestAsync(cancellationToken);
            if (latest is not null)
            {
                form.Name = latest.Name;
            }

            return form;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveAsync(BudgetForm form, bool submit, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(_directory);

            if (submit)
            {
                form.SubmittedAt = DateTime.Now;
                form.Date ??= DateOnly.FromDateTime(DateTime.Today);
            }

            var snapshot = BudgetFormSnapshot.FromForm(form);
            var json = JsonSerializer.Serialize(snapshot, JsonOptions);
            await File.WriteAllTextAsync(PathFor(form.Period), json, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyList<MonthWorksheetSummary>> ListAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(_directory);

            var summaries = new List<MonthWorksheetSummary>();
            foreach (var path in Directory.EnumerateFiles(_directory, "*-*-01.json"))
            {
                var form = await ReadAsync(path, cancellationToken);
                if (!form.IsSubmitted && !form.HasEntries)
                {
                    continue;
                }

                summaries.Add(new MonthWorksheetSummary(form.Period, form.IsSubmitted, form.SubmittedAt));
            }

            return summaries
                .OrderByDescending(summary => summary.Period)
                .ToList();
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<BudgetForm?> FindLatestAsync(CancellationToken cancellationToken)
    {
        var files = Directory.EnumerateFiles(_directory, "*-*-01.json")
            .OrderByDescending(path => path)
            .ToList();

        if (files.Count == 0)
        {
            return null;
        }

        return await ReadAsync(files[0], cancellationToken);
    }

    private static async Task<BudgetForm> ReadAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var snapshot = await JsonSerializer.DeserializeAsync<BudgetFormSnapshot>(stream, JsonOptions, cancellationToken)
                       ?? throw new InvalidOperationException($"Could not read worksheet {path}.");
        return snapshot.ToForm();
    }

    private string PathFor(DateOnly period) =>
        Path.Combine(_directory, $"{period:yyyy-MM-dd}.json");
}
