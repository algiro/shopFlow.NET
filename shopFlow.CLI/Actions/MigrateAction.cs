using shopFlow.Persistency.Impl;
using shopFlow.Persistency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shopFlow.CLI.Actions
{
    public class MigrateAction : ICommandAction
    {
        public void Execute()
        {
            var shopFLowMovsPath = "/data/shopFlowMovs";
            var dbPath = Path.Combine(shopFLowMovsPath, "lite.db");
            Console.WriteLine($"Migrating from Json Files to LiteDB : {dbPath}");
            var source = new JsonMovementPersistency();
            var dest = new LiteDBMovementPersistency(new LiteDB.LiteDatabase(dbPath));
            var migration = new MigrationMovs(source, dest);
            migration.Start(new DateOnly(2024, 01, 01), new DateOnly(2026, 01, 01));
        }
    }
}
