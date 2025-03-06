using System;
using shopFlow.Persistency.Impl;
using shopFlow.Services;
using shopFlow.Utils;

namespace shopFlow.Persistency;

public enum ExpPersistencyType { Json, LiteDB }

public interface IExpensesPersistency
{
    bool Save(IExpense expenseDetail);
    bool Delete(IExpense expense);
    IEnumerable<IExpense> LoadExpenses(DateOnly fromDate, DateOnly toDate);
}

public static class ExpensesPersistencyHelper
{
    private static readonly ILogger logger = LoggerUtils.CreateLogger<IExpensesPersistency>();

    public static void AddExpensesPersistency(this IServiceCollection services)
    {
        var expPersistencyEnv = Environment.GetEnvironmentVariable("MOV_PERSISTENCY_TYPE");
        ExpPersistencyType expPersistencyType = ExpPersistencyType.Json;
        Enum.TryParse<ExpPersistencyType>(expPersistencyEnv, out expPersistencyType);
        logger.LogInformation("ExpPersistencyType: {ExpPersistencyType}", expPersistencyType);

        if (expPersistencyType == ExpPersistencyType.Json)
        {
            services.AddSingleton<IExpensesPersistency, JsonExpensesPersistency>();
        }
        else
        {
            services.AddSingleton<IExpensesPersistency, LiteDBExpensesPersistency>();
        }
    }

}
