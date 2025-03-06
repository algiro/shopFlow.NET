using shopFlow.Persistency.Impl;
using shopFlow.Persistency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace shopFlow.CLI.Actions
{
    public class MigrateJsonToLiteDBAction : ICommandAction
    {
        public void Execute()
        {
            var shopFLowMovsPath = "/data/shopFlowMovs";
            var dbPath = Path.Combine(shopFLowMovsPath, "lite.db");
            Console.WriteLine($"Migrating from Json Files to LiteDB : {dbPath}");
            var startingDate = new DateOnly(2024, 01, 01);
            var endingDate = new DateOnly(2026, 01, 01);

            using (var db = new LiteDatabase(dbPath))
            {
                Console.WriteLine($"Migrating Movements : {dbPath}");

                var sourceMovPersistency = new JsonMovementPersistency();
                var destMovPersistency = new LiteDBMovementPersistency(db);
                var migrationMovs = new MigrationMovs(sourceMovPersistency, destMovPersistency);
                migrationMovs.Start(startingDate, endingDate);
                
                Console.WriteLine($"Migrating Expenses : {dbPath}");
                var sourceExpPersistency = new JsonExpensesPersistency();
                var destExpPersistency = new LiteDBExpensesPersistency(db);
                var migrationExp = new MigrationExpenses(sourceExpPersistency, destExpPersistency);
                migrationExp.Start(startingDate, endingDate);

                db.Checkpoint();
            }
            Console.WriteLine($"Migrating from Json Files to LiteDB : {dbPath} successfully completed!");

        }
    }
}
