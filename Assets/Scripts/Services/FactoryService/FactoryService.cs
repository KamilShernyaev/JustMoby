using Core;
using UnityEngine;
using VContainer;

namespace Services.FactoryService
{
    public class FactoryService<TModel, TView, TController> : IFactoryService<TModel, TView>
        where TController : IController, new()
        where TModel : class, IModel
        where TView : View
    {
        private readonly TView prefab;

        [Inject]
        public FactoryService(TView prefab)
        {
            this.prefab = prefab;
        }

        public TView Create(TModel model)
        {
            var view = Object.Instantiate(prefab);
            var controller = new TController();
            controller.SetModel(model);
            controller.SetView(view);
            return view;
        }
    }
}