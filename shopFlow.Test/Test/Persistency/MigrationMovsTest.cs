using NUnit.Framework;
using shopFlow.Persistency;
using shopFlow.Persistency.Impl;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using FluentAssertions;

namespace shopFlow.Test.Test.Persistency
{
    public class MigrationMovsTest
    {
        private IFileSystem? _fileSystem;
        private LiteDatabase? _db;
        [SetUp]
        public void SetUp()
        {
            _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { @"/data/shopFlowMovs/Movements/2025-01/2025-01-01_20-02-24_Open_Drawer_MOV.json", new MockFileData("{\"Date\":\"2025-01-01T20:02:24.8632138\",\"Amount\":10.0,\"Type\":\"Open\",\"Source\":\"Drawer\"}") },
                    { @"/data/shopFlowMovs/Movements/2025-01/2025-01-02_18-02-24_Open_Drawer_MOV.json", new MockFileData("{\"Date\":\"2025-01-02T18:02:24.8632138\",\"Amount\":9.0,\"Type\":\"Open\",\"Source\":\"Drawer\"}") },
                    { @"/data/shopFlowMovs/Movements/2025-02/2025-02-01_15-02-24_Open_Drawer_MOV.json", new MockFileData("{\"Date\":\"2025-02-01T15:02:24.8632138\",\"Amount\":8.0,\"Type\":\"Open\",\"Source\":\"Drawer\"}") },
                });
            MemoryStream memoryStream = new MemoryStream();
            _db = new LiteDatabase(memoryStream);
        }

        [Test]
        public void Start_FromJsonToLiteDB_Success()
        {
            var source = new JsonMovementPersistency(_fileSystem!);
            var dest = new LiteDBMovementPersistency(_db!);
            var migration = new MigrationMovs(source, dest);
            migration.Start(new DateOnly(2024, 01, 01),new DateOnly(2026,01,01));
            var destMovements = dest.LoadMovements(new DateOnly(2025, 01, 01), new DateOnly(2025, 02, 01));
            destMovements.Count().Should().Be(3);
            var sourceMovements = source.LoadMovements(new DateOnly(2025, 01, 01), new DateOnly(2025, 02, 01));
            sourceMovements.Count().Should().Be(3);
            foreach (var sourceMov in sourceMovements)
            {
                sourceMov.Equals(destMovements.Single(d => d.Id == sourceMov.Id)).Should().BeTrue();
            }

        }
    }
}
