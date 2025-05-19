using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class shoot : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void CountAllGuns()
    {
        for (int y = 0; y < GameManager.Instance.gridSize; y++)
        {
            for (int x = 0; x < GameManager.Instance.gridSize; x++)
            {
                List<int[]> cellBuildings = GameManager.Instance.islands[x, y];
                if (cellBuildings != null)
                {
                    foreach (var data in cellBuildings)
                    {
                        BuildingData building = GameManager.Instance.GetBuildingById(data[0]);
                        if (building != null && building.type == "gun")
                        {
                            GameManager.kolvo_gun += 1;
                        }
                    }
                }
            }
        }

        

    }


    public void shootLogick()
    {
        if (GameManager.Instance.gunArray.Count > GameManager.max_gun)
        {
            ShowMessage("Слишком много пушек! Максимум: " + GameManager.max_gun);
            return;
        }

        Debug.Log("Запуск стрельбы");

        // Отключаем кнопку сразу
        GameManager.Instance.shootButton.interactable = false;
        GameManager.Instance.StartCoroutine(EnableShootButtonAfterDelay(10f));

        int totalDamage = 0;

        foreach (GunInstance gun in GameManager.Instance.gunArray)
        {
            int x = gun.position.x;
            int y = gun.position.y;
            int shotDamage = gun.damage;

            if (gun.direction == 1)
            {
                for (int tx = x + 1; tx < GameManager.Instance.gridSize; tx++)
                {
                    Vector2Int pos = new Vector2Int(tx, y);
                    if (GameManager.Instance.cellEffects.TryGetValue(pos, out string effect))
                    {
                        if (effect == "power")
                        {
                            shotDamage += 2;
                            Debug.Log($"Бафф 'power' на {pos.x},{pos.y} +2 урон");
                        }
                    }
                }

                if (GameManager.Instance.currentEnemyHP > 0)
                {
                    GameManager.Instance.currentEnemyHP -= shotDamage;
                    if (GameManager.Instance.currentEnemyHP < 0)
                        GameManager.Instance.currentEnemyHP = 0;

                    GameManager.Instance.UpdateEnemyUI();
                    Debug.Log($"Пушка на {x},{y} нанесла {shotDamage} урона! Осталось HP: {GameManager.Instance.currentEnemyHP}");

                    totalDamage += shotDamage;
                }
            }
        }
        if (GameManager.Instance.currentEnemyHP <= 0)
        {
            GameManager.Instance.shootButton.interactable = false;
            GameManager.Instance.StartCoroutine(EnableShootButtonAfterDelay(10f));
            ShowMessage("Враг побеждён!");
            GameManager.Instance.EnemyDefeated();
        }
        else
        {
            GameManager.Instance.TakeDamageFromEnemy();
            GameManager.Instance.shootButton.interactable = false;
            GameManager.Instance.StartCoroutine(EnableShootButtonAfterDelay(2f));

        }

        // После подсчёта урона:
        if (totalDamage > 0)
        {
            GameManager.Instance.AddGold(5);
            ShowMessage($"Вы нанесли {totalDamage} урона за ход и получили 5 монет!");
        }
    }

    private IEnumerator DisableShootButtonTemporarily(float seconds)
    {
        GameManager.Instance.shootButton.interactable = false;
        yield return new WaitForSeconds(seconds);
        GameManager.Instance.shootButton.interactable = true;
    }

    private IEnumerator EnableShootButtonAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        GameManager.Instance.shootButton.interactable = true;
        Debug.Log("Кнопка стрельбы снова включена");
    }
    private IEnumerator WaitThenVictory()
    {
        yield return new WaitForSeconds(0.6f);
        GameManager.Instance.StartCoroutine(DisableShootButtonTemporarily(10f));
        ShowMessage("Враг побеждён!");
        GameManager.Instance.EnemyDefeated();

    }



    private void ShowMessage(string message)
    {
            GameManager.Instance.TText.text = message;
            GameManager.Instance.TText.gameObject.SetActive(true);

            GameManager.Instance.StartCoroutine(HideMessageAfterDelay());
    }
    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        GameManager.Instance.TText.gameObject.SetActive(false);
    }

    
}
