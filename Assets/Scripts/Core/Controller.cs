using System;
using UnityEngine;

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
            OnBeforeModelChanged();
            Model = model;
            OnModelChanged();
        }

        public void SetView(TView view)
        {
            OnBeforeViewChanged();
            View = view;
            OnViewChanged();
        }

        public void SetModel(object model)
        {
            if (model == null)
            {
                SetModel(default(TModel));
                return;
            }

            if (model is not TModel tModel)
            {
                throw new ArgumentException("Wrong model type");
            }

            SetModel(tModel);
        }

        public void SetView(object view)
        {
            if (view == null)
            {
                SetView(default(TView));
                return;
            }

            if (view is not TView tView)
            {
                throw new ArgumentException("Wrong view type");
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