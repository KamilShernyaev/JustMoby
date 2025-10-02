using System;
using System.Collections.Generic;
using UnityEngine;

namespace Services.SaveLoadService
{
    [Serializable]
    public class TowerSaveData
    {
        public List<SavedTowerElement> Elements = new();

        public SerializableVector3 BasePosition;

        [Serializable]
        public class SavedTowerElement
        {
            public string ElementTypeID;
            public float HorizontalOffset;
            public int Index;
            public float ElementHeight;
        }
    }


    [Serializable]
    public struct SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public Vector3 ToVector3() => new(x, y, z);
    }
}