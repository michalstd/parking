namespace Parking.Api.DTOs
{
    public class ParkRequest
    {
        public string VehicleReg { get; set; } = null!;
        public int VehicleType { get; set; } // 1,2,3
    }
}