using System;
using shopFlow.Services;

namespace shopFlow.Persistency;

public interface IMovementPersistency
{
    bool Save(IMovement movement);
    bool Delete(IMovement movement);
    IEnumerable<IMovement> LoadMovements(DateOnly fromDate, DateOnly toDate);

}
