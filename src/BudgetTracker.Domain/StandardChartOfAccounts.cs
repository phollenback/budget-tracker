namespace BudgetTracker.Domain;

/// <summary>
/// Official line items from the monthly budget worksheet.
/// </summary>
public static class StandardChartOfAccounts
{
    public static IReadOnlyList<BudgetCategory> Create() =>
    [
        new("Housing", AllocationBucket.Essential,
        [
            "Mortgage/Rent",
            "Property Taxes",
            "Home Maintenance",
            "Homeowner’s/Renter’s Ins.",
            "Utilities (Electric, Gas, Water, etc.)"
        ]),
        new("Transportation", AllocationBucket.Essential,
        [
            "Auto Payment(s)",
            "Auto Insurance",
            "Gas",
            "Maintenance/License",
            "Parking/Tolls/Bus/Train"
        ]),
        new("Debt Payments", AllocationBucket.Essential,
        [
            "Student Loans",
            "Credit Card Debt",
            "Personal Loans"
        ]),
        new("Health Care / Insurance", AllocationBucket.Essential,
        [
            "Health Insurance",
            "Life Insurance",
            "Disability Income Insurance",
            "Long-term Care Insurance",
            "Medical/Dental/Drugs"
        ]),
        new("Household / Personal", AllocationBucket.Essential,
        [
            "Groceries",
            "Personal Care",
            "Clothing/Dry Cleaning",
            "Professional Dues",
            "Cell Phone"
        ]),
        new("Children", AllocationBucket.Essential,
        [
            "Dependent/Child Care",
            "Education/School"
        ]),
        new("Discretionary", AllocationBucket.Discretionary,
        [
            "Cable/Phone/Internet",
            "Dining Out",
            "Recreation/Club Dues",
            "Movies/Sporting Events",
            "Hobbies",
            "Vacation/Travel",
            "Gifts/Contributions"
        ]),
        new("Saving and Investing", AllocationBucket.SavingAndInvesting,
        [
            "Emergency Fund",
            "College Savings",
            "Short/Mid-Term Needs",
            "Retirement Savings"
        ])
    ];
}
