namespace pdbMate.Core.Data.Nzbget
{
    public class NzbgetResult<T>
    {
        public string Version { get; set; }
        public T Result { get; set; }
    }
}
