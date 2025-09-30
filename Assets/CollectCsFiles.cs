using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class CollectCsFiles : EditorWindow
{
    [MenuItem("Tools/Collect CS Files")]
    public static void ShowWindow()
    {
        // Первый диалог: выбор папки с .cs файлами, начиная с Assets
        var assetsPath = Application.dataPath; // Путь к Assets (например, "C:/Project/Assets")
        var selectedPath =
            EditorUtility.OpenFolderPanel("Select Folder with CS Files (starting from Assets)", assetsPath, "");

        if (string.IsNullOrEmpty(selectedPath))
        {
            Debug.Log("No folder selected for CS files. Operation cancelled.");
            return;
        }

        // Проверяем, что выбранный путь начинается с Assets
        if (!selectedPath.StartsWith(assetsPath))
        {
            Debug.LogError("Selected folder for CS files must be inside the Assets directory!");
            return;
        }

        // Собираем все .cs файлы рекурсивно
        var csFiles = new List<string>();
        CollectCsFilesRecursive(selectedPath, csFiles);

        if (csFiles.Count == 0)
        {
            Debug.Log("No .cs files found in the selected folder and subfolders.");
            return;
        }

        // Собираем все директории рекурсивно (включая пустые)
        var directories = new List<string>();
        CollectDirectoriesRecursive(selectedPath, directories, selectedPath);

        // Читаем содержимое всех файлов
        var allContent = new StringBuilder();
        foreach (var filePath in csFiles)
        {
            try
            {
                var content = File.ReadAllText(filePath, Encoding.UTF8);
                allContent.AppendLine($"// === File: {Path.GetFileName(filePath)} ===");
                allContent.AppendLine(content);
                allContent.AppendLine(); // Пустая строка для разделения
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error reading file {filePath}: {e.Message}");
            }
        }

        // Добавляем раздел с файловой структурой (список .cs файлов с относительными путями)
        allContent.AppendLine("// === Directory Structure ===");
        allContent.AppendLine(
            "// List of all directories (including empty) with relative paths from the selected folder:");
        foreach (var dirPath in directories)
        {
            allContent.AppendLine($"// {dirPath}/");
        }

        allContent.AppendLine(); // Пустая строка для разделения

        allContent.AppendLine("// === File Structure ===");
        allContent.AppendLine("// List of all .cs files with relative paths from the selected folder:");
        foreach (var filePath in csFiles)
        {
            // Вычисляем относительный путь
            var relativePath = filePath[selectedPath.Length..]
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace('\\', '/');
            allContent.AppendLine($"// {relativePath}");
        }

        allContent.AppendLine(); // Пустая строка в конце

        // Второй диалог: выбор места и имени для сохранения файла
        var defaultName = "AllCsContent.txt";
        var outputPath = EditorUtility.SaveFilePanel("Save Combined CS Content As", assetsPath, defaultName, "txt");

        if (string.IsNullOrEmpty(outputPath))
        {
            Debug.Log("Save operation cancelled.");
            return;
        }

        // Сохраняем в выбранный файл
        try
        {
            File.WriteAllText(outputPath, allContent.ToString(), Encoding.UTF8);

            // Если файл сохранён внутри Assets, обновляем Unity
            if (outputPath.StartsWith(assetsPath))
            {
                AssetDatabase.Refresh();
            }

            Debug.Log(
                $"Successfully collected {csFiles.Count} .cs files and {directories.Count} directories into: {outputPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving file to {outputPath}: {e.Message}");
        }
    }

    // Рекурсивный поиск .cs файлов (как раньше)
    private static void CollectCsFilesRecursive(string directory, List<string> files)
    {
        var csFilesInDir = Directory.GetFiles(directory, "*.cs", SearchOption.TopDirectoryOnly);
        files.AddRange(csFilesInDir);

        // Рекурсия в поддиректории
        var subDirs = Directory.GetDirectories(directory);
        foreach (var subDir in subDirs)
        {
            if (!subDir.Contains("/Library/") && !subDir.Contains("/obj/") &&
                !subDir.Contains("/Temp/")) // Игнорируем служебные папки Unity
            {
                CollectCsFilesRecursive(subDir, files);
            }
        }
    }

    // Рекурсивный сбор всех директорий (включая пустые)
    private static void CollectDirectoriesRecursive(string directory, List<string> dirs, string basePath)
    {
        // Добавляем текущую директорию
        var relative = directory[basePath.Length..]
            .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace('\\', '/');
        if (string.IsNullOrEmpty(relative))
        {
            relative = "."; // Для корневой папки
        }

        dirs.Add(relative);

        // Рекурсия в поддиректории
        var subDirs = Directory.GetDirectories(directory);
        foreach (var subDir in subDirs)
        {
            if (!subDir.Contains("/Library/") && !subDir.Contains("/obj/") &&
                !subDir.Contains("/Temp/")) // Игнорируем служебные папки Unity
            {
                CollectDirectoriesRecursive(subDir, dirs, basePath);
            }
        }
    }
}