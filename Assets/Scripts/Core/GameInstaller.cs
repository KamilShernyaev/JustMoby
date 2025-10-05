using System.Collections.Generic;
using Element;
using Services.AnimationService;
using Services.CanvasScalerService;
using Services.ConfigProvider;
using Services.DragService;
using Services.LocalizationService;
using Services.NotificationService;
using Services.PoolService;
using Services.SaveLoadService;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Zones.DropZones.DropRules;
using Zones.DropZones.Hole;
using Zones.DropZones.Tower;
using Zones.ScrollArea;

namespace Core
{
    public class GameInstaller : LifetimeScope
    {
        [Header("Prefabs")] [SerializeField] private ElementView containerElementViewPrefab;
        [SerializeField] private DraggingElementView draggingElementViewPrefab;
        [SerializeField] private HoleView holeViewPrefab;
        [SerializeField] private NotificationView notificationViewPrefab;

        [Header("Scene References")] [SerializeField]
        private ScrollContainerView scrollContainerView;

        [SerializeField] private TowerContainerView towerContainerView;
        [SerializeField] private Canvas canvas;
        [SerializeField] private Transform animationsContainer;

        [Header("Game Config")] [SerializeField]
        private GameConfig gameConfig;

        [SerializeField] private DragHandler dragHandler;

        [SerializeField] private TextAsset jsonConfigFile;

        protected override void Configure(IContainerBuilder builder)
        {
            // Register scene instances
            builder.RegisterInstance(scrollContainerView).AsSelf();
            builder.RegisterInstance(holeViewPrefab).AsSelf();
            builder.RegisterInstance(towerContainerView).AsSelf();
            //builder.RegisterInstance(dragHandler).AsSelf().AsImplementedInterfaces();

            // Localization service
            builder.Register<UnityLocalizationService>(Lifetime.Singleton).As<ILocalizationService>();

            // Object pool for ContainerElementView
            builder.Register<ObjectPool<ElementView>>(Lifetime.Scoped)
                .WithParameter("prefab", containerElementViewPrefab)
                .WithParameter("parent", (Transform)null)
                .Keyed("MainPool")
                .AsSelf();

            builder.Register<ObjectPool<ElementView>>(Lifetime.Scoped)
                .WithParameter("prefab", containerElementViewPrefab)
                .WithParameter("parent", animationsContainer)
                .Keyed("AnimationPool")
                .AsSelf();

            builder.RegisterComponentOnNewGameObject<DragHandler>(Lifetime.Singleton)
                .AsImplementedInterfaces().AsSelf();

            // DraggingElementView prefab registration under Canvas
            builder.RegisterComponentInNewPrefab(draggingElementViewPrefab, Lifetime.Singleton)
                .UnderTransform(canvas?.transform)
                .AsSelf();

            // Drop rules registration
            builder.Register<NonRestrictionTowerDropRule>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            // Models and controllers
            builder.Register<HoleModel>(Lifetime.Singleton);
            builder.Register<HoleController>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.Register<AnimationService>(Lifetime.Singleton).As<IAnimationService>()
                .WithParameter(animationsContainer)
                .WithParameter(canvas);

            builder.Register<TowerContainerModel>(Lifetime.Singleton);
            builder.Register<TowerContainerController>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.Register<JsonFileSaveLoadService>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.Register<DraggingElementModel>(Lifetime.Singleton);
            builder.Register<DragController>(Lifetime.Singleton).WithParameter(canvas);

            // Notification view component registration
            builder.RegisterComponent(notificationViewPrefab)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.Register<NotificationService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

            // Factory for creating ContainerElementView from ElementType
            builder.RegisterFactory<ElementType, ElementView>(container =>
            {
                var pool = container.Resolve<ObjectPool<ElementView>>("MainPool");
                return elementType =>
                {
                    var view = pool.Get();
                    view.SetSprite(elementType.Sprite);
                    return view;
                };
            }, Lifetime.Singleton);

            builder.Register<ScrollContainerModel>(Lifetime.Singleton);
            builder.Register<CanvasScalerService>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .WithParameter(canvas);

            // Direct registration ScrollContainerController
            builder.Register<ScrollContainerController>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
        }
    }
}