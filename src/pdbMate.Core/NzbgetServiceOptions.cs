namespace pdbMate.Core
{
    public class NzbgetServiceOptions
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool UseHttps { get; set; }
        public bool IsActive { get; set; }
    }
}
