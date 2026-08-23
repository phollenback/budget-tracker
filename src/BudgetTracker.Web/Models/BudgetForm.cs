namespace BudgetTracker.Web.Models;

public sealed class BudgetForm
{
    public DateOnly Period { get; set; } = FirstOfMonth(DateOnly.FromDateTime(DateTime.Today));
    public string Name { get; set; } = string.Empty;
    public DateOnly? Date { get; set; }
    public DateTime? SubmittedAt { get; set; }

    public decimal? NetMonthlyIncome { get; set; }

    public bool IsSubmitted => SubmittedAt.HasValue;

    public bool HasEntries =>
        NetMonthlyIncome.HasValue
        || Date.HasValue
        || AllLines().Any(line => line.Amount.HasValue);

    public IEnumerable<BudgetLine> AllLines() =>
        Housing
            .Concat(Transportation)
            .Concat(Debt)
            .Concat(HealthCare)
            .Concat(HouseholdPersonal)
            .Concat(Children)
            .Concat(Discretionary)
            .Concat(SavingAndInvesting);

    public static DateOnly FirstOfMonth(DateOnly date) => new(date.Year, date.Month, 1);

    public List<BudgetLine> Housing { get; } =
    [
        new() { Name = "Mortgage/Rent" },
        new() { Name = "Property Taxes" },
        new() { Name = "Home Maintenance" },
        new() { Name = "Homeowner’s/Renter’s Ins." },
        new() { Name = "Utilities (Electric, Gas, Water, etc.)" }
    ];

    public List<BudgetLine> Transportation { get; } =
    [
        new() { Name = "Auto Payment(s)" },
        new() { Name = "Auto Insurance" },
        new() { Name = "Gas" },
        new() { Name = "Maintenance/License" },
        new() { Name = "Parking/Tolls/Bus/Train" }
    ];

    public List<BudgetLine> Debt { get; } =
    [
        new() { Name = "Student Loans" },
        new() { Name = "Credit Card Debt" },
        new() { Name = "Personal Loans" }
    ];

    public List<BudgetLine> HealthCare { get; } =
    [
        new() { Name = "Health Insurance" },
        new() { Name = "Life Insurance" },
        new() { Name = "Disability Income Insurance" },
        new() { Name = "Long-term Care Insurance" },
        new() { Name = "Medical/Dental/Drugs" }
    ];

    public List<BudgetLine> HouseholdPersonal { get; } =
    [
        new() { Name = "Groceries" },
        new() { Name = "Personal Care" },
        new() { Name = "Clothing/Dry Cleaning" },
        new() { Name = "Professional Dues" },
        new() { Name = "Cell Phone" }
    ];

    public List<BudgetLine> Children { get; } =
    [
        new() { Name = "Dependent/Child Care" },
        new() { Name = "Education/School" }
    ];

    public List<BudgetLine> Discretionary { get; } =
    [
        new() { Name = "Cable/Phone/Internet" },
        new() { Name = "Dining Out" },
        new() { Name = "Recreation/Club Dues" },
        new() { Name = "Movies/Sporting Events" },
        new() { Name = "Hobbies" },
        new() { Name = "Vacation/Travel" },
        new() { Name = "Gifts/Contributions" }
    ];

    public List<BudgetLine> SavingAndInvesting { get; } =
    [
        new() { Name = "Emergency Fund" },
        new() { Name = "College Savings" },
        new() { Name = "Short/Mid-Term Needs" },
        new() { Name = "Retirement Savings" }
    ];
}
