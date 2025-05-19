using System.IO;
using UnityEngine;

public static class DatabaseManager
{
    public static string dbFileName = "data.db";

    public static string GetDatabasePath()
    {
        string targetPath = Path.Combine(Application.persistentDataPath, dbFileName);

        if (!File.Exists(targetPath))
        {
            string sourcePath = Path.Combine(Application.streamingAssetsPath, dbFileName);
            File.Copy(sourcePath, targetPath);
            Debug.Log("База данных скопирована из StreamingAssets в: " + targetPath);
        }
        else
        {
            Debug.Log("База уже существует по пути: " + targetPath);
        }

        return targetPath;
    }
}
    