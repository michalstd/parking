using Parking.Api.DTOs;

namespace Parking.Api.Services
{
    public interface IParkingService
    {
        Task<ParkResponse> ParkVehicleAsync(ParkRequest request);
        Task<ParkingStatusResponse> GetStatusAsync();
        Task<ExitResponse> ExitVehicleAsync(ExitRequest request);
    }
}