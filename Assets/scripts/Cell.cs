using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Cell : MonoBehaviour
{
    public GameObject BuildButton; // Сюда перетаскиваем кнопку
    public Image ImgOnIsland; // Убедитесь, что это компонент Image

    private int placedBuildingId = -1; // Можно использовать потом

    public int x; // Координаты клетки на островке
    public int y;

    private bool isPlaced = false;
    private BuildingData tempData;
    private Vector2Int tempPos;




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
            BuildingData selectedBuilding = GameManager.allBuildings.Find(building => building.id == selectedId);

            if (selectedBuilding != null)
            {
                Debug.Log("Строим здание с ID: " + selectedId + " на клетке (" + x + ", " + y + ")");
                // Проверка наличия золота
                if (GameManager.Instance.playerGold < selectedBuilding.price)
                {
                    GameManager.Instance.ShowMessage("Недостаточно монет!");
                    return;
                }

                GameManager.Instance.SpendGold(selectedBuilding.price);
                placedBuildingId = selectedBuilding.id;
                GameManager.Instance.islands[x, y].Add(new int[] { selectedBuilding.id, 0 });

                if (selectedBuilding.icon != null)
                    ImgOnIsland.sprite = selectedBuilding.icon;

                if (selectedBuilding.type == "gun")
                {
                    GameManager.Instance.AddGun(selectedBuilding, new Vector2Int(x, y), 1); // 1 = вправо
                }
                else
                {
                    GameManager.Instance.currentBuildMode = GameManager.BuildMode.None;
                    GameManager.Instance.ApplyBuffs(x, y, selectedBuilding.id);
                }

                GameManager.Instance.CancelBuildingMode();
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);


                // Скрываем кнопку на текущей клетке
                Button btn = GetComponentInChildren<Button>();
                if (btn != null)
                    btn.gameObject.SetActive(false);
                else
                    Debug.LogWarning("На объекте Cell нет компонента Button!");

                // Скрываем все остальные кнопки
                foreach (GameObject obj in GameManager.Instance.cellObjects)
                {
                    if (obj != null)
                    {
                        Cell cell = obj.GetComponent<Cell>();
                        if (cell != null)
                            cell.HideButton();
                    }
                }
            }
        }
    }


    public void SelectDirection()
    {
        if (!GameManager.isGunPlacementPending) return;

        Vector2Int dir = new Vector2Int(x, y) - GameManager.tempGunPosition;

        // Проверка, что клетка соседняя
        if (Mathf.Abs(dir.x) + Mathf.Abs(dir.y) != 1) return;

        // Преобразуем направление в число
        int direction = 0;
        if (dir.x == 1) direction = 1;      // вправо
        else if (dir.x == -1) direction = 2; // влево
        else if (dir.y == 1) direction = 3;  // вверх
        else if (dir.y == -1) direction = 4; // вниз

        // Добавим в массив пушек
        GameManager.Instance.AddGun(
            GameManager.tempGunData,
            GameManager.tempGunPosition,
            direction
        );

        // Очистка
        GameManager.isGunPlacementPending = false;
    }

    public void HighlightNeighborCells()
    {
        Vector2Int[] directions = new Vector2Int[]
        {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left
        };

        foreach (var dir in directions)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;

            if (GameManager.Instance.IsValidCell(nx, ny))
            {
                GameObject cellObj = GameManager.Instance.GetCellObject(nx, ny);
                if (cellObj != null)
                {
                    // Добавим эффект моргания
                    lightcell blink = cellObj.GetComponent<lightcell>();
                    if (blink == null)
                        blink = cellObj.AddComponent<lightcell>();
                    blink.StartBlinking();
                }
            }
        }
    }

    void OnMouseUpAsButton()
    {
        if (GameManager.Instance.isRemovingMode)
        {
            RemoveBuilding();
            return;
        }
    }

    public void RemoveBuilding()
    {
        List<int[]> buildings = GameManager.Instance.islands[x, y];

        if (buildings == null || buildings.Count == 0)
        {
            GameManager.Instance.ShowMessage("На этой клетке нет построек");
            return;
        }

        // Удаляем пушку из gunArray, если она есть
        GunInstance toRemove = null;
        foreach (GunInstance gun in GameManager.Instance.gunArray)
        {
            if (gun.position == new Vector2Int(x, y))
            {
                toRemove = gun;
                break;
            }
        }

        if (toRemove != null)
        {
            GameManager.Instance.gunArray.Remove(toRemove);
            GameManager.Instance.ShowMessage("Пушка удалена");
        }
        else
        {
            GameManager.Instance.ShowMessage("Здание удалено");
        }

        // Вернуть золото (по желанию)
        foreach (var data in buildings)
        {
            int bId = data[0];
            BuildingData bd = GameManager.Instance.GetBuildingById(bId);
            if (bd != null)
            {
                GameManager.Instance.AddGold(bd.price / 2); // вернуть 50% стоимости
            }
        }

        // Очистить клетку
        buildings.Clear();

        // Убрать спрайт на клетке
        if (ImgOnIsland != null)
        {
            ImgOnIsland.sprite = null;
        }

        // Сброс режима удаления
        GameManager.Instance.isRemovingMode = false;
    }

    public void EnableRemoveButton()
    {
        // Включаем кнопку
        if (BuildButton != null)
        {
            BuildButton.SetActive(true);

            // Удаляем старые действия
            Button btn = BuildButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners(); // удаляем всё, что было
                btn.onClick.AddListener(RemoveBuildingFromButton); // добавляем удаление
            }
            else
            {
                Debug.LogWarning("На кнопке нет компонента Button");
            }
        }
        else
        {
            Debug.LogWarning("BuildButton не назначен в Cell!");
        }
    }

    public void RemoveBuildingFromButton()
    {
        List<int[]> buildings = GameManager.Instance.islands[x, y];

        if (buildings == null || buildings.Count == 0)
        {
            GameManager.Instance.ShowMessage("На этой клетке ничего нет");
            return;
        }

        // Удалим пушку из gunArray, если она есть
        GunInstance toRemoveGun = null;
        foreach (var gun in GameManager.Instance.gunArray)
        {
            if (gun.position == new Vector2Int(x, y))
            {
                toRemoveGun = gun;
                break;
            }
        }

        if (toRemoveGun != null)
        {
            GameManager.Instance.gunArray.Remove(toRemoveGun);
            Debug.Log("Пушка удалена");
        }
        else
        {
            Debug.Log("Здание удалено");
        }

        // Вернуть часть стоимости (опционально)
        foreach (var data in buildings)
        {
            BuildingData bd = GameManager.Instance.GetBuildingById(data[0]);
            if (bd != null)
            {
                GameManager.Instance.AddGold(bd.price / 2);
            }
        }

        // Очищаем список построек
        buildings.Clear();

        // Меняем спрайт на sky.png
        Sprite skySprite = Resources.Load<Sprite>("sky");
        if (ImgOnIsland != null)
            ImgOnIsland.sprite = skySprite;

        // Скрываем кнопку на этой клетке
        if (BuildButton != null)
            BuildButton.SetActive(false);

        GameManager.Instance.ShowMessage("Постройка удалена");

        // Выключаем режим удаления (если нужно только один раз)
        GameManager.Instance.CancelRemoveMode();
    }

    public void HideButton()
    {
        Button btn = GetComponentInChildren<Button>();
        if (btn != null)
        {
            // Отключаем GameObject, на котором висит сам Button (вместе с его визуалом)
            btn.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Кнопка не найдена внутри клетки!");
        }
    }

    public void OnDeleteButtonClicked()
    {
        GameManager.Instance.StartRemoveMode();
    }


    public void DisableBuildButton()
    {
        GameManager.Instance.CancelBuildingMode();

    }
}
