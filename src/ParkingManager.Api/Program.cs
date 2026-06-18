using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components;
using ParkingManager.Api.Contracts;
using ParkingManager.Api.Components;
using ParkingManager.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var options = new ParkingManagerOptions();
builder.Services.AddSingleton(options);

var connectionString = builder.Configuration.GetConnectionString("ParkingManagerDb")
    ?? throw new InvalidOperationException("Missing connection string 'ParkingManagerDb'.");

builder.Services.AddDbContext<ParkingDbContext>(db => db.UseNpgsql(connectionString));
builder.Services.AddScoped<ParkingService>();
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(sp.GetRequiredService<NavigationManager>().BaseUri)
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ParkingDbContext>();
    var seededOptions = scope.ServiceProvider.GetRequiredService<ParkingManagerOptions>();

    db.Database.Migrate();
    ParkingSeeder.EnsureSeeded(db, seededOptions);
    DemoDataSeeder.EnsureSeeded(db, seededOptions);
}

app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.MapGet("/api/inventory", (ParkingService parkingService) => Results.Ok(parkingService.GetInventory()));

app.MapGet("/api/sessions", (int? take, ParkingService parkingService) =>
{
    try
    {
        return Results.Ok(parkingService.GetRecentSessions(take ?? 12));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = ex.Message });
    }
});

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
