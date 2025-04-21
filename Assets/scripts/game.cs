using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Для UI-компонентов
using Mono.Data.Sqlite;
using System.Data;
using System.IO;

public class game : MonoBehaviour
{
    public int gridSize = 10; // Размер поля 10x10
    public GameObject cellPrefab; // Префаб клетки
    public float cellSize = 100f; // Размер клетки (в пикселях)
    public Canvas canvas; // Ссылка на Canvas (назначь в инспекторе!)
    public Camera mainCamera;

    // Например, если поле 10x10:
    public GameObject[,] islandGrid; // Сами острова
    public string[,] islandData;     // Названия зданий на них (или ID)


    public string fileName = "data.db";
    private string dbPath;

    public GameObject card; 
    public Transform contentParent;

    public GameObject Button_tower;
    public GameObject Button_gun;


    private void Start()
    {
        islandGrid = new GameObject[gridSize, gridSize];
        islandData = new string[gridSize, gridSize];

        GenerateBoard();
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        dbPath = "URI=file:" + filePath;
        Button_tower_click();
    }

    void GenerateBoard()
    {

        float offsetX = (-gridSize * cellSize) / 2;
        float offsetY = (-gridSize * cellSize) / 2;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector3 position = new Vector3(x * cellSize + offsetX, y * cellSize + offsetY, 0);

                GameObject Cell = Instantiate(cellPrefab);
                Cell.transform.SetParent(canvas.transform, false); // Делаем клетку дочерним объектом Canvas
                Cell.transform.localPosition = position; // Устанавливаем позицию внутри Canvas

                islandGrid[x, y] = Cell;
                islandData[x, y] = "";


                RectTransform rectTransform = Cell.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = new Vector2(cellSize, cellSize); // Устанавливаем размер клетки
                }
            }
        }
    }

    public void Button_tower_click() 
    {
        load_cards("tower");
    }
    public void Button_gun_click()
    {
        load_cards("gun");
    }

    void load_cards(string types)
    {
        // Находим все объекты карточек, например, по тегу или вручную
        GameObject[] cards = new GameObject[5];
        cards[0] = GameObject.Find("card");
        cards[1] = GameObject.Find("card(1)");
        cards[2] = GameObject.Find("card(2)");
        cards[3] = GameObject.Find("card(3)");
        cards[4] = GameObject.Find("card(4)");

        using (var conn = new SqliteConnection(dbPath))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT id, name, about, price, image, type FROM towers where type = '{types}'";
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    int index = 0;
                    while (reader.Read() && index < cards.Length)
                    {
                        int id = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        string description = reader.GetString(2);
                        int price = reader.GetInt32(3);
                        string iconName = reader.GetString(4);
                        string type = reader.GetString(5);

                        

                        Sprite icon = Resources.Load<Sprite>("photo/" + iconName);

                        GameObject cardObj = cards[index];
                        if (cardObj != null)
                        {
                            var cardt = cardObj.GetComponent<BuildingCard>();
                            if (cardt != null)
                            {
                                cardt.SetCard(id, icon, name, description, price);
                            }
                            else
                            {
                                Debug.LogError("На карточке отсутствует компонент BuildingCard!");
                            }
                        }
                        else
                        {
                            Debug.LogError("Карточка с индексом " + index + " не найдена!");
                        }

                        index++;
                    }
                }
            }
        }
    }
}
