namespace Parking.Api.DTOs
{
    public class ParkResponse
    {
        public string VehicleReg { get; set; } = null!;
        public int SpaceNumber { get; set; }
        public DateTime TimeIn { get; set; }
    }
}
