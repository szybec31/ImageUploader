# .NET MAUI Photo Uploader

Aplikacja do wysyłania zdjęć na serwer SMB napisana w .NET MAUI dla Android.

## Funkcje

- 📸 Wybór zdjęć z galerii
- 📤 Wysyłanie na serwer SMB
- 💾 Zapisywanie wielu serwerów
- 🎮 Intuicyjny interfejs

## Wymagania

- .NET 8.0
- Visual Studio 2022
- Android SDK

## Instalacja

```bash
git clone https://github.com/twoja_nazwa/ImageUploader.git
cd ImageUploader
dotnet restore
```
## Struktura Projektu
ImageUploader/
├── Models/
│   └── SmbServer.cs
├── Services/
│   └── ServerStorageService.cs
├── Views/
│   ├── MainPage.xaml
│   ├── MainPage.xaml.cs
│   ├── ServersPage.xaml
│   └── ServersPage.xaml.cs
└── MauiProgram.cs
