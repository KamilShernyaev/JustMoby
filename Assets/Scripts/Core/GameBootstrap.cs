using System;
using Element;
using Services.CanvasScalerService;
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
        private ICanvasScalerService canvasScalerService;

        [Inject]
        public void Construct(IObjectResolver container, ISaveLoadService saveLoadService,
            ICanvasScalerService canvasScalerService)
        {
            this.container = container;
            this.saveLoadService = saveLoadService;
            this.canvasScalerService = canvasScalerService;
        }

        private void Start()
        {
            canvasScalerService.AdjustUI();

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
                container.Resolve<ScrollContainerModel>();
                container.Resolve<ScrollContainerView>();
                var dragHandler = container.Resolve<IDragStartHandler>();
                container.Resolve<Func<ElementType, ElementView>>();
                container.Resolve<IConfigProvider>();
                container.Resolve<ScrollContainerController>();
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
                container.Resolve<ScrollContainerModel>();

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