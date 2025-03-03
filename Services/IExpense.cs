
using Newtonsoft.Json;

namespace shopFlow.Services {

    public enum ExpenseType { Food, Drink, Design, Decoration, Stationary, Equipment, Utensils, Other }
    public interface IExpense : IMovement {
        ExpenseType ExpenseType {get;}
        string ExpType { get; }
        string Supplies {get;}
        string  Description {get;}
    }
    public static class Expense {
        public static IExpense Create(IMovement movement, ExpenseType type,string supplies, string description) => new DefaultExpense(movement, type, supplies, description);
        public static IExpense Create(IMovement movement, string expType,string supplies, string description) => new DefaultExpense(movement, expType, supplies, description);
    }

    public class DefaultExpense : IExpense
    {
        [JsonConstructor]
        public DefaultExpense(DateTime date, decimal amount, MovementType type,SourceType source,ExpenseType expenseType,string expType,string supplies, string description) {
            Date = date;
            Amount = amount;
            Source = source;
            Type = type;
            if (string.IsNullOrEmpty(expType)) {
                ExpenseType = expenseType;
                ExpType = ExpenseType.ToString();
            }
            else {
                ExpType = expType;
            }
            Supplies = supplies;
            Description = description;

        }

        public DefaultExpense(IMovement movement, ExpenseType expenseType,string supplies, string description):this(movement.Date,movement.Amount,movement.Type,movement.Source,expenseType,expType:"",supplies,description)
        {
        }
        public DefaultExpense(IMovement movement, string expType,string supplies, string description):this(movement.Date,movement.Amount,movement.Type,movement.Source,ExpenseType.Other, expType,supplies,description)
        {
        }

        public DateTime Date { get; }
        public decimal Amount { get; }

        [JsonIgnore]
        public ExpenseType ExpenseType {get;}
        public string ExpType {get;}
        public string Supplies {get;}
        public string  Description {get;}
        public SourceType Source {get;}
        public MovementType Type {get;}
    }

    public static class ExpenseHelper {
        public static string GetFileName(this IExpense expense)
            => $"{expense.Date.ToString("yyyy-MM-dd_HH-mm-ss")}_{expense.Type}_EXP.json";
    }
}
