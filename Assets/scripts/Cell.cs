using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    public GameObject BuildButton; // Сюда перетаскиваем кнопку
    public Image ImgOnIsland; // Убедитесь, что это компонент Image

    private int placedBuildingId = -1; // Можно использовать потом

    public int x; // Координаты клетки на островке
    public int y;

    public void EnableBuildButton()
    {
        BuildButton.SetActive(true);
        Debug.Log("Кнопка активирована: " + BuildButton.name);
    }

    public void BuildSelected()
    {
        int selectedId = GameManager.selectedCardId;

        if (selectedId != -1)
        {
            // Найдем здание по ID
            BuildingData selectedBuilding = GameManager.allBuildings.Find(building => building.id == selectedId);

            if (selectedBuilding != null)
            {
                Debug.Log("Строим здание с ID: " + selectedId + " на клетке (" + x + ", " + y + ")");


                placedBuildingId = selectedBuilding.id;

                GameManager.Instance.islands[x, y].Add(new int[] { selectedBuilding.id, 0 });

                // Проверяем, что изображение существует
                if (selectedBuilding.icon != null)
                {
                    ImgOnIsland.sprite = selectedBuilding.icon;
                }
                else
                {
                    Debug.LogError("Не удалось найти изображение для здания с ID: " + selectedId);
                }

                BuildButton.SetActive(false); // Скрываем кнопку после постройки
            }
        }
    }



    public void DisableBuildButton()
    {
        BuildButton.SetActive(false);
    }
}
