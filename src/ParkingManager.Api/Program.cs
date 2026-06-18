using ParkingManager.Api.Contracts;
using ParkingManager.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var options = new ParkingManagerOptions();
builder.Services.AddSingleton(options);
builder.Services.AddSingleton(new ParkingLotState(options));
builder.Services.AddSingleton<ParkingService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/inventory", (ParkingService parkingService) => Results.Ok(parkingService.GetInventory()));

app.MapPost("/api/sessions/entry", (EntryRequest request, ParkingService parkingService) =>
{
    try
    {
        return Results.Ok(parkingService.RegisterEntry(request));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = ex.Message });
    }
});

app.MapGet("/api/sessions/{sessionId:guid}", (Guid sessionId, ParkingService parkingService) =>
{
    try
    {
        return Results.Ok(parkingService.GetSession(sessionId));
    }
    catch (Exception ex)
    {
        return Results.NotFound(new { Error = ex.Message });
    }
});

app.MapPost("/api/sessions/{sessionId:guid}/payment", (Guid sessionId, PaymentRequest request, ParkingService parkingService) =>
{
    try
    {
        return Results.Ok(parkingService.RegisterPayment(sessionId, request));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = ex.Message });
    }
});

app.MapPost("/api/sessions/{sessionId:guid}/exit", (Guid sessionId, ParkingService parkingService) =>
{
    try
    {
        var result = parkingService.ValidateExit(sessionId);
        return result.CanExit ? Results.Ok(result) : Results.BadRequest(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = ex.Message });
    }
});

app.MapPost("/api/weather/interval", (WeatherIntervalRequest request, ParkingService parkingService) =>
{
    try
    {
        return Results.Ok(parkingService.AddWeatherInterval(request));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = ex.Message });
    }
});

app.MapGet("/api/reports/monthly", (int year, int month, ParkingService parkingService) =>
{
    try
    {
        return Results.Ok(parkingService.BuildMonthlyReport(year, month));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = ex.Message });
    }
});

app.Run();
