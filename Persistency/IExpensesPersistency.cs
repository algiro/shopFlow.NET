using System;
using shopFlow.Services;

namespace shopFlow.Persistency;

public interface IExpensesPersistency
{
    bool Save(IExpense expenseDetail);
    bool Delete(IExpense expense);
    IEnumerable<IExpense> LoadExpenses(DateOnly fromDate, DateOnly toDate);
}
