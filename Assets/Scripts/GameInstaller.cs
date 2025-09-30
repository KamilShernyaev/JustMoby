using System;
using System.Collections.Generic;
using System.Linq;
using Element;
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
using Zones.DropZones.Tower.TowerElement;
using Zones.ScrollArea;
using Zones.ScrollArea.ScrollElement;

public class GameInstaller : LifetimeScope
{
    [SerializeField] private ScrollElementView scrollElementViewPrefab;
    [SerializeField] private ScrollContainerView scrollContainerView;
    [SerializeField] private DraggingElementView draggingElementViewPrefab;
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private HoleView holeViewPrefab;
    [SerializeField] private TowerContainerView towerContainerView;
    [SerializeField] private TowerElementView towerElementViewPrefab;
    [SerializeField] private Canvas canvas;
    [SerializeField] private NotificationView notificationViewPrefab;

    private ISaveLoadService saveLoadService;
    private TowerContainerModel towerModel;

    protected override void Configure(IContainerBuilder builder)
    {
        // Регистрация префабов и UI
        builder.RegisterInstance(scrollElementViewPrefab).AsSelf();
        builder.RegisterInstance(scrollContainerView).AsSelf();
        builder.RegisterInstance(holeViewPrefab).AsSelf();
        builder.RegisterInstance(towerContainerView).AsSelf().As<TowerContainerView>();

        builder.Register<UnityLocalizationService>(Lifetime.Singleton).As<ILocalizationService>();

        // Регистрация пулов
        builder.Register<ObjectPool<ScrollElementView>>(Lifetime.Singleton)
            .AsSelf()
            .WithParameter("parent", scrollContainerView.transform)
            .WithParameter("prefab", scrollElementViewPrefab);
        builder.Register<ObjectPool<TowerElementView>>(Lifetime.Singleton)
            .AsSelf()
            .WithParameter("parent", towerContainerView.transform)
            .WithParameter("prefab", towerElementViewPrefab);

        // Регистрация правил
        builder.Register<NonRestrictionTowerDropRule>(Lifetime.Singleton).As<IDropRule>().AsSelf();
        //builder.Register<OnlyOneColorTowerDropRule>(Lifetime.Singleton).As<IDropRule>().AsSelf();

        // Модели и контроллеры
        builder.Register<HoleModel>(Lifetime.Singleton);
        builder.Register<HoleController>(Lifetime.Singleton).As<IDropZone>().AsSelf();

        builder.Register<TowerContainerModel>(Lifetime.Singleton);
        builder.Register<TowerContainerController>(Lifetime.Singleton).As<IDropZone>().AsSelf();

        builder.Register<JsonFileSaveLoadService>(Lifetime.Singleton).As<ISaveLoadService>();

        builder.Register<DraggingElementModel>(Lifetime.Singleton);
        builder.Register<DragController>(Lifetime.Singleton);

        builder.RegisterComponentInNewPrefab(draggingElementViewPrefab, Lifetime.Singleton)
            .UnderTransform(canvas.transform)
            .AsImplementedInterfaces()
            .AsSelf();

        builder.RegisterComponentOnNewGameObject<DragHandler>(Lifetime.Scoped)
            .UnderTransform(canvas.transform)
            .AsImplementedInterfaces()
            .AsSelf();

        builder.RegisterComponent(notificationViewPrefab)
            .AsSelf();

        builder.Register<NotificationService>(Lifetime.Singleton).AsSelf();

        builder.RegisterFactory<ElementType, ScrollElementView>(container =>
        {
            var pool = container.Resolve<ObjectPool<ScrollElementView>>();
            var dragHandler = container.Resolve<IDragStartHandler>();

            return elementType =>
            {
                var view = pool.Get();
                var model = new ScrollElementModel { ElementType = elementType };
                var controller = new ScrollElementController(model, view);
                controller.Initialize(Container.Resolve<IDragStartHandler>());
                view.Initialize(controller);
                return view;
            };
        }, Lifetime.Singleton);


        // ScrollContainerModel
        builder.Register<ScrollContainerModel>(Lifetime.Singleton);

        // ScrollContainerController
        builder.RegisterFactory<(ScrollContainerModel, ScrollContainerView, IDragStartHandler,
            ObjectPool<ScrollElementView>, Func<ElementType, ScrollElementView>), ScrollContainerController>(
            container =>
            {
                return tuple => new ScrollContainerController(
                    tuple.Item1,
                    tuple.Item2,
                    tuple.Item3,
                    tuple.Item4,
                    tuple.Item5);
            }, Lifetime.Singleton);
    }

    protected override void Awake()
    {
        base.Awake();
        
        saveLoadService = Container.Resolve<ISaveLoadService>();
        towerModel = Container.Resolve<TowerContainerModel>();
        // if (saveLoadService.HasData())
        // {
        //     var savedData = saveLoadService.LoadData();
        //     towerModel.LoadFromSaveData(savedData, gameConfig.AvailableTypes);
        // }

        // Инициализация ScrollContainerModel
        var scrollModel = Container.Resolve<ScrollContainerModel>();
        scrollModel.ElementsScroll.Clear();
        var typesCount = gameConfig.AvailableTypes.Length;
        var perTypeCount = gameConfig.BottomElementCount / typesCount;
        var remainder = gameConfig.BottomElementCount % typesCount;
        for (var i = 0; i < typesCount; i++)
        {
            var count = perTypeCount + (i < remainder ? 1 : 0);
            var type = gameConfig.AvailableTypes[i];
            for (var j = 0; j < count; j++)
            {
                scrollModel.ElementsScroll.Add(new ScrollElementModel
                    { ElementType = type });
            }
        }

        // Создаём ScrollContainerController через фабрику
        var factory = Container
            .Resolve<Func<(ScrollContainerModel, ScrollContainerView,
                IDragStartHandler,
                ObjectPool<ScrollElementView>,
                Func<ElementType, ScrollElementView>),
                ScrollContainerController>>();

        var scrollController = factory((
            scrollModel,
            scrollContainerView,
            Container.Resolve<IDragStartHandler>(),
            Container.Resolve<ObjectPool<ScrollElementView>>(),
            Container.Resolve<Func<ElementType, ScrollElementView>>()
        ));
        var dragHandler = Container.Resolve<IDragStartHandler>();
        var towerController = Container.Resolve<TowerContainerController>();
        towerController.Initialize(dragHandler);
    }

    // private void SaveTowerState()
    // {
    //     if (towerModel == null || saveLoadService == null)
    //         return;
    //     var data = towerModel.ToSaveData();
    //     saveLoadService.SaveData(data);
    // }

    // private void OnApplicationQuit()
    // {
    //     SaveTowerState();
    // }
    //
    // private void OnApplicationPause(bool pauseStatus)
    // {
    //     if (pauseStatus)
    //     {
    //         SaveTowerState();
    //     }
    // }
}