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
    public class JsonExpensesPersistencyTest
    {
        private IFileSystem? _fileSystem;
        [SetUp]
        public void SetUp()
        {
            _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { @"/data/shopFlowMovs/Expenses/2025-01/2025-01-01_17-02-24_Expense_EXP.json", new MockFileData("{\"Date\":\"2025-01-01T17:02:24.5023348Z\",\"Amount\":41.2,\"ExpType\":\"Test EXP1\",\"Supplies\":\"Test Suppl1\",\"Description\":\"\",\"Source\":1,\"Type\":4}") },
                    { @"/data/shopFlowMovs/Expenses/2025-01/2025-01-02_18-02-24_Expense_EXP.json", new MockFileData("{\"Date\":\"2025-01-02T18:02:24.5023348Z\",\"Amount\":42.2,\"ExpType\":\"Test EXP1\",\"Supplies\":\"Test Suppl1\",\"Description\":\"\",\"Source\":1,\"Type\":4}") },
                    { @"/data/shopFlowMovs/Expenses/2025-02/2025-02-01_15-02-24_Expense_EXP.json", new MockFileData("{\"Date\":\"2025-02-01T15:02:24.5023348Z\",\"Amount\":3.3,\"ExpType\":\"Test EXP1\",\"Supplies\":\"Test Suppl1\",\"Description\":\"\",\"Source\":1,\"Type\":4}") },
                    { @"/data/shopFlowMovs/Expenses/2025-03/2025-03-03_20-11-25_Expense_EXP.json", new MockFileData("{\"Date\":\"2025-03-03T20:11:25.5023348Z\",\"Amount\":2.3,\"ExpType\":\"Test EXP1\",\"Supplies\":\"Test Suppl1\",\"Description\":\"\",\"Source\":1,\"Type\":4}") },
                });
        }

        [Test]
        public void LoadMovements_NoMatch()
        {
            JsonExpensesPersistency jsonExpensesPersistency = new JsonExpensesPersistency(_fileSystem!);
            var movements = jsonExpensesPersistency.LoadExpenses(new DateOnly(2024, 01, 01), new DateOnly(2024, 01, 02));
            movements.Count().Should().Be(0);
        }


        [Test]
        public void LoadMovements_SamePeriod_Success()
        {
            JsonExpensesPersistency jsonExpensesPersistency = new JsonExpensesPersistency(_fileSystem!);
            var movements = jsonExpensesPersistency.LoadExpenses(new DateOnly(2025,01,01), new DateOnly(2025, 01, 02));
            movements.Count().Should().Be(2);  
        }

        [Test]
        public void LoadMovements_DifferentPeriods_Success()
        {
            JsonExpensesPersistency jsonExpensesPersistency = new JsonExpensesPersistency(_fileSystem!);
            var movements = jsonExpensesPersistency.LoadExpenses(new DateOnly(2025, 01, 01), new DateOnly(2025, 02, 02));
            movements.Count().Should().Be(3);
        }

        [Test]
        public void Save_VerifyItsPresent_Success()
        {
            JsonExpensesPersistency jsonExpensesPersistency = new JsonExpensesPersistency(_fileSystem!);
            jsonExpensesPersistency.Save(
                Expense.Create(
                    Movement.Create(new DateTime(2025, 01, 03), 6.4m, MovementType.Deposit, SourceType.Card),
                    ExpenseType.Food,"Supplies4Test", "Description4Test"));
            
            var movements = jsonExpensesPersistency.LoadExpenses(new DateOnly(2025, 01, 01), new DateOnly(2025, 01, 03));
            movements.Count().Should().Be(3);
        }

        [Test]
        public void Delete_VerifyDeletion_Success()
        {
            JsonExpensesPersistency jsonExpensesPersistency = new JsonExpensesPersistency(_fileSystem!);
            var expenses = jsonExpensesPersistency.LoadExpenses(new DateOnly(2025, 01, 01), new DateOnly(2025, 01, 01));
            expenses.Count().Should().Be(1);
            jsonExpensesPersistency.Delete(expenses.First()).Should().BeTrue();
            var movementsAfterDelete = jsonExpensesPersistency.LoadExpenses(new DateOnly(2025, 01, 01), new DateOnly(2025, 01, 01));
            movementsAfterDelete.Count().Should().Be(0);
        }

        [Test]
        public void Delete_NotMatchingMovement_Success()
        {
            JsonExpensesPersistency jsonExpensesPersistency = new JsonExpensesPersistency(_fileSystem!);
            var notExistingMovement = Expense.Create(
                Movement.Create(new DateTime(2024, 01, 01), 6.4m, MovementType.Deposit, SourceType.Card),
                "Food", "Supplies4Test", "Description4Test");

            jsonExpensesPersistency.Delete(notExistingMovement).Should().BeFalse();
        }

    }
}
