using LiteDB;
using shopFlow.Components.Pages;
using shopFlow.Services;
using shopFlow.Utils;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace shopFlow.Persistency.Impl
{
    public class LiteDBMovementPersistency : IMovementPersistency
    {
        private readonly TimeOnly DAY_TIME_START = new TimeOnly(0, 0, 0);
        private readonly TimeOnly DAY_TIME_END = new TimeOnly(23, 59, 59);
        private readonly ILogger _logger = LoggerUtils.CreateLogger<LiteDBMovementPersistency>();
        private readonly LiteDatabase _db;
        private const string MOVEMENTS_COLLECTION = "movements";
        public LiteDBMovementPersistency(LiteDatabase db)
        {
            _db = db;
            _db.GetCollection<DefaultMovement>(MOVEMENTS_COLLECTION).EnsureIndex(m => m.Id,unique:true);
            _db.GetCollection<DefaultMovement>(MOVEMENTS_COLLECTION).EnsureIndex(m => m.Date);
        }
        public bool Delete(IMovement movement)
        {
            try
            {
                var hasBeenDeleted = _db.GetCollection<DefaultMovement>(MOVEMENTS_COLLECTION).Delete(movement.Id);
                _logger.LogInformation("Delete movement: {Movement} deleted:{HasBeenDeleted}", movement, hasBeenDeleted);
                return hasBeenDeleted;
            }
            finally
            {
                _db.Checkpoint();
            }
        }
        public IEnumerable<IMovement> LoadMovements(DateOnly fromDate, DateOnly toDate)
        {
            _logger.LogInformation("LoadMovements movement: {FromDate} to {ToDate}", fromDate, toDate);
            var movements = _db.GetCollection<DefaultMovement>(MOVEMENTS_COLLECTION).Query().Where(m => m.Date >= fromDate.ToDateTime(DAY_TIME_START) && m.Date <= toDate.ToDateTime(DAY_TIME_END)).ToList();
            _logger.LogInformation("LoadMovements movement: {FromDate} to {ToDate} loaded#:{MovCount}", fromDate, toDate, movements.Count);
            return movements;
        }

        public bool Save(IMovement movement)
        {
            try
            {
                _logger.LogInformation("Save movement: {Movement}", movement);
                var movColl = _db.GetCollection<DefaultMovement>(MOVEMENTS_COLLECTION);
                var storedMovement = movColl.FindById(movement.Id);
                if (storedMovement != null)
                {
                    var hasBeenUpdated = movColl.Update(movement.AsDefaultMovement());
                    _logger.LogInformation("Update movement: {Movement} hasBeenUpdated:{hasBeenUpdated}", movement, hasBeenUpdated);
                    return true;
                }
                else
                {
                    var idMov = movColl.Insert(movement.AsDefaultMovement());
                    _logger.LogInformation("Insert movement: {Movement} with Id:{Id}", movement, idMov);
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
