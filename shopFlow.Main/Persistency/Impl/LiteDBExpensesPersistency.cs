using LiteDB;
using shopFlow.Services;
using shopFlow.Utils;


namespace shopFlow.Persistency.Impl
{
    public class LiteDBExpensesPersistency : IExpensesPersistency
    {
        private readonly TimeOnly DAY_TIME_START = new TimeOnly(0, 0, 0);
        private readonly TimeOnly DAY_TIME_END = new TimeOnly(23, 59, 59);
        private readonly ILogger _logger = LoggerUtils.CreateLogger<LiteDBExpensesPersistency>();
        private readonly LiteDatabase _db;
        private const string EXPENSES_COLLECTION = "expenses";
        public LiteDBExpensesPersistency(LiteDatabase db)
        {
            _db = db;
            _db.GetCollection<DefaultExpense>(EXPENSES_COLLECTION).EnsureIndex(m => m.Id,unique:true);
            _db.GetCollection<DefaultExpense>(EXPENSES_COLLECTION).EnsureIndex(m => m.Date);
        }
        public bool Delete(IExpense expense)
        {
            try
            {
                var hasBeenDeleted = _db.GetCollection<DefaultExpense>(EXPENSES_COLLECTION).Delete(expense.Id);
                _logger.LogInformation("Delete expense: {Expense} deleted:{HasBeenDeleted}", expense, hasBeenDeleted);
                return hasBeenDeleted;
            }
            finally
            {
                _db.Checkpoint();
            }
        }
        public IEnumerable<IExpense> LoadExpenses(DateOnly fromDate, DateOnly toDate)
        {
            _logger.LogInformation("LoadExpenses: {FromDate} to {ToDate}", fromDate, toDate);
            var expenses = _db.GetCollection<DefaultExpense>(EXPENSES_COLLECTION).Query().Where(m => m.Date >= fromDate.ToDateTime(DAY_TIME_START) && m.Date <= toDate.ToDateTime(DAY_TIME_END)).ToList();
            _logger.LogInformation("LoadExpenses: {FromDate} to {ToDate} loaded#:{ExpCount}", fromDate, toDate, expenses.Count);
            return expenses;
        }

        public bool Save(IExpense expense)
        {
            try
            {
                _logger.LogInformation("Save expense: {Expense}", expense);
                var expColl = _db.GetCollection<DefaultExpense>(EXPENSES_COLLECTION);
                var storedExpense = expColl.FindById(expense.Id);
                if (storedExpense != null)
                {
                    var hasBeenUpdated = expColl.Update(expense.AsDefaultExpense());
                    _logger.LogInformation("Update expense: {Expense} hasBeenUpdated:{hasBeenUpdated}", expense, hasBeenUpdated);
                    return true;
                }
                else
                {
                    var idExp = expColl.Insert(expense.AsDefaultExpense());
                    _logger.LogInformation("Insert expense: {Expense} with Id:{Id}", expense, idExp);
                    return true;
                }
            }
            finally
            {
                _db.Checkpoint();
            }
        }
    }
}
