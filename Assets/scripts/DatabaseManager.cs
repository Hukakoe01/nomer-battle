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
            Debug.Log("���� ������ ����������� �� StreamingAssets �: " + targetPath);
        }
        else
        {
            Debug.Log("���� ��� ���������� �� ����: " + targetPath);
        }

        return targetPath;
    }
}
    