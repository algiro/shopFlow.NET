using FluentAssertions;
using Moq;
using NUnit.Framework;
using shopFlow.Persistency;
using shopFlow.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shopFlow.Test.Test.Services
{
    public class MovementServiceTest
    {
        [Test]
        public void Open_VerifyPersistencyCall()
        {
            DateTime firstJan = new DateTime(2025, 1, 1);
            // Arrange
            var movementPersistencyMock = new Mock<IMovementPersistency>();
            movementPersistencyMock.Setup(m => m.Save(It.IsAny<IMovement>()))
                .Returns(true);

            var movementService = new DefaultMovementService(movementPersistencyMock.Object);
            // Act
            var hasBeenOpened = movementService.Open(firstJan, 105);
            // Assert
            hasBeenOpened.Should().BeTrue();
        }

        [Test]
        public void Close_NotPartial_SingleDrawerMovWithNoCardMovement()
        {
            DateTime firstJan = new DateTime(2025, 1, 1);
            // Arrange
            var movementPersistencyMock = new Mock<IMovementPersistency>();
            movementPersistencyMock.Setup(m => m.LoadMovements(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .Returns([Movement.Create(firstJan,100,MovementType.Open,SourceType.Drawer)]);
            movementPersistencyMock.Setup(m => m.Save(It.IsAny<IMovement>()))
                .Returns(true);

            var movementService = new DefaultMovementService(movementPersistencyMock.Object);
            // Act
            var hasBeenClosed = movementService.Close(firstJan, 150, card: (false, 75));
            // Assert
            hasBeenClosed.Should().BeTrue();
            movementPersistencyMock.Verify(m => m.Save(It.Is<IMovement>(m => m.Type == MovementType.Close && m.Source == SourceType.Drawer && m.Amount == 150)), Times.Once);
            movementPersistencyMock.Verify(m => m.Save(It.Is<IMovement>(m => m.Type == MovementType.Deposit && m.Source == SourceType.Card && m.Amount == 75)), Times.Once);
        }
        [Test]
        public void Close_NotPartial_SingleDrawerMovWithCardMovement()
        {
            DateTime firstJan = new DateTime(2025, 1, 1);
            // Arrange
            var movementPersistencyMock = new Mock<IMovementPersistency>();
            movementPersistencyMock.Setup(m => m.LoadMovements(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .Returns([
                    Movement.Create(firstJan, 100, MovementType.Open, SourceType.Drawer),
                    Movement.Create(firstJan, 120, MovementType.Deposit, SourceType.Card)
                ]);
            movementPersistencyMock.Setup(m => m.Save(It.IsAny<IMovement>()))
                .Returns(true);

            var movementService = new DefaultMovementService(movementPersistencyMock.Object);
            // Act
            var hasBeenClosed = movementService.Close(firstJan, 150, card: (false, 135));
            // Assert
            hasBeenClosed.Should().BeTrue();
            movementPersistencyMock.Verify(m => m.Save(It.Is<IMovement>(m => m.Type == MovementType.Close && m.Source == SourceType.Drawer && m.Amount == 150)), Times.Once);
            movementPersistencyMock.Verify(m => m.Save(It.Is<IMovement>(m => m.Type == MovementType.Deposit && m.Source == SourceType.Card && m.Amount == 15)), Times.Once); // 135 - 120
        }
        [Test]
        public void Close_NotPartial_SingleDrawerMovWithCardMovementGreaterThanLastTotal()
        {
            DateTime firstJan = new DateTime(2025, 1, 1);
            // Arrange
            var movementPersistencyMock = new Mock<IMovementPersistency>();
            movementPersistencyMock.Setup(m => m.LoadMovements(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .Returns([
                    Movement.Create(firstJan, 100, MovementType.Open, SourceType.Drawer),
                    Movement.Create(firstJan, 120, MovementType.Deposit, SourceType.Card)
                ]);
            movementPersistencyMock.Setup(m => m.Save(It.IsAny<IMovement>()))
                .Returns(true);

            var movementService = new DefaultMovementService(movementPersistencyMock.Object);
            // Act
            var hasBeenClosed = movementService.Close(firstJan, 150, card: (false, 105));
            // Assert
            hasBeenClosed.Should().BeFalse();
            movementPersistencyMock.Verify(m => m.Save(It.Is<IMovement>(m => m.Type == MovementType.Close && m.Source == SourceType.Drawer && m.Amount == 150)), Times.Once);
            // the difference is negative (105 - 120), so no movement should be saved
            movementPersistencyMock.Verify(m => m.Save(It.Is<IMovement>(m => m.Type == MovementType.Deposit && m.Source == SourceType.Card)), Times.Never); 
        }
        [Test]
        public void Close_Partial_SingleDrawerMovWithNOCardMovement()
        {
            DateTime firstJan = new DateTime(2025, 1, 1);
            // Arrange
            var movementPersistencyMock = new Mock<IMovementPersistency>();
            movementPersistencyMock.Setup(m => m.LoadMovements(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .Returns([
                    Movement.Create(firstJan, 100, MovementType.Open, SourceType.Drawer)
                ]);
            movementPersistencyMock.Setup(m => m.Save(It.IsAny<IMovement>()))
                .Returns(true);

            var movementService = new DefaultMovementService(movementPersistencyMock.Object);
            // Act
            var hasBeenClosed = movementService.Close(firstJan, 150, card: (true, 135));
            // Assert
            hasBeenClosed.Should().BeTrue();
            movementPersistencyMock.Verify(m => m.Save(It.Is<IMovement>(m => m.Type == MovementType.Close && m.Source == SourceType.Drawer && m.Amount == 150)), Times.Once);
            movementPersistencyMock.Verify(m => m.Save(It.Is<IMovement>(m => m.Type == MovementType.Deposit && m.Source == SourceType.Card && m.Amount == 135)), Times.Once);
        }
        [Test]
        public void Close_Partial_SingleDrawerMovWithCardMovement()
        {
            DateTime firstJan = new DateTime(2025, 1, 1);
            // Arrange
            var movementPersistencyMock = new Mock<IMovementPersistency>();
            movementPersistencyMock.Setup(m => m.LoadMovements(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .Returns([
                    Movement.Create(firstJan, 100, MovementType.Open, SourceType.Drawer),
                    Movement.Create(firstJan, 120, MovementType.Deposit, SourceType.Card)
                ]);
            movementPersistencyMock.Setup(m => m.Save(It.IsAny<IMovement>()))
                .Returns(true);

            var movementService = new DefaultMovementService(movementPersistencyMock.Object);
            // Act
            var hasBeenClosed = movementService.Close(firstJan, 150, card: (true, 135));
            // Assert
            hasBeenClosed.Should().BeTrue();
            movementPersistencyMock.Verify(m => m.Save(It.Is<IMovement>(m => m.Type == MovementType.Close && m.Source == SourceType.Drawer && m.Amount == 150)), Times.Once);
            movementPersistencyMock.Verify(m => m.Save(It.Is<IMovement>(m => m.Type == MovementType.Deposit && m.Source == SourceType.Card && m.Amount == 135)), Times.Once); 
        }
    }
}
