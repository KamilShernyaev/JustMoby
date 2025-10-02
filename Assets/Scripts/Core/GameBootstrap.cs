using System;
using Element;
using Services.ConfigProvider;
using Services.DragService;
using Services.PoolService;
using Services.SaveLoadService;
using UnityEngine;
using VContainer;
using Zones.DropZones.Tower;
using Zones.ScrollArea;

namespace Core
{
    public class GameBootstrap : MonoBehaviour
    {
        private IObjectResolver container;
        private ISaveLoadService saveLoadService;

        [Inject]
        public void Construct(IObjectResolver container, ISaveLoadService saveLoadService)
        {
            this.container = container;
            this.saveLoadService = saveLoadService;
        }

        private void Start()
        {
            InitializeControllers();
            LoadGameState();
        }

        private void OnApplicationQuit()
        {
            SaveGameState();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveGameState();
            }
        }

        private void InitializeControllers()
        {
            try
            {
                var scrollModel = container.Resolve<ScrollContainerModel>();

                var scrollFactory = container.Resolve<Func<(ScrollContainerModel, ScrollContainerView, IDragStartHandler,
                    ObjectPool<ElementView>,
                    Func<ElementType, ElementView>, IConfigProvider),
                    ScrollContainerController>>();

                var scrollController = scrollFactory((
                    scrollModel,
                    container.Resolve<ScrollContainerView>(),
                    container.Resolve<IDragStartHandler>(),
                    container.Resolve<ObjectPool<ElementView>>(),
                    container.Resolve<Func<ElementType, ElementView>>(),
                    container.Resolve<IConfigProvider>()
                ));

                var dragHandler = container.Resolve<IDragStartHandler>();
                var towerController = container.Resolve<TowerContainerController>();
                towerController.Initialize(dragHandler);

                Debug.Log("Game controllers initialized successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize controllers: {e.Message}\n{e.StackTrace}");
            }
        }

        private void LoadGameState()
        {
            var config = container.Resolve<IConfigProvider>();
            var scrollModel = container.Resolve<ScrollContainerModel>();
            scrollModel.InitializeElements(config);

            var towerData = saveLoadService.LoadData();
            if (towerData == null)
            {
                Debug.Log("No saved tower data found. Starting fresh.");
                return;
            }

            try
            {
                var towerModel = container.Resolve<TowerContainerModel>();
                towerModel.LoadFromSaveData(towerData, config.AvailableTypes);

                var towerController = container.Resolve<TowerContainerController>();
                towerController.LoadFromSavedData();

                Debug.Log("Tower state loaded successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load tower state: {e.Message}\n{e.StackTrace}");
            }
        }

        private void SaveGameState()
        {
            try
            {
                var towerModel = container.Resolve<TowerContainerModel>();
                saveLoadService.SaveData(towerModel.ToSaveData());
                Debug.Log("Tower state saved successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save tower state: {e.Message}\n{e.StackTrace}");
            }
        }
    }
}