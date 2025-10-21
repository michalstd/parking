using Microsoft.EntityFrameworkCore;
using Parking.Api.Models;

namespace Parking.Api.Data
{
    public class ParkingContext : DbContext
    {
        public ParkingContext(DbContextOptions<ParkingContext> options)
            : base(options) { }

        public DbSet<ParkingSpace> ParkingSpaces { get; set; }
        public DbSet<ParkingRecord> ParkingRecords { get; set; }
    }
}