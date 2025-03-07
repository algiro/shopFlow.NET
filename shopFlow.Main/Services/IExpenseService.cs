using shopFlow.Persistency;
using shopFlow.Utils;

namespace shopFlow.Services
{

    public interface IExpenseService
    {
        bool Add(IExpense expense);
        bool Delete(IExpense expense);
        bool Update(IExpense expense);
        IEnumerable<IExpense> GetExpenses(DateOnly fromDate, DateOnly toDate);

    }

    public class DefaultExpenseService(IExpensesPersistency expensesPersistency) : IExpenseService
    {
        private static readonly ILogger logger = LoggerUtils.CreateLogger<IExpenseService>();
        public bool Add(IExpense expense)
        {
            try
            {
                return expensesPersistency.Save(expense);
            }
            catch (Exception ex)
            {
                logger.LogError("Error on ExpenseService.Add {Expense}, error:{Error}", expense, ex);
                return false;
            }
        }
        public bool Delete(IExpense expense)
        {
            try
            {
                return expensesPersistency.Delete(expense);
            }
            catch (Exception ex)
            {
                logger.LogError("Error on ExpenseService.Delete {Expense}, error:{Error}", expense, ex);
                return false;
            }
        }
        public bool Update(IExpense expense)
        {
            try
            {
                return expensesPersistency.Save(expense);
            }
            catch (Exception ex)
            {
                logger.LogError("Error on ExpenseService.Update {Expense}, error:{Error}", expense, ex);
                return false;
            }
        }
        public IEnumerable<IExpense> GetExpenses(DateOnly fromDate, DateOnly toDate) => expensesPersistency.LoadExpenses(fromDate, toDate);
    }
}