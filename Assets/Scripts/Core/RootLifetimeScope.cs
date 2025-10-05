using Services.CanvasScalerService;
using Services.ConfigProvider;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Core
{
    public class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private CanvasScalerConfig canvasScalerConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(gameConfig).As<IConfigProvider>();
            builder.RegisterInstance(canvasScalerConfig);
        }
    }
}