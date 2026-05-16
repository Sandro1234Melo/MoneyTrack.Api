namespace MoneyTrack.Api.Domain.Enum
{
    public enum GoalTypeEnum
    {
        Savings = 0,
        ExpenseLimit = 1
    }

    public enum GoalScopeEnum
    {
        General = 0,
        Category = 1,
        Location = 2
    }

    public enum GoalPeriodEnum
    {
        Custom = 0,
        Monthly = 1,
        Weekly = 2
    }
}
