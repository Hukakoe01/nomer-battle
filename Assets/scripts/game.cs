using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class game : MonoBehaviour
{
    public int gridSize = 10;
    public GameObject cellPrefab;
    public float cellSize = 100f;
    public Canvas canvas;
    public Camera mainCamera;

    public GameObject[,] islandGrid;
    public string[,] islandData;

    public GameObject card;
    public Transform contentParent;

    public GameObject Button_tower;
    public GameObject Button_gun;
    public GameObject Button_shoot;

    private shoot scriptShoot;

    private void Start()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        islandGrid = new GameObject[gridSize, gridSize];
        islandData = new string[gridSize, gridSize];

        GenerateBoard();
        Button_tower_click();
        GameManager.kolvo_gun = 0;
        GameManager.Instance.InitEnemies();
        GameManager.Instance.SpawnEnemy(0);
    }

    void GenerateBoard()
    {
        GameManager.Instance.cellObjects = new GameObject[gridSize, gridSize];
        float offsetX = (-gridSize * cellSize) / 2;
        float offsetY = (gridSize * cellSize) / 2;

        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                Vector3 position = new Vector3(x * cellSize + offsetX, offsetY - y * cellSize, 0);

                GameObject Cell = Instantiate(cellPrefab);
                Cell.transform.SetParent(canvas.transform, false);
                Cell.transform.localPosition = position;

                islandGrid[x, y] = Cell;
                islandData[x, y] = "";

                Cell.GetComponent<Cell>().x = x;
                Cell.GetComponent<Cell>().y = y;

                GameManager.Instance.cellObjects[x, y] = Cell;

                RectTransform rectTransform = Cell.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = new Vector2(cellSize, cellSize);
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
        List<BuildingData> buildings = GameManager.Instance.GetBuildingsByType(types);

        GameObject[] cards = new GameObject[5];
        cards[0] = GameObject.Find("card");
        cards[1] = GameObject.Find("card(1)");
        cards[2] = GameObject.Find("card(2)");
        cards[3] = GameObject.Find("card(3)");
        cards[4] = GameObject.Find("card(4)");

        for (int i = 0; i < cards.Length && i < buildings.Count; i++)
        {
            BuildingData building = buildings[i];
            GameObject cardObj = cards[i];

            if (cardObj != null)
            {
                var cardComponent = cardObj.GetComponent<BuildingCard>();
                if (cardComponent != null)
                {
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
