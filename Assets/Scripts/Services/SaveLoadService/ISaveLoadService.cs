using Services.PersistentService;

namespace Services.SaveLoadService
{
    public interface ISaveLoadService : IDataPersistenceService<TowerSaveData>
    {
    }
}