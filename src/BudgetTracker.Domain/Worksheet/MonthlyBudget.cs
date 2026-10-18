namespace BudgetTracker.Domain;

public sealed class MonthlyBudget
{
    private readonly List<BudgetCategory> _categories;
    private AllocationSnapshot? _allocation;

    public Guid Id { get; }
    public MonthPeriod Period { get; }
    public string OwnerName { get; private set; }
    public DateOnly? CompletedOn { get; private set; }
    public Money NetMonthlyIncome { get; private set; }
    public BudgetStatus Status { get; private set; }
    public DateTime? SubmittedAt { get; private set; }

    public IReadOnlyList<BudgetCategory> Categories => _categories;
    public bool IsSubmitted => Status == BudgetStatus.Submitted;

    public bool HasWorksheetData =>
        IsSubmitted
        || CompletedOn.HasValue
        || !NetMonthlyIncome.Equals(Money.Zero)
        || AllLines().Any(line => !line.Amount.Equals(Money.Zero));

    public IEnumerable<BudgetLineItem> AllLines() =>
        _categories.SelectMany(category => category.Lines);

    public Money EssentialSubtotal => CalculateAllocation().Essential.Total;
    public Money DiscretionarySubtotal => CalculateAllocation().Discretionary.Total;
    public Money SavingSubtotal => CalculateAllocation().SavingAndInvesting.Total;
    public Money TotalMonthlyExpenses => CalculateAllocation().TotalMonthlyExpenses;
    public Money LessExpenses => TotalMonthlyExpenses;
    public Balance SurplusOrDeficit => CalculateAllocation().SurplusOrDeficit;

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

    public static MonthlyBudget Restore(
        Guid id,
        MonthPeriod period,
        string ownerName,
        DateOnly? completedOn,
        Money netMonthlyIncome,
        BudgetStatus status,
        DateTime? submittedAt,
        IReadOnlyDictionary<string, Money>? lineAmounts = null)
    {
        ArgumentNullException.ThrowIfNull(period);
        ArgumentNullException.ThrowIfNull(netMonthlyIncome);

        var budget = new MonthlyBudget(id, period)
        {
            OwnerName = ownerName?.Trim() ?? string.Empty,
            CompletedOn = completedOn,
            NetMonthlyIncome = netMonthlyIncome,
            Status = status,
            SubmittedAt = submittedAt
        };

        if (lineAmounts is null)
        {
            return budget;
        }

        foreach (var (name, amount) in lineAmounts)
        {
            budget.FindLine(name)?.SetAmount(amount);
        }

        return budget;
    }

    public void SetOwnerName(string name) =>
        OwnerName = name?.Trim() ?? string.Empty;

    public void SetCompletedOn(DateOnly? date) =>
        CompletedOn = date;

    public void SetIncome(Money amount)
    {
        ArgumentNullException.ThrowIfNull(amount);
        NetMonthlyIncome = amount;
        InvalidateTotals();
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
        InvalidateTotals();
    }

    public BudgetCategory? CategoryNamed(string name) =>
        _categories.SingleOrDefault(category => category.Name == name);

    public AllocationSnapshot CalculateAllocation()
    {
        if (_allocation is not null)
        {
            return _allocation;
        }

        var essentialTotal = SubtotalOf(AllocationBucket.Essential);
        var discretionaryTotal = SubtotalOf(AllocationBucket.Discretionary);
        var savingTotal = SubtotalOf(AllocationBucket.SavingAndInvesting);
        var expenses = essentialTotal.Add(discretionaryTotal);

        _allocation = new AllocationSnapshot(
            new BucketAllocation(AllocationBucket.Essential, essentialTotal, NetMonthlyIncome),
            new BucketAllocation(AllocationBucket.Discretionary, discretionaryTotal, NetMonthlyIncome),
            new BucketAllocation(AllocationBucket.SavingAndInvesting, savingTotal, NetMonthlyIncome),
            expenses,
            Balance.Of(NetMonthlyIncome, essentialTotal, discretionaryTotal, savingTotal));

        return _allocation;
    }

    public void Submit()
    {
        Status = BudgetStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;
        CompletedOn ??= DateOnly.FromDateTime(DateTime.UtcNow);
    }

    private void InvalidateTotals() => _allocation = null;

    private Money SubtotalOf(AllocationBucket bucket) =>
        Money.Sum(_categories.Where(category => category.Bucket == bucket).Select(category => category.Subtotal));

    private BudgetLineItem? FindLine(string lineName) =>
        AllLines().SingleOrDefault(line => line.Name == lineName);
}
