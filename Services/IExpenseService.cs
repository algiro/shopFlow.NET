using shopFlow.Persistency;

namespace shopFlow.Services {

    public interface IExpenseService {
        bool Add(IExpense expense);
        bool Delete(IExpense expense);
        bool Update(IExpense expense);
        void Upgrade();
        IEnumerable<IExpense> GetExpenses((int Year,int Month) period);
        IEnumerable<IExpense> GetExpenses(DateOnly fromDate,DateOnly toDate);

    }

    public class DefaultExpenseService : IExpenseService
    {
        public bool Add(IExpense expense) {
            try {
                return ExpensesPersistency.Save(expense);
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error on IExpenseService.Add " + expense + " error:" + ex);
                return false;
            }
        }
        public bool Delete(IExpense expense) {
            try {
                return ExpensesPersistency.Delete(expense);
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error on IExpenseService.Add " + expense + " error:" + ex);
                return false;
            }
        }
        public bool Update(IExpense expense) {
            try {
                return ExpensesPersistency.Save(expense);
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error on IExpenseService.Add " + expense + " error:" + ex);
                return false;
            }
        }        
        public IEnumerable<IExpense> GetExpenses((int Year,int Month) period)  => ExpensesPersistency.LoadExpenses(period);
        public IEnumerable<IExpense> GetExpenses(DateOnly fromDate,DateOnly toDate) => ExpensesPersistency.LoadExpenses(fromDate,toDate);

        public void Upgrade() {
            var oldExpenses = ExpensesPersistency.LoadExpenses(new DateOnly(2024,05,01),new DateOnly(2025,01,30));
            foreach (var exp in oldExpenses) {
                Update(exp);
            }
        }
    }
}