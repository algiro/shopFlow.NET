using FluentAssertions;
using NUnit.Framework;
using shopFlow.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shopFlow.Test.Test.Services
{
    public class MovementSummaryTest
    {
        [Test]
        public void CreateSummary_NoMovements()
        {
            // Arrange
            // Act
            var summary = MovementSummary.Create(Enumerable.Empty<IMovement>());
            // Assert
            summary.DaySummaries.Should().BeEmpty();
            summary.CashAmount.Should().Be(0);
            summary.CardAmount.Should().Be(0);
            summary.ExpenseAmount.Should().Be(0);
            summary.FromDate.Should().Be(DateOnly.MinValue);
            summary.ToDate.Should().Be(DateOnly.MinValue);
        }

        [Test]
        public void CreateSummary_OneOpenMovement()
        {
            // Arrange
            DateTime firstJan = new DateTime(2025, 1, 1);
            IEnumerable<IMovement> movements = [
                Movement.Create(firstJan, 100, MovementType.Open, SourceType.Drawer)
            ];
            // Act
            var summary = MovementSummary.Create(movements);
            // Assert
            summary.DaySummaries.Count().Should().Be(1);
            // no close movement, so cash amount is 0
            summary.DaySummaries.First().CashAmount.Should().Be(0);    
            summary.CashAmount.Should().Be(0);
            summary.CardAmount.Should().Be(0);
            summary.ExpenseAmount.Should().Be(0);
            summary.FromDate.Should().Be(DateOnly.FromDateTime(firstJan));
            summary.ToDate.Should().Be(DateOnly.FromDateTime(firstJan));
        }

        [Test]
        public void CreateSummary_OpenCloseMovement()
        {
            // Arrange
            DateTime firstJan = new DateTime(2025, 1, 1);
            IEnumerable<IMovement> movements = [
                Movement.Create(firstJan, 100, MovementType.Open, SourceType.Drawer),
                Movement.Create(firstJan, 145, MovementType.Close, SourceType.Drawer)
            ];
            // Act
            var summary = MovementSummary.Create(movements);
            // Assert
            summary.DaySummaries.Count().Should().Be(1);
            // 145 - 100 = 45
            summary.DaySummaries.First().CashAmount.Should().Be(45); 
            summary.CashAmount.Should().Be(45);
            summary.CardAmount.Should().Be(0);
            summary.ExpenseAmount.Should().Be(0);
            summary.FromDate.Should().Be(DateOnly.FromDateTime(firstJan));
            summary.ToDate.Should().Be(DateOnly.FromDateTime(firstJan));
        }

        [Test]
        public void CreateSummary_OpenCloseMovementAndCard()
        {
            // Arrange
            DateTime firstJan = new DateTime(2025, 1, 1);
            IEnumerable<IMovement> movements = [
                Movement.Create(firstJan, 100, MovementType.Open, SourceType.Drawer),
                Movement.Create(firstJan, 145, MovementType.Close, SourceType.Drawer),
                Movement.Create(firstJan, 162, MovementType.Deposit, SourceType.Card)
            ];
            // Act
            var summary = MovementSummary.Create(movements);
            // Assert
            summary.DaySummaries.Count().Should().Be(1);
            // 145 - 100 = 45
            summary.DaySummaries.First().CashAmount.Should().Be(45);
            summary.DaySummaries.First().CardAmount.Should().Be(162);
            summary.CashAmount.Should().Be(45);
            summary.CardAmount.Should().Be(162);
            summary.ExpenseAmount.Should().Be(0);
            summary.FromDate.Should().Be(DateOnly.FromDateTime(firstJan));
            summary.ToDate.Should().Be(DateOnly.FromDateTime(firstJan));
        }

        [Test]
        public void CreateSummary_MultipleOpenCloseMovementAndCard()
        {
            // Arrange
            DateTime firstJan = new DateTime(2025, 1, 1);
            IEnumerable<IMovement> movements = [
                Movement.Create(firstJan, 100, MovementType.Open, SourceType.Drawer),
                Movement.Create(firstJan.AddHours(3), 145, MovementType.Close, SourceType.Drawer),    // 45 cash = 145 - 100
                Movement.Create(firstJan.AddHours(3.2), 162, MovementType.Deposit, SourceType.Card),
                Movement.Create(firstJan.AddHours(4), 145, MovementType.Open, SourceType.Drawer),
                Movement.Create(firstJan.AddHours(5), 195, MovementType.Close, SourceType.Drawer),     // 50 cash = 195 - 145, Total cash = 45 + 50 = 95
                Movement.Create(firstJan.AddHours(3.2), 38, MovementType.Deposit, SourceType.Card),    // Total card = 162 + 38 = 200

            ];
            // Act
            var summary = MovementSummary.Create(movements);
            // Assert
            summary.DaySummaries.Count().Should().Be(1);
            // 145 - 100 = 45
            summary.DaySummaries.First().CashAmount.Should().Be(95);
            summary.DaySummaries.First().CardAmount.Should().Be(200);
            summary.CashAmount.Should().Be(95);
            summary.CardAmount.Should().Be(200);
            summary.ExpenseAmount.Should().Be(0);
            summary.FromDate.Should().Be(DateOnly.FromDateTime(firstJan));
            summary.ToDate.Should().Be(DateOnly.FromDateTime(firstJan));
        }

        [Test]
        public void CreateSummary_MultipleDays_MultipleOpenCloseMovementAndCard()
        {
            // Arrange
            DateTime firstJan = new DateTime(2025, 1, 1);
            DateTime thirdJan = new DateTime(2025, 1, 3);

            IEnumerable<IMovement> movements = [
                Movement.Create(firstJan, 100, MovementType.Open, SourceType.Drawer),
                Movement.Create(firstJan.AddHours(3), 145, MovementType.Close, SourceType.Drawer),    // 45 cash = 145 - 100
                Movement.Create(firstJan.AddHours(3.2), 162, MovementType.Deposit, SourceType.Card),
                Movement.Create(firstJan.AddHours(4), 145, MovementType.Open, SourceType.Drawer),
                Movement.Create(firstJan.AddHours(5), 195, MovementType.Close, SourceType.Drawer),     // 50 cash = 195 - 145, Total cash = 45 + 50 = 95
                Movement.Create(firstJan.AddHours(3.2), 38, MovementType.Deposit, SourceType.Card),    // Total card = 162 + 38 = 200
                
                Movement.Create(thirdJan, 76, MovementType.Open, SourceType.Drawer),
                Movement.Create(thirdJan.AddHours(3), 145, MovementType.Close, SourceType.Drawer),    // 69 cash = 145 - 76
                Movement.Create(thirdJan.AddHours(3.2), 121.5m, MovementType.Deposit, SourceType.Card),
                Movement.Create(thirdJan.AddHours(4), 101, MovementType.Open, SourceType.Drawer),
                Movement.Create(thirdJan.AddHours(5), 195, MovementType.Close, SourceType.Drawer),     // 94 cash = 195 - 101, Total cash = 69 + 94 = 163
                Movement.Create(thirdJan.AddHours(3.2), 24.1m, MovementType.Deposit, SourceType.Card), // Total card = 121.5 + 24.1 = 145.6

            ];
            // Act
            var summary = MovementSummary.Create(movements);
            // Assert
            summary.DaySummaries.Count().Should().Be(2);
            // 145 - 100 = 45
            var firstJanSummary = summary.DaySummaries.Single(d => d.Date == DateOnly.FromDateTime(firstJan));
            var thirdJanSummary = summary.DaySummaries.Single(d => d.Date == DateOnly.FromDateTime(thirdJan));

            firstJanSummary.CashAmount.Should().Be(95);
            firstJanSummary.CardAmount.Should().Be(200);

            thirdJanSummary.CashAmount.Should().Be(163);
            thirdJanSummary.CardAmount.Should().Be(145.6m);

            summary.CashAmount.Should().Be(258); // 95 + 163
            summary.CardAmount.Should().Be(345.6m); // 200 + 145.6
            summary.ExpenseAmount.Should().Be(0);
            summary.FromDate.Should().Be(DateOnly.FromDateTime(firstJan));
            summary.ToDate.Should().Be(DateOnly.FromDateTime(thirdJan));
        }

    }
}
