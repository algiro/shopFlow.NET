using shopFlow.Persistency;
using shopFlow.Utils;

namespace shopFlow.Services
{

    public interface IMovementService
    {
        bool Open(DateTime dateTime, decimal amount);
        bool Close(DateTime dateTime, decimal cashAmount, (bool IsPartial, decimal Amount) card);
        IEnumerable<IMovement> GetMovements(DateOnly fromDate, DateOnly toDate);
        bool Update(IMovement movement);
        bool Delete(IMovement movement);

    }

    public class DefaultMovementService(IMovementPersistency movementPersistency) : IMovementService
    {
        private static readonly ILogger logger = LoggerUtils.CreateLogger<IMovementService>();
        public bool Open(DateTime dateTime, decimal amount)
        {
            try
            {
                var movement = Movement.Create(dateTime, amount, MovementType.Open, SourceType.Drawer);
                return movementPersistency.Save(movement);
            }
            catch (Exception ex)
            {
                logger.LogError("Error on MovementService.Open {DateTime}, amount:{Amount}, error:{Error}", dateTime , amount , ex);
                return false;
            }

        }
        public bool Close(DateTime dateTime, decimal cashAmount, (bool IsPartial, decimal Amount) card)
        {
            try
            {
                var drawerMovement = Movement.Create(dateTime, cashAmount, MovementType.Close, SourceType.Drawer);
                var drawerMovInserted = movementPersistency.Save(drawerMovement);
                var amountToPersist = card.Amount;
                if (!card.IsPartial)
                { // I've to calculate the partial amount starting from the latest card movement within the same day 
                    var movDate = DateOnly.FromDateTime(dateTime);
                    var dayMovements = GetMovements(movDate, movDate);
                    var dayCardMovements = dayMovements.Where(m => m.Source == SourceType.Card).OrderBy(m => m.Date);
                    var latestCardMov = dayCardMovements.LastOrDefault();
                    if (latestCardMov != null)
                    {
                        amountToPersist = card.Amount - latestCardMov.Amount;
                        if (amountToPersist < 0)
                        {
                            logger.LogWarning("Close: card amount is less than the latest card movement amount, no movement will be saved");
                            return false;
                        }
                    }
                }
                var cardMovement = Movement.Create(dateTime, amountToPersist, MovementType.Deposit, SourceType.Card);
                var cardMovInserted = movementPersistency.Save(cardMovement);

                return drawerMovInserted && cardMovInserted;
            }
            catch (Exception ex)
            {
                logger.LogError("Error on MovementService.Close {DateTime}, cashAmount:{CashAmount}, error:{Error}", dateTime, cashAmount, ex);
                return false;
            }

        }

        public IEnumerable<IMovement> GetMovements(DateOnly fromDate, DateOnly toDate) => movementPersistency.LoadMovements(fromDate, toDate);
        public bool Update(IMovement movement) => movementPersistency.Save(movement);
        public bool Delete(IMovement movement) => movementPersistency.Delete(movement);

    }
}