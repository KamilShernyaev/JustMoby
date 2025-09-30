using System;
using System.IO;
using UnityEngine;

namespace Services.SaveLoadService
{
    public class JsonFileSaveLoadService : ISaveLoadService
    {
        private readonly string saveFilePath = Path.Combine(Application.persistentDataPath, "tower_save.json");

        public void SaveData(TowerSaveData data)
        {
            try
            {
                var json = JsonUtility.ToJson(data, prettyPrint: true);
                File.WriteAllText(saveFilePath, json);
                Debug.Log($"Saved tower state to {saveFilePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save tower state: {e}");
            }
        }

        public TowerSaveData LoadData()
        {
            if (!HasData())
                return null;

            try
            {
                var json = File.ReadAllText(saveFilePath);
                return JsonUtility.FromJson<TowerSaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load tower state: {e}");
                return null;
            }
        }

        public bool HasData()
        {
            return File.Exists(saveFilePath);
        }
    }
}