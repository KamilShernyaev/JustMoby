using UnityEngine;
using UnityEditor;
using Element;

[CustomEditor(typeof(GameConfig))]
public class GameConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameConfig config = (GameConfig)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Конвертировать выделенные Texture2D в ElementType"))
        {
            ConvertSelectedTexturesToElementTypes(config);
        }
    }

    private void ConvertSelectedTexturesToElementTypes(GameConfig config)
    {
        var guids = Selection.assetGUIDs;
        if (guids.Length == 0)
        {
            Debug.LogWarning("Нет выделенных объектов в проекте.");
            return;
        }

        var texturesList = new System.Collections.Generic.List<Texture2D>();

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
            {
                texturesList.Add(tex);
            }
        }

        if (texturesList.Count == 0)
        {
            Debug.LogWarning("В выделении нет Texture2D.");
            return;
        }

        Undo.RecordObject(config, "Convert Textures to ElementTypes");

        var elementTypes = new ElementType[texturesList.Count];

        for (int i = 0; i < texturesList.Count; i++)
        {
            var tex = texturesList[i];

            // Создаём спрайт из текстуры
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(tex));
            if (sprite == null)
            {
                sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }

            elementTypes[i] = new ElementType
            {
                ID = tex.name,
                Sprite = sprite
            };
        }

        var so = new SerializedObject(config);
        var prop = so.FindProperty("availableTypes");
        if (prop != null)
        {
            prop.arraySize = elementTypes.Length;
            for (int i = 0; i < elementTypes.Length; i++)
            {
                var elemProp = prop.GetArrayElementAtIndex(i);
                var idProp = elemProp.FindPropertyRelative("ID");
                var spriteProp = elemProp.FindPropertyRelative("Sprite");

                idProp.stringValue = elementTypes[i].ID;
                spriteProp.objectReferenceValue = elementTypes[i].Sprite;
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
            Debug.Log($"Успешно конвертировано {elementTypes.Length} ElementType в {config.name}");
        }
        else
        {
            Debug.LogError("В GameConfig нет поля availableTypes");
        }
    }
}
