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
                Debug.Log("Строим здание с ID: " + selectedId + " на клетке " + gameObject.name);

                placedBuildingId = selectedBuilding.id;


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
            else
            {
                Debug.LogError("Не удалось найти здание с ID: " + selectedId);
            }
        }
        else
        {
            Debug.LogError("Выбран некорректный ID здания: " + selectedId);
        }
    }



    public void DisableBuildButton()
    {
        BuildButton.SetActive(false);
    }
}
