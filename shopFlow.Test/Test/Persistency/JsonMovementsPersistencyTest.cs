using FluentAssertions;
using NUnit.Framework;
using shopFlow.Persistency;
using shopFlow.Services;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace showFlow.Test.Test.Persistency
{
    public class JsonMovementsPersistencyTest
    {
        private IFileSystem? _fileSystem;
        [SetUp]
        public void SetUp()
        {
            _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { @"/data/shopFlowMovs/Movements/2025-01/2025-01-01_20-02-24_Open_Drawer_MOV.json", new MockFileData("{\"Date\":\"2025-01-01T20:02:24.8632138\",\"Amount\":10.0,\"Type\":\"Open\",\"Source\":\"Drawer\"}") },
                    { @"/data/shopFlowMovs/Movements/2025-01/2025-01-02_18-02-24_Open_Drawer_MOV.json", new MockFileData("{\"Date\":\"2025-01-02T18:02:24.8632138\",\"Amount\":9.0,\"Type\":\"Open\",\"Source\":\"Drawer\"}") },
                    { @"/data/shopFlowMovs/Movements/2025-02/2025-02-01_15-02-24_Open_Drawer_MOV.json", new MockFileData("{\"Date\":\"2025-02-01T15:02:24.8632138\",\"Amount\":8.0,\"Type\":\"Open\",\"Source\":\"Drawer\"}") },
                });
        }

        [Test]
        public void LoadMovements_NoMatch()
        {
            JsonMovementPersistency jsonMovementPersistency = new JsonMovementPersistency(_fileSystem!);
            var movements = jsonMovementPersistency.LoadMovements(new DateOnly(2024, 01, 01), new DateOnly(2024, 01, 02));
            movements.Count().Should().Be(0);
        }


        [Test]
        public void LoadMovements_SamePeriod_Success()
        {
            JsonMovementPersistency jsonMovementPersistency = new JsonMovementPersistency(_fileSystem!);
            var movements = jsonMovementPersistency.LoadMovements(new DateOnly(2025,01,01), new DateOnly(2025, 01, 02));
            movements.Count().Should().Be(2);  
        }

        [Test]
        public void LoadMovements_DifferentPeriods_Success()
        {
            JsonMovementPersistency jsonMovementPersistency = new JsonMovementPersistency(_fileSystem!);
            var movements = jsonMovementPersistency.LoadMovements(new DateOnly(2025, 01, 01), new DateOnly(2025, 02, 02));
            movements.Count().Should().Be(3);
        }

        [Test]
        public void Save_VerifyItsPresent_Success()
        {
            JsonMovementPersistency jsonMovementPersistency = new JsonMovementPersistency(_fileSystem!);
            jsonMovementPersistency.Save(Movement.Create(new DateTime(2025, 01, 03), 6.4m, MovementType.Deposit, SourceType.Card));
            var movements = jsonMovementPersistency.LoadMovements(new DateOnly(2025, 01, 01), new DateOnly(2025, 01, 03));
            movements.Count().Should().Be(3);
        }

        [Test]
        public void Delete_VerifyDeletion_Success()
        {
            JsonMovementPersistency jsonMovementPersistency = new JsonMovementPersistency(_fileSystem!);
            var movements = jsonMovementPersistency.LoadMovements(new DateOnly(2025, 01, 01), new DateOnly(2025, 01, 01));
            movements.Count().Should().Be(1);
            jsonMovementPersistency.Delete(movements.First()).Should().BeTrue();
            var movementsAfterDelete = jsonMovementPersistency.LoadMovements(new DateOnly(2025, 01, 01), new DateOnly(2025, 01, 01));
            movementsAfterDelete.Count().Should().Be(0);
        }

        [Test]
        public void Delete_NotMatchingMovement_Success()
        {
            JsonMovementPersistency jsonMovementPersistency = new JsonMovementPersistency(_fileSystem!);
            var notExistingMovement = Movement.Create(new DateTime(2024, 01, 01), 6.4m, MovementType.Deposit, SourceType.Card);            
            jsonMovementPersistency.Delete(notExistingMovement).Should().BeFalse();
        }

    }
}
