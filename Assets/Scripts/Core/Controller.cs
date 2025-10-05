using System;
using Core.MVC;

namespace Core
{
    public abstract class Controller<TModel, TView> : IController
        where TModel : class, IModel
        where TView : View
    {
        public TModel Model { get; private set; }
        public TView View { get; private set; }

        public Type GetModelType() => typeof(TModel);
        public Type GetViewType() => typeof(TView);

        public Controller(TModel model, TView view)
        {
            SetModel(model);
            SetView(view);
        }

        public void SetModel(TModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "Model is NULL in SetModel!");
            }
            OnBeforeModelChanged();
            Model = model;
            OnModelChanged();
        }

        public void SetModel(object model)
        {
            if (model == null)
            {
                SetModel(null);
                return;
            }

            if (model is not TModel tModel)
            {
                var error = $"Wrong model type: expected {typeof(TModel).Name}, got {model.GetType().Name}";
                throw new ArgumentException(error);
            }

            SetModel(tModel);
        }

        public void SetView(TView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view), "View is NULL in SetView!");
            }
            OnBeforeViewChanged();
            View = view;
            OnViewChanged();
        }

        public void SetView(object view)
        {
            if (view == null)
            {
                SetView(null);
                return;
            }

            if (view is not TView tView)
            {
                var error = $"Wrong view type: expected {typeof(TView).Name}, got {view.GetType().Name}";
                throw new ArgumentException(error);
            }

            SetView(tView);
        }

        protected virtual void OnBeforeModelChanged()
        {
        }

        protected virtual void OnModelChanged()
        {
        }

        protected virtual void OnBeforeViewChanged()
        {
        }

        protected virtual void OnViewChanged()
        {
        }
    }
}
