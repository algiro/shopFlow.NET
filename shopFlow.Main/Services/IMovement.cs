
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace shopFlow.Services {

    public enum MovementType { Open, Close, Withdrawal, Deposit, Expense }
    public enum SourceType { Drawer, Card }
    public interface IMovement {
        DateTime Date {get;}
        decimal Amount {get;}
        MovementType Type {get;}
        SourceType Source { get; }
    }
    public static class Movement {
        public static IMovement Create(DateTime dateTime,decimal amount, MovementType type,SourceType source) => new DefaultMovement(dateTime,amount,type,source);
    }

    public class DefaultMovement : IMovement
    {
        public DefaultMovement(DateTime date, decimal amount, MovementType type,SourceType source)
        {
            Date = date;
            Amount = amount;
            Type = type;
            Source = source;
        }

        public DateTime Date { get; }
        public decimal Amount { get; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MovementType Type { get; }

        [JsonConverter(typeof(StringEnumConverter))]
        public SourceType Source { get; }
    }

    public static class MovementHelper {
        public static string GetFileName(this IMovement movement)
            => $"{movement.Date.ToString("yyyy-MM-dd_HH-mm-ss")}_{movement.Type}_{movement.Source}_MOV.json";

        public static bool IsAnExpense(this IMovement movement)
            => movement.Type == MovementType.Withdrawal || movement.Type == MovementType.Expense;
    }
}
