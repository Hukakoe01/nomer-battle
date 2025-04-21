using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public GameObject BuildButton; // Сюда перетаскиваем кнопку

    private int placedBuildingId = -1; // Можно использовать потом

    public void EnableBuildButton()
    {
        try
        {
            if (BuildButton != null)
            {
                BuildButton.SetActive(true);
                Debug.Log("Кнопка активирована: " + BuildButton.name);
            }
            else
            {
                throw new System.NullReferenceException("Кнопка не привязана!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка: {e.Message} на объекте {gameObject.name}. Удаляем весь остров.");

            // Удаляем саму клетку
            Destroy(gameObject);
        }
    }



    public void BuildSelected()
    {
        int selectedId = GameManager.selectedCardId;

        if (selectedId != -1)
        {
            Debug.Log("Строим здание с ID: " + selectedId + " на клетке " + gameObject.name);
            placedBuildingId = selectedId;

            // Здесь можно поставить картинку, модель и т.д.
            BuildButton.SetActive(false); // спрятать кнопку после постройки
        }
    }
    public void DisableBuildButton()
    {
        if (BuildButton != null)
        {
            BuildButton.SetActive(false);
        }
    }

}