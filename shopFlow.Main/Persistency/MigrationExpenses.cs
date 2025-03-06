using shopFlow.Utils;

namespace shopFlow.Persistency
{
    public class MigrationExpenses(IExpensesPersistency expensesPersistencySource, IExpensesPersistency expensesPersistencyDest)
    {
        private static readonly ILogger _logger = LoggerUtils.CreateLogger<MigrationExpenses>();
        public void Start(DateOnly startDate,DateOnly endDate)
        {
            var expExported = expensesPersistencySource.LoadExpenses(startDate, endDate);
            var expExportedCount = expExported.Count();
            _logger.LogInformation("Start {FromDate} to {ToDate} loaded#:{MovCount}", startDate, endDate, expExportedCount);
            foreach (var expense in expExported)
            {
                expensesPersistencyDest.Save(expense);
            }
            var expImported = expensesPersistencyDest.LoadExpenses(startDate, endDate);
            _logger.LogInformation("Verify dest {FromDate} to {ToDate} loaded#:{ExpCount}", startDate, endDate, expImported.Count());
            if (expImported.Count() != expExportedCount)
            {
                _logger.LogError("Error migrating Expenses from {FromDate} to {ToDate} exported#:{ExpExportedCount} imported#:{ExpImportedCount}", startDate, endDate, expExportedCount, expImported.Count());
            }
        }
    }
}
