using Microsoft.Maui.Controls;
using System;
using PhotoUploader.Models;
using PhotoUploader.Services;

namespace PhotoUploader;

public partial class ServerFormPage : ContentPage
{
    private readonly ServerStorageService _serverService;
    private readonly ServersPage _serversPage;
    private SmbServer? _editingServer;

    public ServerFormPage(SmbServer? server, ServersPage serversPage)
    {
        InitializeComponent();
        _serverService = new ServerStorageService();
        _serversPage = serversPage;
        _editingServer = server; // Teraz może być null

        if (server != null)
        {
            Title = "Edytuj serwer";
            LoadServerData();
        }
        else
        {
            Title = "Dodaj serwer";
        }
    }

    private void LoadServerData()
    {
        if (_editingServer == null)
        {
            Console.WriteLine("DEBUG: _editingServer jest null - pomijam ładowanie danych");
            return;
        }
        serverNameEntry.Text = _editingServer.ServerName;
        serverIpEntry.Text = _editingServer.ServerIp;
        shareNameEntry.Text = _editingServer.ShareName;
        smbUserEntry.Text = _editingServer.SmbUser;
        smbPassEntry.Text = _editingServer.SmbPass;
        smbDomainEntry.Text = _editingServer.SmbDomain;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(serverNameEntry.Text))
        {
            await DisplayAlert("Błąd", "Nazwa serwera jest wymagana", "OK");
            return;
        }

        var server = _editingServer ?? new SmbServer();

        server.ServerName = serverNameEntry.Text;
        server.ServerIp = serverIpEntry.Text;
        server.ShareName = shareNameEntry.Text;
        server.SmbUser = smbUserEntry.Text;
        server.SmbPass = smbPassEntry.Text;
        server.SmbDomain = smbDomainEntry.Text;

        _serverService.SaveServer(server);
        _serversPage.RefreshServers();

        await DisplayAlert("Sukces", _editingServer == null ?
            "Serwer został dodany" : "Serwer został zaktualizowany", "OK");

        await Navigation.PopAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}