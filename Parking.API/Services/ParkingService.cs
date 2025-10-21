using Microsoft.EntityFrameworkCore;
using Parking.Api.Data;
using Parking.Api.DTOs;
using Parking.Api.Models;

namespace Parking.Api.Services
{
    public class ParkingService : IParkingService
    {
        private readonly ParkingContext _ctx;
        private readonly Dictionary<int, double> _rates = new()
        {
            {1, 0.10}, {2, 0.20}, {3, 0.40}
        };

        public ParkingService(ParkingContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<ParkResponse> ParkVehicleAsync(ParkRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.VehicleReg))
                throw new ArgumentException("VehicleReg required");

            if (!_rates.ContainsKey(request.VehicleType))
                throw new ArgumentException("VehicleType must be 1,2 or 3");

            // Checking if already parked
            var existing = await _ctx.ParkingRecords
                .Where(r => r.VehicleReg == request.VehicleReg && r.TimeOut == null)
                .FirstOrDefaultAsync();

            if (existing != null)
                throw new InvalidOperationException("Vehicle already parked");

            // finding the first available space
            var space = await _ctx.ParkingSpaces
                .Where(s => !s.IsOccupied)
                .OrderBy(s => s.Id)
                .FirstOrDefaultAsync();

            if (space == null)
                throw new InvalidOperationException("No available space");

            space.IsOccupied = true;

            var now = DateTime.UtcNow;
            var record = new ParkingRecord
            {
                VehicleReg = request.VehicleReg,
                VehicleType = request.VehicleType,
                SpaceNumber = space.Id,
                TimeIn = now
            };

            _ctx.ParkingRecords.Add(record);
            await _ctx.SaveChangesAsync();

            return new ParkResponse
            {
                VehicleReg = record.VehicleReg,
                SpaceNumber = record.SpaceNumber,
                TimeIn = record.TimeIn
            };
        }

        public async Task<ParkingStatusResponse> GetStatusAsync()
        {
            var total = await _ctx.ParkingSpaces.CountAsync();
            var occupied = await _ctx.ParkingSpaces.CountAsync(s => s.IsOccupied);
            return new ParkingStatusResponse
            {
                AvailableSpaces = total - occupied,
                OccupiedSpaces = occupied
            };
        }

        public async Task<ExitResponse> ExitVehicleAsync(ExitRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.VehicleReg))
                throw new ArgumentException("VehicleReg required");

            var record = await _ctx.ParkingRecords
                .Where(r => r.VehicleReg == request.VehicleReg && r.TimeOut == null)
                .FirstOrDefaultAsync();

            if (record == null)
                throw new KeyNotFoundException("Vehicle not found");

            var timeOut = DateTime.UtcNow;
            record.TimeOut = timeOut;

            // Calculation of minutes parked (rounded down to full minutes)
            var totalMinutes = (int)Math.Floor((timeOut - record.TimeIn).TotalMinutes);
            if (totalMinutes < 0) totalMinutes = 0;

            // base charge
            if (!_rates.TryGetValue(record.VehicleType, out var rate))
                throw new InvalidOperationException("Invalid vehicle type in record");

            var baseCharge = totalMinutes * rate;

            // additional add £1 every full 5 minutes
            var extraBlocks = totalMinutes / 5;
            var extraCharge = extraBlocks * 1.0;

            var totalCharge = baseCharge + extraCharge;

            record.Charge = totalCharge;

            // free space
            var space = await _ctx.ParkingSpaces.FindAsync(record.SpaceNumber);
            if (space != null) space.IsOccupied = false;

            await _ctx.SaveChangesAsync();

            return new ExitResponse
            {
                VehicleReg = record.VehicleReg,
                VehicleCharge = Math.Round(totalCharge, 2),
                TimeIn = record.TimeIn,
                TimeOut = record.TimeOut.Value
            };
        }
    }
}