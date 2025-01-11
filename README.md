# ClarityHub - Zarządzanie Projektami i Zadaniami

## Opis Projektu

ClarityHub to aplikacja stworzona w ramach przedmiotu **Integracja aplikacji .NET z bazami danych**. Jej celem jest umożliwienie użytkownikom zarządzania projektami i przypisanymi do nich zadaniami. Aplikacja pozwala na:
- Tworzenie projektów oraz zadań z nimi związanych,
- Aktualizację statusu projektów i zadań,
- Przeglądanie szczegółów zadań i projektów,
- Usuwanie projektów i zadań,
- Wyświetlanie listy zadań przypisanych na dany dzień.

Aplikacja integruje nowoczesne technologie .NET z relacyjną bazą danych PostgreSQL, co umożliwia efektywne zarządzanie danymi oraz ich dynamiczne wyświetlanie.

---

## Funkcjonalności

- **Zarządzanie projektami**:
  - Tworzenie nowych projektów z wieloma zadaniami.
  - Aktualizacja statusu projektów.
  - Usuwanie projektów i powiązanych zadań.

- **Zarządzanie zadaniami**:
  - Tworzenie, edytowanie i usuwanie zadań.
  - Wyświetlanie zadań przypisanych na dzisiejszy dzień.
  - Aktualizacja statusu zadań i synchronizacja statusu projektu.

- **Profile użytkowników**:
  - Logowanie, rejestracja i personalizacja profilu użytkownika.
  - Przechowywanie danych użytkownika w bazie.

---

## Technologie

### Backend
- **.NET Core 6.0**: Serwer backend obsługujący żądania HTTP i współpracujący z bazą danych.
- **Dapper**: Lekka biblioteka ORM do wykonywania zapytań SQL i mapowania wyników.
- **Identity Framework**: Obsługa logowania, rejestracji oraz zarządzania użytkownikami.

### Frontend
- **Razor Pages**: Dynamiczne generowanie widoków opartych na modelu danych.
- **HTML/CSS/JavaScript**: Personalizowane style i dynamiczne formularze.

### Baza Danych
- **PostgreSQL**: Relacyjna baza danych do przechowywania projektów, zadań i profili użytkowników.
- **Funkcje składowane**: Używane do operacji takich jak tworzenie projektów, aktualizacja statusu oraz pobieranie danych.

---

## Struktura Bazy Danych

### Tabele
- **Projects**: Przechowuje informacje o projektach (`Id`, `Name`, `Description`, `DueDate`, `IsCompleted`, `CreatedAt`).
- **Tasks**: Przechowuje zadania przypisane do projektów (`Id`, `Name`, `Description`, `DueDate`, `IsCompleted`, `ProjectId`).
- **UserProfile**: Informacje o użytkownikach (`Id`, `UserId`, `FirstName`, `LastName`).

### Funkcje składowane
- `add_project_with_tasks`: Dodaje projekt wraz z powiązanymi zadaniami.
- `update_project_and_tasks_status`: Aktualizuje status projektu i przypisanych do niego zadań.
- `get_user_tasks_for_today`: Pobiera zadania użytkownika zaplanowane na dzisiejszy dzień.

---

## Konfiguracja

### Wymagania systemowe
- .NET 6.0 SDK
- PostgreSQL 14+
- Node.js (opcjonalnie dla zarządzania bibliotekami frontendu)

### Plik konfiguracyjny `appsettings.json`
Przykład konfiguracji:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=planerdb;Username=postgres;Password=yourpassword"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
