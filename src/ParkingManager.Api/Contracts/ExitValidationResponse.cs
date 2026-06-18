namespace ParkingManager.Api.Contracts;

public sealed class ExitValidationResponse
{
    public required bool CanExit { get; init; }

    public required string Message { get; init; }

    public decimal AdditionalAmountRequired { get; init; }
}
