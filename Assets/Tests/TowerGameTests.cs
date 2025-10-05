using System;
using System.Collections;
using Core;
using Element;
using NUnit.Framework;
using Services.PoolService;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Zones.DropZones.DropRules;
using Zones.DropZones.Tower;
using Zones.DropZones.Tower.TowerElement;
using Zones.ScrollArea;
using Assert = UnityEngine.Assertions.Assert;
using Object = UnityEngine.Object;

namespace Tests
{
    public class TowerGameTests
    {
        private GameConfig CreateTestGameConfig()
        {
            var config = ScriptableObject.CreateInstance<GameConfig>();
            config.SetField("availableTypes", new[]
            {
                new ElementType
                {
                    ID = "Red",
                    Sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f))
                },
                new ElementType
                {
                    ID = "Blue",
                    Sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f))
                }
            });
            config.SetField("bottomCubeCount", 4);
            return config;
        }

        [Test]
        public void TowerContainerModel_AddElement_IncreasesElementCount()
        {
            var model = new TowerContainerModel();
            var element = new TowerElementModel { ElementType = new ElementType { ID = "Red" }, ElementHeight = 1f };

            model.AddElement(element);

            Assert.AreEqual(1, model.ElementCount, "Element count should be 1 after adding an element.");
        }

        [Test]
        public void TowerContainerModel_RemoveElement_UpdatesIndices()
        {
            var model = new TowerContainerModel();
            model.AddElement(new TowerElementModel
                { ElementType = new ElementType { ID = "Red" }, Index = 0, ElementHeight = 1f });
            model.AddElement(new TowerElementModel
                { ElementType = new ElementType { ID = "Blue" }, Index = 1, ElementHeight = 1f });

            model.RemoveElementAt(0);

            Assert.AreEqual(1, model.ElementCount, "Element count should be 1 after removal.");
            Assert.AreEqual(0, model.GetElementAt(0).Index, "Remaining element index should be updated to 0.");
        }

        [Test]
        public void TowerContainerModel_CanAddElement_RespectsHeightLimit()
        {
            var model = new TowerContainerModel();
            model.AddElement(new TowerElementModel
                { ElementType = new ElementType { ID = "Red" }, ElementHeight = 2f });

            var canAdd = model.CanAddElement(1f, 3f);
            var cannotAdd = model.CanAddElement(2f, 3f);

            Assert.IsTrue(canAdd, "Should be able to add element within height limit.");
            Assert.IsFalse(cannotAdd, "Should not be able to add element exceeding height limit.");
        }

        [Test]
        public void ScrollContainerModel_InitializeElements_DistributesCorrectly()
        {
            var config = CreateTestGameConfig();
            var model = new ScrollContainerModel();

            model.InitializeElements(config);

            Assert.AreEqual(4, model.ElementsScroll.Count, "Should initialize 4 elements (2 per type).");
            Assert.AreEqual(2, model.ElementsScroll.FindAll(e => e.ElementType.ID == "Red").Count,
                "Should have 2 Red elements.");
            Assert.AreEqual(2, model.ElementsScroll.FindAll(e => e.ElementType.ID == "Blue").Count,
                "Should have 2 Blue elements.");
        }

        [Test]
        public void ScrollContainerModel_InitializeElements_HandlesEmptyConfig()
        {
            var config = ScriptableObject.CreateInstance<GameConfig>();
            config.SetField("availableTypes", Array.Empty<ElementType>());
            config.SetField("bottomCubeCount", 0);
            var model = new ScrollContainerModel();

            model.InitializeElements(config);

            Assert.AreEqual(0, model.ElementsScroll.Count, "Should initialize empty list for empty config.");
        }

        [Test]
        public void ObjectPool_GetAndReturn()
        {
            var prefab = new GameObject("TestPrefab").AddComponent<Image>();
            var pool = new ObjectPool<Image>(prefab, initialSize: 2);
            var obj1 = pool.Get();
            Assert.IsTrue(obj1.gameObject.activeSelf);
            pool.ReturnToPool(obj1);
            Assert.IsFalse(obj1.gameObject.activeSelf);
            Assert.AreEqual(1, pool.AvailableCount);
        }

        [Test]
        public void NonRestrictionTowerDropRule_AllowsAnyElement()
        {
            var rule = new NonRestrictionTowerDropRule();
            var element = new TowerElementModel { ElementType = new ElementType { ID = "Red" } };
            var tower = new TowerContainerModel();

            var canAdd = rule.CanAddElement(element, tower);

            Assert.IsTrue(canAdd, "NonRestriction rule should allow any element.");
        }

        [Test]
        public void OnlyOneColorTowerDropRule_RestrictsDifferentColor()
        {
            var rule = new OnlyOneColorTowerDropRule();
            var tower = new TowerContainerModel();
            tower.AddElement(new TowerElementModel { ElementType = new ElementType { ID = "Red" } });
            var sameColorElement = new TowerElementModel { ElementType = new ElementType { ID = "Red" } };
            var differentColorElement = new TowerElementModel { ElementType = new ElementType { ID = "Blue" } };

            var canAddSame = rule.CanAddElement(sameColorElement, tower);
            var canAddDifferent = rule.CanAddElement(differentColorElement, tower);

            Assert.IsTrue(canAddSame, "Should allow element of the same color.");
            Assert.IsFalse(canAddDifferent, "Should not allow element of a different color.");
        }
    }

    public static class TestExtensions
    {
        public static void SetField<T>(this ScriptableObject obj, string fieldName, T value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }
    }
}