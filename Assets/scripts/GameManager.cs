using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System.IO;

// GameManager.cs

public class GameManager : MonoBehaviour  // Наследуем от MonoBehaviour
{
    public static GameManager Instance;
    public static int selectedCardId = -1; // -1 = ничего не выбрано

    public static List<BuildingData> allBuildings = new List<BuildingData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadBuildingsFromDB();
        }
        else
        {
            Destroy(gameObject);
        }
    }


    void LoadBuildingsFromDB()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "data.db");
        string dbPath = "URI=file:" + filePath;

        using (var conn = new SqliteConnection(dbPath))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id, name, about, price, image, type FROM towers";
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BuildingData building = new BuildingData();

                        building.id = reader.GetInt32(0);
                        building.name = reader.GetString(1);
                        building.description = reader.GetString(2);
                        building.price = reader.GetInt32(3);
                        building.imageName = reader.GetString(4);
                        building.type = reader.GetString(5);

                        building.icon = Resources.Load<Sprite>("photo/" + building.imageName);

                        // Добавляем отладку для проверки загруженных данных
                        Debug.Log($"Здание загружено: {building.name}, ID: {building.id}, Цена: {building.price}");

                        allBuildings.Add(building);
                    }
                }
            }
        }
    }

    public List<BuildingData> GetBuildingsByType(string type)
    {
        return allBuildings.FindAll(building => building.type == type);
    }

    void PrintAllBuildings()
        {
            foreach (var building in allBuildings)
            {
                Debug.Log($"Building ID: {building.id}, Name: {building.name}, Price: {building.price}, Type: {building.type}");
            }
        }
}
