namespace MauiAsyncViewsDemo.Models;

public sealed record AppointmentModel(Guid Id, string Subject, DateTime Start, string CustomerName);
