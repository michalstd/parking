namespace Parking.Api.Models
{
    public class ParkingRecord
    {
        public int Id { get; set; }
        public string VehicleReg { get; set; } = null!;
        public int VehicleType { get; set; }
        public int SpaceNumber { get; set; }
        public DateTime TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public double? Charge { get; set; }
    }
}