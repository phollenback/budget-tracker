namespace BudgetTracker.Domain;

public sealed class MonthlyBudget
{
    private readonly List<BudgetCategory> _categories;

    public Guid Id { get; }
    public MonthPeriod Period { get; }
    public string OwnerName { get; private set; }
    public DateOnly? CompletedOn { get; private set; }
    public Money NetMonthlyIncome { get; private set; }
    public BudgetStatus Status { get; private set; }
    public DateTime? SubmittedAt { get; private set; }

    public IReadOnlyList<BudgetCategory> Categories => _categories;
    public bool IsSubmitted => Status == BudgetStatus.Submitted;

    public IEnumerable<BudgetLineItem> AllLines() =>
        _categories.SelectMany(category => category.Lines);

    public Money EssentialSubtotal => SubtotalOf(AllocationBucket.Essential);
    public Money DiscretionarySubtotal => SubtotalOf(AllocationBucket.Discretionary);
    public Money SavingSubtotal => SubtotalOf(AllocationBucket.SavingAndInvesting);
    public Money TotalMonthlyExpenses => EssentialSubtotal.Add(DiscretionarySubtotal);
    public Money LessExpenses => TotalMonthlyExpenses;
    public Balance SurplusOrDeficit =>
        Balance.Of(NetMonthlyIncome, EssentialSubtotal, DiscretionarySubtotal, SavingSubtotal);

    private MonthlyBudget(Guid id, MonthPeriod period)
    {
        Id = id;
        Period = period;
        OwnerName = string.Empty;
        NetMonthlyIncome = Money.Zero;
        Status = BudgetStatus.Draft;
        _categories = StandardChartOfAccounts.Create().ToList();
    }

    public static MonthlyBudget Open(MonthPeriod period)
    {
        ArgumentNullException.ThrowIfNull(period);
        return new MonthlyBudget(Guid.NewGuid(), period);
    }

    public void SetOwnerName(string name) =>
        OwnerName = name?.Trim() ?? string.Empty;

    public void SetCompletedOn(DateOnly? date) =>
        CompletedOn = date;

    public void SetIncome(Money amount)
    {
        ArgumentNullException.ThrowIfNull(amount);
        NetMonthlyIncome = amount;
    }

    public void SetLineAmount(string lineName, Money amount)
    {
        if (string.IsNullOrWhiteSpace(lineName))
        {
            throw new ArgumentException("Line item name is required.", nameof(lineName));
        }

        ArgumentNullException.ThrowIfNull(amount);

        var line = FindLine(lineName)
                   ?? throw new ArgumentException($"Unknown line item '{lineName}'.", nameof(lineName));

        line.SetAmount(amount);
    }

    public BudgetCategory? CategoryNamed(string name) =>
        _categories.SingleOrDefault(category => category.Name == name);

    public AllocationSnapshot CalculateAllocation()
    {
        var essential = new BucketAllocation(AllocationBucket.Essential, EssentialSubtotal, NetMonthlyIncome);
        var discretionary = new BucketAllocation(AllocationBucket.Discretionary, DiscretionarySubtotal, NetMonthlyIncome);
        var saving = new BucketAllocation(AllocationBucket.SavingAndInvesting, SavingSubtotal, NetMonthlyIncome);

        return new AllocationSnapshot(
            essential,
            discretionary,
            saving,
            TotalMonthlyExpenses,
            SurplusOrDeficit);
    }

    public void Submit()
    {
        Status = BudgetStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;
        CompletedOn ??= DateOnly.FromDateTime(DateTime.UtcNow);
    }

    private Money SubtotalOf(AllocationBucket bucket) =>
        Money.Sum(_categories.Where(category => category.Bucket == bucket).Select(category => category.Subtotal));

    private BudgetLineItem? FindLine(string lineName) =>
        AllLines().SingleOrDefault(line => line.Name == lineName);
}
