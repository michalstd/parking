using Microsoft.EntityFrameworkCore;
using Parking.Api.Data;
using Parking.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<ParkingContext>(opt =>
    opt.UseInMemoryDatabase("ParkingDb"));
builder.Services.AddScoped<IParkingService, ParkingService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ParkingContext>();
    DbSeeder.Seed(ctx, 10);
}

if (app.Environment.IsDevelopment())
{
    //TODO Swagger
}

app.MapControllers();
app.Run();