using Core;

namespace Services.FactoryService
{
    public interface IFactoryService<TModel, TView> where TModel : class, IModel where TView : View
    {
        TView Create(TModel model);
    }
}