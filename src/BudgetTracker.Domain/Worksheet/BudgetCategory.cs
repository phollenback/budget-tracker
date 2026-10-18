namespace BudgetTracker.Domain;

public sealed class BudgetCategory
{
    private readonly List<BudgetLineItem> _lines;

    public string Name { get; }
    public AllocationBucket Bucket { get; }
    public IReadOnlyList<BudgetLineItem> Lines => _lines;
    public Money Subtotal => Money.Sum(_lines.Select(line => line.Amount));

    internal BudgetCategory(string name, AllocationBucket bucket, IEnumerable<string> lineNames)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name is required.", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(lineNames);

        Name = name;
        Bucket = bucket;
        _lines = lineNames.Select(lineName => new BudgetLineItem(lineName)).ToList();

        if (_lines.Count == 0)
        {
            throw new ArgumentException("A category must have at least one line item.", nameof(lineNames));
        }
    }

    internal BudgetLineItem? FindLine(string name) =>
        _lines.SingleOrDefault(line => line.Name == name);
}
