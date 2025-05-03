using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shoot : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameManager.kolvo_gun = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void shootLogick()
    {
        Debug.Log("выстрел вы нанесли 1 урон");
        for (int y = 0; y < GameManager.Instance.gridSize; y++)
        {
            for (int x = 0; x < GameManager.Instance.gridSize; x++)
            {
                // Проверяем, есть ли здания в этой клетке
                List<int[]> cellBuildings = GameManager.Instance.islands[x, y];
                if (cellBuildings != null)
                {
                    foreach (var data in cellBuildings)
                    {
                        int buildingId = data[0];
                        int check = data[1];

                        // Получаем инфу о здании
                        BuildingData building = GameManager.Instance.GetBuildingById(buildingId);

                        // Проверяем, что тип "tower" и здание ещё не действовало
                        if (building != null && building.type == "tower" && check == 0)
                        {
                            Debug.Log($"tower с ID {buildingId} на клетке {x},{y} активируется");

                            // Здесь вызываем действие
                            // Предположим, ты добавишь метод типа:
                            // building.ExecuteBuildingAction(x, y);
                            // но сейчас просто выводим в консоль

                            // Помечаем, что здание действовало
                            data[1] = 1;

                            // Если нужно останавливаться на первом tower — раскомментируй:
                            // return;
                        }
                        else if (building != null && building.type == "gun")
                        {
                            if (GameManager.max_gun == GameManager.kolvo_gun) 
                            {
                                
                            }
                            GameManager.kolvo_gun += 1;
                        }
                    }
                }
            }
        }
    }

}
