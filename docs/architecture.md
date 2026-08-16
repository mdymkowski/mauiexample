# Architektura ładowania

```text
DashboardPage
   |
   +-- GetRequiredService<AppointmentsView>()
   |       +-- AppointmentsViewModel
   |
   +-- GetRequiredService<TasksView>()
   |       +-- TasksViewModel
   |
   +-- GetRequiredService<CustomersView>()
           +-- CustomersViewModel

Konstruktor:
   tylko budowa drzewa UI
          |
          v
OnAppearing
          |
          +--> Task.Yield()
          |
          +--> Appointments.InitializeAsync()
          |
          +--> Task.Yield()
          |
          +--> WhenAll(
                 Tasks.InitializeAsync(),
                 Customers.InitializeAsync()
               )

OnDisappearing
          |
          +--> CancellationTokenSource.Cancel()
          +--> CancelLoading() każdego child VM
```

## Dlaczego to jest płynniejsze

`await` nie gwarantuje sam z siebie płynności, ale pozwala nie blokować UI przy prawdziwie
asynchronicznym I/O. Dodatkowo nie uruchamiamy wszystkich operacji podczas konstrukcji strony
i nie wrzucamy tysięcy elementów do kolekcji naraz.

## Gdy child viewów jest 8-10

Nie uruchamiaj bezmyślnie 10 operacji przez `Task.WhenAll`.
Podziel je na priorytety albo zastosuj `SemaphoreSlim`, np. maksymalnie 2-3 ładowania naraz.

## Gdy child jest w zakładce

Najlepiej nie inicjalizować go przy starcie Dashboardu.
Uruchom `InitializeAsync()` dopiero po wybraniu zakładki.
