using shopFlow.Persistency;

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
        public bool Add(IExpense expense)
        {
            try
            {
                return expensesPersistency.Save(expense);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error on IExpenseService.Add " + expense + " error:" + ex);
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
                Console.Error.WriteLine("Error on IExpenseService.Add " + expense + " error:" + ex);
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
                Console.Error.WriteLine("Error on IExpenseService.Add " + expense + " error:" + ex);
                return false;
            }
        }
        public IEnumerable<IExpense> GetExpenses(DateOnly fromDate, DateOnly toDate) => expensesPersistency.LoadExpenses(fromDate, toDate);
    }
}