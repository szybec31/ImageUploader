using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using PhotoUploader.Models;
using Microsoft.Maui.Storage;

namespace PhotoUploader.Services
{
    public class ServerStorageService
    {
        private const string ServersKey = "saved_servers";
        private List<SmbServer> _servers = new List<SmbServer>();

        public ServerStorageService()
        {
            LoadServers();
        }

        public List<SmbServer> GetServers()
        {
            return _servers.OrderBy(s => s.ServerName).ToList();
        }

        public SmbServer? GetServer(string id)
        {
            return _servers.FirstOrDefault(s => s.Id == id);
        }

        public void SaveServer(SmbServer server)
        {
            var existing = _servers.FirstOrDefault(s => s.Id == server.Id);
            if (existing != null)
            {
                _servers.Remove(existing);
            }
            _servers.Add(server);
            SaveServers();
        }

        public void DeleteServer(string id)
        {
            var server = _servers.FirstOrDefault(s => s.Id == id);
            if (server != null)
            {
                _servers.Remove(server);
                SaveServers();
            }
        }

        private void LoadServers()
        {
            try
            {
                var json = Preferences.Get(ServersKey, string.Empty);
                if (!string.IsNullOrEmpty(json))
                {
                    _servers = JsonSerializer.Deserialize<List<SmbServer>>(json) ?? new List<SmbServer>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading servers: {ex.Message}");
                _servers = new List<SmbServer>();
            }
        }

        private void SaveServers()
        {
            try
            {
                var json = JsonSerializer.Serialize(_servers);
                Preferences.Set(ServersKey, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving servers: {ex.Message}");
            }
        }
    }
}