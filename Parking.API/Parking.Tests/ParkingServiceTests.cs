using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Parking.Api.Data;
using Parking.Api.Services;
using Parking.Api.DTOs;

namespace Parking.Tests
{
    [TestFixture]
    public class ParkingServiceTests
    {
        private ParkingContext _context = null!;
        private ParkingService _service = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ParkingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ParkingContext(options);
            DbSeeder.Seed(_context, numberOfSpaces: 3);
            _service = new ParkingService(_context);
        }

        [Test]
        public async Task Park_And_Exit_Should_Calculate_Correct_Charge()
        {
            var parkResp = await _service.ParkVehicleAsync(
                new ParkRequest { VehicleReg = "ABC12345", VehicleType = 1 });

            var record = _context.ParkingRecords.First(r => r.VehicleReg == "ABC12345" && r.TimeOut == null);
            record.TimeIn = DateTime.UtcNow.AddMinutes(-12);
            await _context.SaveChangesAsync();

            var exitResp = await _service.ExitVehicleAsync(new ExitRequest { VehicleReg = "ABC12345" });

            var expectedCharge = 3.2;
            Assert.That(Math.Round(exitResp.VehicleCharge, 2), Is.EqualTo(expectedCharge));
            Assert.That(exitResp.VehicleReg, Is.EqualTo("ABC12345"));
            Assert.That(exitResp.TimeOut, Is.Not.Null);
        }

        [Test]
        public async Task Park_SameVehicleTwice_Should_Throw_InvalidOperationException()
        {
            // Arrange
            await _service.ParkVehicleAsync(new ParkRequest { VehicleReg = "XYZ111", VehicleType = 2 });

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.ParkVehicleAsync(new ParkRequest { VehicleReg = "XYZ111", VehicleType = 2 }));

            Assert.That(ex.Message, Does.Contain("already parked"));
        }

        [Test]
        public async Task Exit_NonExistingVehicle_Should_Throw_KeyNotFoundException()
        {
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _service.ExitVehicleAsync(new ExitRequest { VehicleReg = "NOTPRESENT" }));

            Assert.That(ex.Message, Does.Contain("not found"));
        }

        [Test]
        public async Task Should_Return_Correct_Status_After_Parking_Two_Cars()
        {
            var initial = await _service.GetStatusAsync();
            Assert.That(initial.AvailableSpaces, Is.EqualTo(3));
            Assert.That(initial.OccupiedSpaces, Is.EqualTo(0));

            await _service.ParkVehicleAsync(new ParkRequest { VehicleReg = "A1", VehicleType = 1 });
            await _service.ParkVehicleAsync(new ParkRequest { VehicleReg = "A2", VehicleType = 2 });

            var after = await _service.GetStatusAsync();
            Assert.That(after.AvailableSpaces, Is.EqualTo(1));
            Assert.That(after.OccupiedSpaces, Is.EqualTo(2));
        }

        [Test]
        public async Task Park_WhenFull_Should_Throw_InvalidOperationException()
        {
            await _service.ParkVehicleAsync(new ParkRequest { VehicleReg = "C1", VehicleType = 1 });
            await _service.ParkVehicleAsync(new ParkRequest { VehicleReg = "C2", VehicleType = 2 });
            await _service.ParkVehicleAsync(new ParkRequest { VehicleReg = "C3", VehicleType = 3 });

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.ParkVehicleAsync(new ParkRequest { VehicleReg = "C4", VehicleType = 1 }));

            Assert.That(ex.Message, Does.Contain("No available space"));
        }

        [TearDown]
        public void Cleanup()
        {
            _context.Dispose();
        }
    }
}