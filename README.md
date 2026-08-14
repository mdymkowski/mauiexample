# MAUI Async Views Demo

Przykładowa aplikacja .NET MAUI pokazująca wzorzec dużego ekranu złożonego z kilku ContentView, gdzie każdy child view ma własny ViewModel.

Projekt jest zbudowany w C# Markup, bez XAML.

## Co pokazuje projekt

- DashboardPage tworzy child viewy przez IServiceProvider.GetRequiredService<T>().
- Każdy child (AppointmentsView, TasksView, CustomersView) ma własny ViewModel.
- Konstruktory View i ViewModeli nie uruchamiają LoadData.
- OnAppearing głównej strony uruchamia inicjalizację dopiero po utworzeniu UI.
- Najważniejszy child jest ładowany pierwszy.
- Pozostałe niezależne childy są ładowane przez Task.WhenAll.
- Opuszczenie strony anuluje operacje przez CancellationToken.
- AsyncViewModelBase blokuje podwójne InitializeAsync.
- CustomersViewModel pokazuje paginację po 50 rekordów przez RemainingItemsThreshold.

## Najważniejsza zasada

Dashboard constructor -> tworzy View + ViewModel -> buduje Grid -> brak I/O

OnAppearing -> render UI -> InitializeAsync najważniejszego childa -> InitializeAsync pozostałych
