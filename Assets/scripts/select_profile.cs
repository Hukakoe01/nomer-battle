using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;

public class select_profile : MonoBehaviour
{
    int tutorialStatus = -1;
    public void button_profile1()
    {
        tutorialStatus = GetTutorialStatus(1);
        next_step();
    }

    public void button_profile2()
    {
        tutorialStatus = GetTutorialStatus(2);
        next_step();
    }

    public void button_profile3()
    {
        tutorialStatus = GetTutorialStatus(3);
        next_step();
    }
    public void next_step()
    {
        if (tutorialStatus == 0)
        {
            SceneManager.LoadScene("TutorialScene");
        }
        else if (tutorialStatus == 1)
        {
            SceneManager.LoadScene("SampleScene");
        }
    }
    public void button_returns()
    {
        SceneManager.LoadScene(0);
    }
    private string dbPath;
    // Start is called before the first frame update
    void Start()
    {
        // Формируем корректный путь
        string fileName = "data.db";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        // Если база данных еще не скопирована, копируем её из исходного пути
        //string sourcePath = Path.Combine(Application.streamingAssetsPath, fileName);
        //if (!File.Exists(filePath))
        //{
        //if (File.Exists(sourcePath))
        //{
        //File.Copy(sourcePath, filePath);
        //}
        //}
        //File.Copy(sourcePath, filePath);
        dbPath = "URI=file:" + filePath;
    }



    private int GetTutorialStatus(int nom)
    {
        if (!File.Exists(dbPath.Replace("URI=file:", ""))) // Проверяем существование файла без "URI=file:"
        {
            Debug.LogError("База данных не найдена: " + dbPath);
            return -1;
        }

        using (var conn = new SqliteConnection(dbPath)) // Оставляем URI=file: только в SqliteConnection
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT tutorial FROM profile WHERE ID = @id";
                cmd.Parameters.AddWithValue("@id", nom); // Используем параметры, чтобы избежать SQL-инъекций

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetInt32(0);
                    }
                }
            }
        }
        return -1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
