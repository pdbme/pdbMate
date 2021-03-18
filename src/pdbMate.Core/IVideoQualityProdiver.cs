using pdbMate.Core.Data;

namespace pdbMate.Core
{
    public interface IVideoQualityProdiver
    {
        VideoQuality GetById(int id);
        VideoQuality GetByName(string name);
    }
}