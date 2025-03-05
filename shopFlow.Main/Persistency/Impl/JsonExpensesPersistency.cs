using System.IO.Abstractions;
using System.Text.Json.Serialization;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using shopFlow.Config;
using shopFlow.Services;
using shopFlow.Utils;

namespace shopFlow.Persistency
{

    public class JsonExpensesPersistency(IFileSystem fileSystem) : IExpensesPersistency
    {
        private readonly static ILogger _logger = LoggerUtils.CreateLogger<JsonExpensesPersistency>();
        public const string SUB_FOLDER = "Expenses";
        public const string CONFIG_FOLDER = "Config";
        public bool Save(IExpense expenseDetail)
        {
            try
            {
                _logger.LogInformation("Save expense: {ExpenseDetail}", expenseDetail);
                var periodFolder = $"{expenseDetail.Date.Year}-{expenseDetail.Date.Month.ToString("00")}";
                var fullPath = Path.Combine(baseFolder, periodFolder);
                if (!fileSystem.Directory.Exists(fullPath))
                {
                    fileSystem.Directory.CreateDirectory(fullPath);
                }
                fileSystem.File.WriteAllText(Path.Combine(fullPath, expenseDetail.GetFileName()), JsonConvert.SerializeObject(expenseDetail));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving expense: {ExpenseDetail}", expenseDetail);
                return false;
            }

        }
        public bool Delete(IExpense expense)
        {
            try
            {
                _logger.LogInformation("Delete expense: {Expense}", expense);
                var periodFolder = $"{expense.Date.Year}-{expense.Date.Month.ToString("00")}";
                var fullPath = Path.Combine(baseFolder, periodFolder);
                var filePath = Path.Combine(fullPath, expense.GetFileName());
                if (fileSystem.File.Exists(filePath))
                {
                    fileSystem.File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting expense: {Expense}", expense);
                return false;
            }

        }
        private IEnumerable<IExpense> LoadExpenses((int Year, int Month) period)
        {
            try
            {
                _logger.LogInformation("LoadExpenses with period: {Period}", period);
                var periodFolder = $"{period.Year}-{period.Month.ToString("00")}";
                var fullPath = Path.Combine(baseFolder, periodFolder);
                if (fileSystem.Directory.Exists(fullPath))
                {
                    var allFiles = fileSystem.Directory.GetFiles(fullPath);
                    var expensesFiles = allFiles.Where(f => f.EndsWith("_EXP.json")).Select(f => new FileInfo(f));
                    List<IExpense> expenses = new();
                    foreach (FileInfo file in expensesFiles)
                    {
                        try
                        {
                            var expense = JsonConvert.DeserializeObject<DefaultExpense>(fileSystem.File.ReadAllText(file.FullName));
                            if (expense != null)
                                expenses.Add(expense);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error parsing expense: {File}", file.FullName);
                        }
                    }
                    _logger.LogInformation("LoadExpenses with period: {Period} expenses# {ExpensesCount}", period, expenses.Count);

                    return expenses;
                }
                else
                {
                    _logger.LogInformation("LoadExpenses with period: {Period} no expenses", period);
                    return [];
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading expenses with period: {Period}", period);
            }
            return [];
        }

        public IEnumerable<IExpense> LoadExpenses(DateOnly fromDate, DateOnly toDate)
        {
            try
            {
                var periods = GetPeriods(fromDate, toDate);
                _logger.LogInformation("LoadExpenses {FromDate} to {ToDate}, periods: {Periods}", fromDate, toDate, string.Join(';', periods));
                List<IExpense> expenses = [];
                foreach (var period in periods)
                {
                    var periodExps = LoadExpenses(period);
                    expenses.AddRange(periodExps);
                }
                _logger.LogInformation("LoadExpenses, from: {FromDate} to {ToDate} expenses# {ExpensesCount}", fromDate, toDate, expenses.Count);

                return expenses.Where(m => m.Date >= fromDate.ToDateTime(TimeOnly.Parse("00:00 AM")) && m.Date <= toDate.ToDateTime(TimeOnly.Parse("11:59 PM")));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading expenses from: {FromDate} to {ToDate}", fromDate, toDate);
            }
            return [];
        }

        private static (int Year, int Month)[] GetPeriods(DateOnly fromDate, DateOnly toDate)
        {
            var currDate = fromDate;
            List<(int Year, int Month)> periodFolders = [];
            while (currDate <= toDate)
            {
                var periodFolder = (currDate.Year, currDate.Month);
                periodFolders.Add(periodFolder);
                _logger.LogInformation("CurrendDate: {CurrentDate} => Period identified: {PeriodFolder}", currDate, periodFolder);
                currDate = currDate.AddMonths(1);
            }
            return [.. periodFolders];
        }
        private static string mainPersistencyFolder = ShopFlowConfig.Instance.PersistencyMainFolder ?? Directory.GetCurrentDirectory();
        private static string baseFolder = Path.Combine(mainPersistencyFolder, SUB_FOLDER);
    }
}