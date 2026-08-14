using CommunityToolkit.Mvvm.ComponentModel;
using MauiAsyncViewsDemo.Models;
using MauiAsyncViewsDemo.Repositories;
using System.Collections.ObjectModel;

namespace MauiAsyncViewsDemo.ViewModels;

public partial class AppointmentsViewModel : AsyncViewModelBase
{
    private readonly IAppointmentRepository _repository;

    [ObservableProperty]
    private ObservableCollection<AppointmentModel> appointments = [];

    public AppointmentsViewModel(IAppointmentRepository repository)
    {
        _repository = repository;
    }

    protected override async Task LoadAsync(CancellationToken cancellationToken)
    {
        var data = await _repository.GetTodayAsync(cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        // Jedna podmiana kolekcji zamiast wielu Add podczas pierwszego loadu.
        Appointments = new ObservableCollection<AppointmentModel>(data);
    }
}
