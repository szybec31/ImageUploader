using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using PhotoUploader.Models;
using PhotoUploader.Services;
using SMBLibrary;
using SMBLibrary.Client;
using System;
using System.IO;
using System.Threading.Tasks;
// Alias, żeby uniknąć konfliktu z System.IO.FileAttributes
using SMBFileAttributes = SMBLibrary.FileAttributes;

namespace PhotoUploader;

public partial class MainPage : ContentPage
{
    private string? _pickedFilePath;
    private SmbServer? _selectedServer;
    private readonly ServerStorageService _serverService;

    public MainPage()
    {
        InitializeComponent();
        _serverService = new ServerStorageService();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateServerStatus();
    }

    private void UpdateServerStatus()
    {
        // Sprawdź czy mamy zapisane serwery i wybierz pierwszy jako domyślny
        var servers = _serverService.GetServers();
        if (servers.Any() && _selectedServer == null)
        {
            _selectedServer = servers.First();
        }

        if (_selectedServer != null)
        {
            ServerStatusLabel.Text = $"Serwer: {_selectedServer.ServerName}";
            ServerStatusLabel.TextColor = Colors.Green;
        }
        else
        {
            ServerStatusLabel.Text = "Nie wybrano serwera";
            ServerStatusLabel.TextColor = Colors.Red;
        }

        SendButton.IsEnabled = _selectedServer != null && !string.IsNullOrEmpty(_pickedFilePath);
    }

    private async void OnPickPhotoClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Wybierz zdjęcie",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                _pickedFilePath = result.FullPath;
                PreviewImage.Source = ImageSource.FromFile(_pickedFilePath);
                StatusLabel.Text = $"Wybrano: {result.FileName}";
                UpdateServerStatus();
            }
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"❌ Błąd wyboru: {ex.Message}";
        }
    }

    private async void OnManageServersClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ServersPage(this));
    }

    public void SetSelectedServer(SmbServer server)
    {
        _selectedServer = server;
        UpdateServerStatus();
    }

    private async void OnSendPhotoClicked(object sender, EventArgs e)
    {
        try
        {
            Console.WriteLine("DEBUG: Rozpoczynam wysyłanie...");

            if (string.IsNullOrEmpty(_pickedFilePath))
            {
                throw new Exception("Nie wybrano pliku");
            }

            if (_selectedServer == null)
            {
                throw new Exception("_selectedServer jest null");
            }

            // Dodaj szczegółowe sprawdzenie każdej właściwości
            Console.WriteLine($"DEBUG: _selectedServer = {_selectedServer}");
            Console.WriteLine($"DEBUG: ServerIp = {_selectedServer.ServerIp}");
            Console.WriteLine($"DEBUG: ShareName = {_selectedServer.ShareName}");
            Console.WriteLine($"DEBUG: SmbUser = {_selectedServer.SmbUser}");
            Console.WriteLine($"DEBUG: SmbPass = {_selectedServer.SmbPass}");
            Console.WriteLine($"DEBUG: SmbDomain = {_selectedServer.SmbDomain}");

            if (string.IsNullOrEmpty(_selectedServer.ServerIp))
                throw new Exception("ServerIp jest null lub pusty");
            if (string.IsNullOrEmpty(_selectedServer.ShareName))
                throw new Exception("ShareName jest null lub pusty");
            if (string.IsNullOrEmpty(_selectedServer.SmbUser))
                throw new Exception("SmbUser jest null lub pusty");
            if (string.IsNullOrEmpty(_selectedServer.SmbPass))
                throw new Exception("SmbPass jest null lub pusty");

            StatusLabel.Text = "⏳ Łączenie z serwerem SMB...";
            SendButton.IsEnabled = false;

            await Task.Run(() => SendFileToServer(_selectedServer, _pickedFilePath));
            StatusLabel.Text = $"✅ Wysłano: {Path.GetFileName(_pickedFilePath)}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DEBUG: Błąd: {ex.Message}");
            Console.WriteLine($"DEBUG: StackTrace: {ex.StackTrace}");
            StatusLabel.Text = $"❌ Błąd: {ex.Message}";
        }
        finally
        {
            SendButton.IsEnabled = true;
        }
    }

    private void SendFileToServer(SmbServer server, string filePath)
    {
        Console.WriteLine("DEBUG: SendFileToServer - start");

        if (server == null)
        {
            Console.WriteLine("DEBUG: server jest null");
            throw new ArgumentNullException(nameof(server));
        }
        if (string.IsNullOrEmpty(filePath))
        {
            Console.WriteLine("DEBUG: filePath jest null lub pusty");
            throw new ArgumentNullException(nameof(filePath));
        }
        if (!File.Exists(filePath))
        {
            Console.WriteLine("DEBUG: plik nie istnieje");
            throw new FileNotFoundException("Plik nie istnieje", filePath);
        }

        SMB2Client? client = null;
        ISMBFileStore? fileStore = null;
        object? handle = null;
        bool isLoggedIn = false;

        try
        {
            Console.WriteLine($"DEBUG: Łączenie z {server.ServerIp}...");
            client = new SMB2Client();

            if (!client.Connect(server.ServerIp, SMBTransportType.DirectTCPTransport))
                throw new Exception("Nie udało się połączyć z serwerem SMB");

            NTStatus status = client.Login(
                domainName: server.SmbDomain ?? string.Empty,
                userName: server.SmbUser,
                password: server.SmbPass,
                authenticationMethod: AuthenticationMethod.NTLMv2 // Explicitnie ustaw metodę
            );

            if (status != NTStatus.STATUS_SUCCESS)
                throw new Exception($"Logowanie nieudane: {status}");

            isLoggedIn = true;

            fileStore = client.TreeConnect(server.ShareName, out status);
            if (status != NTStatus.STATUS_SUCCESS)
                throw new Exception($"TreeConnect nieudane: {status}");

            string extension = Path.GetExtension(filePath);
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"{timestamp}{extension}";
            string remotePath = fileName;

            using var localStream = File.OpenRead(filePath);

            status = fileStore.CreateFile(
                out handle,
                out FileStatus fileStatusOut,
                remotePath,
                AccessMask.GENERIC_WRITE | AccessMask.SYNCHRONIZE,
                SMBFileAttributes.Normal,
                ShareAccess.None,
                CreateDisposition.FILE_OVERWRITE_IF,
                CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT,
                null);

            if (status != NTStatus.STATUS_SUCCESS)
                throw new Exception($"CreateFile nieudane: {status}");

            byte[] buffer = new byte[64 * 1024];
            int read;
            long offset = 0;

            while ((read = localStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                byte[] chunk = new byte[read];
                Array.Copy(buffer, chunk, read);

                status = fileStore.WriteFile(out int written, handle, offset, chunk);
                if (status != NTStatus.STATUS_SUCCESS)
                    throw new Exception($"WriteFile nieudane: {status}");

                offset += written;
            }

            fileStore.FlushFileBuffers(handle);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DEBUG: Błąd w SendFileToServer: {ex.Message}");
            throw;
        }
        finally
        {
            if (handle != null)
                fileStore?.CloseFile(handle);

            if (isLoggedIn)
                client?.Logoff();

            client?.Disconnect();
        }
    }
}