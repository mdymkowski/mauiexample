namespace MauiAsyncViewsDemo.Models;

public sealed record TaskModel(Guid Id, string Subject, DateTime DueDate, bool IsImportant);
