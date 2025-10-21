namespace Parking.Api.DTOs
{
    public class ParkingStatusResponse
    {
        public int AvailableSpaces { get; set; }
        public int OccupiedSpaces { get; set; }
    }
}