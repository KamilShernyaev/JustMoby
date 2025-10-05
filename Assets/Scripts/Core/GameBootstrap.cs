using System;
using Element;
using Services.ConfigProvider;
using Services.DragService;
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

        private void OnApplicationQuit() => SaveGameState();

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
                var scrollView = container.Resolve<ScrollContainerView>();
                var dragHandler = container.Resolve<IDragStartHandler>();
                var elementFactory = container.Resolve<Func<ElementType, ElementView>>();
                var config = container.Resolve<IConfigProvider>();
                var scrollController = container.Resolve<ScrollContainerController>();
                var towerController = container.Resolve<TowerContainerController>();
                towerController.Initialize(dragHandler);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize controllers: {e.Message}\n{e.StackTrace}");
            }
        }

        private void LoadGameState()
        {
            try
            {
                var config = container.Resolve<IConfigProvider>();
                var scrollModel = container.Resolve<ScrollContainerModel>();

                var towerData = saveLoadService.LoadData();
                if (towerData == null)
                {
                    return;
                }

                var towerModel = container.Resolve<TowerContainerModel>();
                towerModel.LoadFromSaveData(towerData, config.AvailableTypes);

                var towerController = container.Resolve<TowerContainerController>();
                towerController.LoadFromSavedData();
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
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save tower state: {e.Message}\n{e.StackTrace}");
            }
        }
    }
}