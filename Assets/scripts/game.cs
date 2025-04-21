using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Для UI-компонентов
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

    public GameObject card;
    public Transform contentParent;

    public GameObject Button_tower;
    public GameObject Button_gun;
    public GameObject Button_shoot;

    private shoot scriptShoot;

    

    private void Start()
    {GameManager gm = FindObjectOfType<GameManager>();
        islandGrid = new GameObject[gridSize, gridSize];
        islandData = new string[gridSize, gridSize];

        GenerateBoard();
        Button_tower_click();  // Загружаем карточки типа "tower"
    }

    void GenerateBoard()
    {
        float offsetX = (-gridSize * cellSize) / 2;
        float offsetY = (-gridSize * cellSize) / 2;

        for (int y = gridSize - 1; y >= 0; y--) // Начинаем с нижней строки
        {
            for (int x = 0; x < gridSize; x++) // Слева направо
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
    public void Button_shoot_click()
    {

        scriptShoot = FindObjectOfType<shoot>();
        scriptShoot.shootLogick();
    }

    void load_cards(string types)
    {
        // Получаем все карточки из GameManager
        List<BuildingData> buildings = GameManager.Instance.GetBuildingsByType(types);

        // Находим все объекты карточек, например, по тегу или вручную
        GameObject[] cards = new GameObject[5];
        cards[0] = GameObject.Find("card");
        cards[1] = GameObject.Find("card(1)");
        cards[2] = GameObject.Find("card(2)");
        cards[3] = GameObject.Find("card(3)");
        cards[4] = GameObject.Find("card(4)");

        // Заполняем карточки данными из GameManager
        for (int i = 0; i < cards.Length && i < buildings.Count; i++)
        {
            BuildingData building = buildings[i];
            GameObject cardObj = cards[i];

            if (cardObj != null)
            {
                var cardComponent = cardObj.GetComponent<BuildingCard>();
                if (cardComponent != null)
                {
                    // Устанавливаем данные карточки
                    cardComponent.SetCard(building.id, building.icon, building.name, building.description, building.price);
                }
                else
                {
                    Debug.LogError("На карточке отсутствует компонент BuildingCard!");
                }
            }
            else
            {
                Debug.LogError("Карточка с индексом " + i + " не найдена!");
            }
        }
    }
}
