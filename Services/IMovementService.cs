using shopFlow.Persistency;

namespace shopFlow.Services {

    public interface IMovementService {
        bool Open(DateTime dateTime, decimal amount);
        bool Close(DateTime dateTime, decimal cashAmount,(bool IsPartial, decimal Amount) card);
        IEnumerable<IMovement> GetMovements((int Year,int Month) period);
        IEnumerable<IMovement> GetMovements(DateOnly fromDate,DateOnly toDate);
        bool Update(IMovement movement);
        bool Delete(IMovement movement);

    }

    public class DefaultMovementService : IMovementService
    {
        public bool Open(DateTime dateTime, decimal amount)
        {
            try {
                var movement = Movement.Create(dateTime,amount,MovementType.Open,SourceType.Drawer);
                return MovementPersistency.Save(movement);
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error on MovementService.Open " + dateTime + " amount:" + amount + " error:" + ex);
                return false;
            }

        }
        public bool Close(DateTime dateTime, decimal cashAmount,(bool IsPartial, decimal Amount) card)
        {
            try {
                var drawerMovement = Movement.Create(dateTime,cashAmount,MovementType.Close,SourceType.Drawer);
                var drawerMovInserted = MovementPersistency.Save(drawerMovement);
                var amountToPersist = card.Amount;
                if (!card.IsPartial) { // I've to calculate the partial amount starting from the latest card movement within the same day 
                    var movDate = DateOnly.FromDateTime(dateTime);
                    var dayMovements = GetMovements(movDate,movDate);
                    var dayCardMovements = dayMovements.Where(m => m.Source==SourceType.Card).OrderBy(m => m.Date);
                    var latestCardMov = dayCardMovements.LastOrDefault();
                    if (latestCardMov != null) {
                        amountToPersist = card.Amount - latestCardMov.Amount;
                    }
                }
                var cardMovement = Movement.Create(dateTime,amountToPersist,MovementType.Deposit,SourceType.Card);
                var cardMovInserted = MovementPersistency.Save(cardMovement);

                return drawerMovInserted && cardMovInserted;
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error on MovementService.Close " + dateTime + " cashAmount:" + cashAmount + " error:" + ex);
                return false;
            }

        }

        public IEnumerable<IMovement> GetMovements((int Year,int Month) period)  => MovementPersistency.LoadMovements(period);
        public IEnumerable<IMovement> GetMovements(DateOnly fromDate,DateOnly toDate) => MovementPersistency.LoadMovements(fromDate,toDate);

        public bool Update(IMovement movement) => MovementPersistency.Save(movement);
        public bool Delete(IMovement movement) => MovementPersistency.Delete(movement);
        
    }
}