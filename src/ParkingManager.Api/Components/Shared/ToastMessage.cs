namespace ParkingManager.Api.Components.Shared;

public enum ToastKind
{
    Success,
    Info,
    Warning,
    Error
}

public sealed record ToastMessage(Guid Id, ToastKind Kind, string Title, string Message);