using CommunityToolkit.Mvvm.ComponentModel;
using MauiAsyncViewsDemo.Models;
using MauiAsyncViewsDemo.Repositories;
using System.Collections.ObjectModel;

namespace MauiAsyncViewsDemo.ViewModels;

public partial class TasksViewModel : AsyncViewModelBase
{
    private readonly ITaskRepository _repository;

    [ObservableProperty]
    private ObservableCollection<TaskModel> tasks = [];

    public TasksViewModel(ITaskRepository repository)
    {
        _repository = repository;
    }

    protected override async Task LoadAsync(CancellationToken cancellationToken)
    {
        var data = await _repository.GetDashboardTasksAsync(cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        Tasks = new ObservableCollection<TaskModel>(data);
    }
}
