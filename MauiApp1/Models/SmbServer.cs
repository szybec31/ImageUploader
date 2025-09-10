using System;

namespace PhotoUploader.Models
{
    public class SmbServer
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ServerName { get; set; }
        public string ServerIp { get; set; }
        public string ShareName { get; set; }
        public string SmbUser { get; set; }
        public string SmbPass { get; set; }
        public string SmbDomain { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return $"{ServerName} ({ServerIp})";
        }
    }
}