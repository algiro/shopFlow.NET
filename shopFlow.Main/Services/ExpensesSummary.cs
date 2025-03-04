
namespace shopFlow.Services {

    public interface ISuppliesExpensesSummary {
        DateOnly FromDate {get;}
        DateOnly ToDate {get;}
        IEnumerable<(string Supplies,decimal Amount)> SuppliesData {get;}
    }
    public interface IPeriodExpensesSummary {
        DateOnly FromDate {get;}
        DateOnly ToDate {get;}
        decimal Amount {get;}
    }
    public interface ITypeExpensesSummary {
        DateOnly FromDate {get;}
        DateOnly ToDate {get;}
        IEnumerable<(string Type,decimal Amount)> ExpenseTypeData {get;}
    }

    public static class ExpensesSummary {
        public static ISuppliesExpensesSummary CreateGroupedBySupplies(IEnumerable<IExpense> expenses) => new SuppliesExpensesSummary(expenses);
        public static ITypeExpensesSummary CreateGroupedByExpType(IEnumerable<IExpense> expenses) => new TypeExpensesSummary(expenses);
        public static IEnumerable<IPeriodExpensesSummary> CreateGroupedByWeeks(IEnumerable<IExpense> expenses) => PeriodExpensesSummary.Create(expenses);
    }

    public class SuppliesExpensesSummary : ISuppliesExpensesSummary
    {
        public SuppliesExpensesSummary(IEnumerable<IExpense> expenses)
        {
            SuppliesData = expenses.GroupBy(e => e.Supplies).Select(
                g => (g.Key,g.Sum(s => s.Amount)));

            if (expenses.Count() > 0) {
                var ordExp = expenses.OrderBy(e => e.Date).ToArray();
                FromDate = DateOnly.FromDateTime(ordExp.First().Date);
                ToDate = DateOnly.FromDateTime(ordExp.Last().Date);
            }
        }

        public DateOnly FromDate {get;}
        public DateOnly ToDate {get;}
        public IEnumerable<(string Supplies,decimal Amount)> SuppliesData {get;}
    }
    public class TypeExpensesSummary : ITypeExpensesSummary
    {
        public TypeExpensesSummary(IEnumerable<IExpense> expenses)
        {
            ExpenseTypeData = expenses.GroupBy(e => e.ExpType).Select(
                g => (g.Key.ToString(),g.Sum(s => s.Amount)));

            if (expenses.Count() > 0) {
                var ordExp = expenses.OrderBy(e => e.Date).ToArray();
                FromDate = DateOnly.FromDateTime(ordExp.First().Date);
                ToDate = DateOnly.FromDateTime(ordExp.Last().Date);
            }
        }

        public DateOnly FromDate {get;}
        public DateOnly ToDate {get;}
        public IEnumerable<(string Type,decimal Amount)> ExpenseTypeData {get;}
    }

    public class PeriodExpensesSummary
    {
        
        public static IEnumerable<IPeriodExpensesSummary> Create(IEnumerable<IExpense> expenses)
        {
            List<IPeriodExpensesSummary> periodsSumaries = new();
            if (expenses.Count() > 0) {
                var ordExp = expenses.OrderBy(e => e.Date).ToArray();
                var fromDate = DateOnly.FromDateTime(ordExp.First().Date);
                var toDate = DateOnly.FromDateTime(ordExp.Last().Date);

                var currentMonday = GetPreviousMonday(fromDate);
                var currentSunday = currentMonday.AddDays(6);
                while (currentSunday <= toDate) {
                    periodsSumaries.Add(Create(currentMonday,currentSunday,expenses));
                    currentMonday = currentMonday.AddDays(7);
                    currentSunday = currentSunday.AddDays(7);
                }
                periodsSumaries.Add(Create(currentMonday,currentSunday,expenses));
            }
            return periodsSumaries;
        }
        private static DateOnly GetPreviousMonday(DateOnly date) {
            while (date.DayOfWeek != DayOfWeek.Monday) {
                date = date.AddDays(-1);
            }
            return date;
        }
        private static IPeriodExpensesSummary Create(DateOnly fromDate,DateOnly toDate,IEnumerable<IExpense> expenses) {
            return new DefaultPeriodExpensesSummary(fromDate,toDate,expenses.Where(e => DateOnly.FromDateTime(e.Date) >= fromDate && DateOnly.FromDateTime(e.Date) <= toDate).Sum(e => e.Amount));
        }

        private class DefaultPeriodExpensesSummary(DateOnly fromDate,DateOnly toDate,decimal amount) : IPeriodExpensesSummary {
            public DateOnly FromDate {get;} = fromDate;
            public DateOnly ToDate {get;} = toDate;
            public decimal Amount {get;} = amount;
        }
    }
}
