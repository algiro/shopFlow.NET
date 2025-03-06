using shopFlow.Utils;

namespace shopFlow.Persistency
{
    public class MigrationMovs(IMovementPersistency movementPersistencySource, IMovementPersistency movementPersistencyDest)
    {
        private static readonly ILogger _logger = LoggerUtils.CreateLogger<MigrationMovs>();
        public void Start(DateOnly startDate,DateOnly endDate)
        {
            var movExported = movementPersistencySource.LoadMovements(startDate, endDate);
            var movExportedCount = movExported.Count();
            _logger.LogInformation("Start {FromDate} to {ToDate} loaded#:{MovCount}", startDate, endDate, movExportedCount);
            foreach (var movement in movExported)
            {
                movementPersistencyDest.Save(movement);
            }
            var movImported = movementPersistencyDest.LoadMovements(startDate, endDate);
            _logger.LogInformation("Verify dest {FromDate} to {ToDate} loaded#:{MovCount}", startDate, endDate, movImported.Count());
            if (movImported.Count() != movExportedCount)
            {
                _logger.LogError("Error migrating movements from {FromDate} to {ToDate} exported#:{MovExportedCount} imported#:{MovImportedCount}", startDate, endDate, movExportedCount, movImported.Count());
            }
        }
    }
}
