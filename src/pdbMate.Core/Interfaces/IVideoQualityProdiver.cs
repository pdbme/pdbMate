using pdbMate.Core.Data;

namespace pdbMate.Core.Interfaces
{
    public interface IVideoQualityProdiver
    {
        VideoQuality GetById(int id);
        VideoQuality GetByName(string name);
    }
}