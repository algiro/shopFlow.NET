
namespace shopFlow.Services {

    public interface IMovementsSummary {
        DateOnly FromDate {get;}
        DateOnly ToDate {get;}
        decimal CashAmount {get;}
        decimal CardAmount {get;}
        decimal ExpenseAmount {get;}

        IEnumerable<IDaySummary> DaySummaries {get;}
    }

    public interface IDaySummary {
        DateOnly Date {get;}
        decimal CashAmount {get;}
        decimal CardAmount {get;}
        decimal ExpenseAmount {get;}

        IEnumerable<string> Notes {get;}

    }

    public static class MovementSummary {
        public static IMovementsSummary Create(IEnumerable<IMovement> movements) => new DefaultMovementsSummary(movements);
        public static IDaySummary CreateDay(DateOnly date,decimal cashAmount,decimal cardAmount,decimal expenseAmount) => new DefaultDaySummary(date,cashAmount,cardAmount,expenseAmount,Enumerable.Empty<string>());
        public static IDaySummary CreateDay(DateOnly date,decimal cashAmount,decimal cardAmount,decimal expenseAmount,IEnumerable<string> notes) => new DefaultDaySummary(date,cashAmount,cardAmount,expenseAmount,notes);
    }

    public class DefaultMovementsSummary : IMovementsSummary
    {
        public DefaultMovementsSummary(IEnumerable<IMovement> movements)
        {
            var orderedMovements = movements.OrderBy(m => m.Date).ToArray();
            if (orderedMovements.Length > 0) {

                FromDate = DateOnly.FromDateTime(orderedMovements.First().Date);
                ToDate = DateOnly.FromDateTime(orderedMovements.Last().Date);
                var daySummaries = new List<IDaySummary>();
                var totCashAmount = 0m;
                var totCardAmount = 0m;
                var totExpenseAmount = 0m;
                DateOnly currDate = FromDate;
                while (currDate <= ToDate) {
                    Console.Out.WriteLine("DefaultMovementsSummary handling date:" + currDate);
                    var dayMovements = movements.Where(m => DateOnly.FromDateTime(m.Date) == currDate).ToArray();
                    Console.Out.WriteLine("DefaultMovementsSummary handling date:" + currDate + " dayMovements#: " + dayMovements.Length);

                    var openDrawerMov  = dayMovements.Where(m => m.Source == SourceType.Drawer && m.Type == MovementType.Open).OrderBy(m => m.Date).ToArray();
                    var closeDrawerMov = dayMovements.Where(m => m.Source == SourceType.Drawer && m.Type == MovementType.Close).OrderBy(m => m.Date).ToArray();
                    Console.Out.WriteLine("DefaultMovementsSummary handling date:" + currDate + " openDrawerMov#: " + openDrawerMov.Length + " closeDrawerMov#:" + closeDrawerMov.Length);

                    var dayCardMovements = dayMovements.Where(m => m.Source == SourceType.Card && !m.IsAnExpense());
                    var cardAmount =  dayCardMovements.Sum(m => m.Amount);
                    
                    var dayCardMovementsToProcess = dayCardMovements;

                    var expenseAmount = dayMovements.Where(m => m.IsAnExpense()).Sum(m => m.Amount);
                    totExpenseAmount += expenseAmount;
                    totCardAmount += cardAmount;

                    if (openDrawerMov.Length != closeDrawerMov.Length) {
                        Console.Error.WriteLine(currDate + " drawer hasn't been closed properly");
                        daySummaries.Add(MovementSummary.CreateDay(currDate,cashAmount:0,cardAmount,expenseAmount,new string[] {"drawer hasn't been closed properly"}));
                    }
                    else {
                        for (int i=0;i<openDrawerMov.Length;i++) {
                            var cashDiff = closeDrawerMov[i].Amount - openDrawerMov[i].Amount;
                            Console.Out.WriteLine("DefaultMovementsSummary handling date:" + currDate + " close: " + closeDrawerMov[i].Amount + " open:" + openDrawerMov[i].Amount);

                            totCashAmount += cashDiff;
                            decimal partialCardAmount = 0;
                            var dtDeltaToCloseDrawerMov = closeDrawerMov[i].Date.AddSeconds(2);
                            partialCardAmount = dayCardMovementsToProcess.Where(m => m.Date <= dtDeltaToCloseDrawerMov).Sum(m => m.Amount);

                            daySummaries.Add(MovementSummary.CreateDay(currDate,cashDiff,partialCardAmount,expenseAmount));
                            dayCardMovementsToProcess = dayCardMovementsToProcess.Where(m => m.Date >= dtDeltaToCloseDrawerMov);
                        }
                    }


                    currDate = currDate.AddDays(1);
                }
                CashAmount = totCashAmount;
                CardAmount = totCardAmount;
                ExpenseAmount = totExpenseAmount;
                DaySummaries = daySummaries;
            }
        }

        public DateOnly FromDate {get;}
        public DateOnly ToDate {get;}
        public decimal CashAmount {get;}
        public decimal CardAmount {get;}
        public decimal ExpenseAmount {get;}
        public IEnumerable<IDaySummary> DaySummaries {get;} = Enumerable.Empty<IDaySummary>();

    }

    public class DefaultDaySummary(DateOnly date,decimal cashAmount,decimal cardAmount,decimal expenseAmount,IEnumerable<string> notes) : IDaySummary {
        public DateOnly Date {get;} = date;
        public decimal CashAmount {get;} = cashAmount;
        public decimal CardAmount {get;} = cardAmount;

        public decimal ExpenseAmount {get;} = expenseAmount;

        public IEnumerable<string> Notes => notes;
    }

    public static class SummaryHelper {
        public static decimal GetTotal(this IMovementsSummary movementsSummary) => movementsSummary.CardAmount + movementsSummary.CashAmount;
        public static decimal GetTotalNet(this IMovementsSummary movementsSummary) => movementsSummary.CardAmount + movementsSummary.CashAmount - movementsSummary.ExpenseAmount;
        public static decimal GetTotal(this IDaySummary daySummary) => daySummary.CardAmount + daySummary.CashAmount;

    }

}
