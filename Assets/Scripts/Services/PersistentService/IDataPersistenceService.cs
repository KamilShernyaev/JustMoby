namespace Services.PersistentService
{
    public interface IDataPersistenceService<TData>
    {
        void SaveData(TData data);
        TData LoadData();
        bool HasData();
    }
}