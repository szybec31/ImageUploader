using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using PhotoUploader.Models;
using PhotoUploader.Services;

namespace PhotoUploader;

public partial class ServersPage : ContentPage
{
    private readonly ServerStorageService _serverService;
    private readonly MainPage _mainPage;
    private List<SmbServer> _servers = new List<SmbServer>();

    public ServersPage(MainPage mainPage)
    {
        InitializeComponent();
        _serverService = new ServerStorageService();
        _mainPage = mainPage;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadServers();
    }

    private void LoadServers()
    {
        _servers = _serverService.GetServers();
        serversCollectionView.ItemsSource = _servers;
        emptyLabel.IsVisible = _servers.Count == 0;
    }

    private async void OnAddServerClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ServerFormPage(null, this));
    }

    private async void OnServerSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is SmbServer selectedServer)
        {
            var action = await DisplayActionSheet(
                selectedServer.ServerName,
                "Anuluj",
                null,
                "Wybierz",
                "Edytuj",
                "Usuń");

            if (action == "Wybierz")
            {
                _mainPage.SetSelectedServer(selectedServer);
                await Navigation.PopAsync();
            }
            else if (action == "Edytuj")
            {
                await Navigation.PushAsync(new ServerFormPage(selectedServer, this));
            }
            else if (action == "Usuń")
            {
                var confirm = await DisplayAlert(
                    "Potwierdzenie",
                    $"Czy na pewno chcesz usunąć serwer '{selectedServer.ServerName}'?",
                    "Tak", "Nie");

                if (confirm)
                {
                    _serverService.DeleteServer(selectedServer.Id);
                    LoadServers();
                }
            }

            serversCollectionView.SelectedItem = null;
        }
    }

    public void RefreshServers()
    {
        LoadServers();
    }
}