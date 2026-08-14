# MAUI Async Views Demo

Przykładowa aplikacja `.NET MAUI` pokazująca wzorzec dla dużego ekranu złożonego
z kilku `ContentView`, gdzie każdy child view ma własny ViewModel.

Projekt jest celowo zbudowany w **C# Markup**, bez XAML.

## Co pokazuje projekt

- `DashboardPage` tworzy child viewy przez `IServiceProvider.GetRequiredService<T>()`.
- Każdy child (`AppointmentsView`, `TasksView`, `CustomersView`) ma własny ViewModel.
- **Konstruktory View i ViewModeli nie uruchamiają żadnego LoadData.**
- `OnAppearing` głównej strony uruchamia inicjalizację dopiero po utworzeniu UI.
- Najważniejszy child jest ładowany pierwszy.
- Pozostałe niezależne childy są ładowane później przez `Task.WhenAll`.
- Opuszczenie strony anuluje operacje przez `CancellationToken`.
- `AsyncViewModelBase` blokuje podwójne `InitializeAsync`.
- `CustomersViewModel` pokazuje paginację po 50 rekordów przez
  `RemainingItemsThreshold`.
- Pierwszy load podmienia całą `ObservableCollection`, zamiast wykonywać tysiące `Add`.
- Fake repositories symulują wolne SQLite/API.

## Najważniejsza zasada

Nie:

```text
Dashboard constructor
  -> GetRequiredService<ChildView>()
      -> ChildView constructor
          -> LoadData()
```

Tylko:

```text
Dashboard constructor
  -> tworzy View + ViewModel
  -> buduje Grid
  -> brak I/O

OnAppearing
  -> render UI
  -> InitializeAsync najważniejszego childa
  -> InitializeAsync pozostałych
```

## Struktura

```text
MauiAsyncViewsDemo
├── Contracts
│   ├── IAsyncChildView.cs
│   └── IAsyncInitializable.cs
├── Models
├── Repositories
│   ├── I...Repository.cs
│   └── Fake...Repository.cs
├── ViewModels
│   ├── AsyncViewModelBase.cs
│   ├── AppointmentsViewModel.cs
│   ├── TasksViewModel.cs
│   └── CustomersViewModel.cs
├── Views
│   ├── DashboardPage.cs
│   ├── AppointmentsView.cs
│   ├── TasksView.cs
│   └── CustomersView.cs
├── App.cs
├── MauiProgram.cs
└── MauiAsyncViewsDemo.csproj
```

## Pakiety

Projekt używa:

- `CommunityToolkit.Maui.Markup`
- `CommunityToolkit.Mvvm`

## Jak przenieść wzorzec do prawdziwej aplikacji

W `Fake...Repository` zamień `Task.Delay` na właściwe operacje async:

```csharp
await httpClient.GetFromJsonAsync(..., cancellationToken);
```

lub:

```csharp
await dbContext.Entities
    .AsNoTracking()
    .ToListAsync(cancellationToken);
```

Dla SQLite użyj API, które faktycznie wykonuje zapytania asynchronicznie.

Nie otaczaj I/O bez potrzeby w:

```csharp
Task.Run(...)
```

`Task.Run` zostaw dla kosztownej pracy CPU, nie dla zwykłego `await` HTTP/DB.

## Co można rozbudować

Kolejne dobre kroki:

1. osobny `NavigationService`,
2. `BasePage`,
3. cache danych i polityka odświeżania,
4. `RefreshAsync`,
5. limit równoległości przy 8-10 child ViewModelach,
6. skeleton loading,
7. realny SQLite repository,
8. wersja z Telerik `RadCollectionView`,
9. testy ViewModeli.
