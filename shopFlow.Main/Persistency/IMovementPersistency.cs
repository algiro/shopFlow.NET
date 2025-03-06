using System;
using shopFlow.Persistency.Impl;
using shopFlow.Services;
using shopFlow.Utils;

namespace shopFlow.Persistency;

public enum MovPersistencyType { Json, LiteDB }

public interface IMovementPersistency
{
    bool Save(IMovement movement);
    bool Delete(IMovement movement);
    IEnumerable<IMovement> LoadMovements(DateOnly fromDate, DateOnly toDate);
}

public static class MovementPersistencyHelper
{
    private static readonly ILogger logger = LoggerUtils.CreateLogger<IMovementPersistency>();
    public static void AddMovementPersistency(this IServiceCollection services)
    {
        var movPersistencyEnv = Environment.GetEnvironmentVariable("MOV_PERSISTENCY_TYPE");
        MovPersistencyType movPersistencyType = MovPersistencyType.Json;
        Enum.TryParse<MovPersistencyType>(movPersistencyEnv, out movPersistencyType);
        logger.LogInformation("MovPersistencyType: {MovPersistencyType}", movPersistencyType);

        if (movPersistencyType == MovPersistencyType.Json)
        {
            services.AddSingleton<IMovementPersistency, JsonMovementPersistency>();
        }
        else
        {
            services.AddSingleton(new LiteDB.LiteDatabase("/data/shopFlowMovs/lite.db"));
            services.AddSingleton<IMovementPersistency, LiteDBMovementPersistency>();
        }
    }
}
