using Parking.Api.Models;

namespace Parking.Api.Data
{
    public static class DbSeeder
    {
        public static void Seed(ParkingContext ctx, int numberOfSpaces = 10)
        {
            if (ctx.ParkingSpaces.Any()) return;

            for (int i = 1; i <= numberOfSpaces; i++)
            {
                ctx.ParkingSpaces.Add(new ParkingSpace { Id = i, IsOccupied = false });
            }
            ctx.SaveChanges();
        }
    }
}