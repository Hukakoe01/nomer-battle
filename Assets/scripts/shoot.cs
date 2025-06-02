using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class shoot : MonoBehaviour
{
    public void shootLogick()
    {
        if (GameManager.Instance.gunArray.Count > GameManager.max_gun)
        {
            ShowMessage("Слишком много пушек! Максимум: " + GameManager.max_gun);
            return;
        }

        Debug.Log("Запуск стрельбы");
        GameManager.Instance.shootButton.interactable = false;

        // Старт задержки повторного выстрела
        GameManager.Instance.StartCoroutine(EnableShootButtonAfterDelay(10f));

        int totalDamage = 0;
        List<GunInstance> gunsToRemove = new List<GunInstance>();

        foreach (GunInstance gun in GameManager.Instance.gunArray)
        {
            int x = gun.position.x;
            int y = gun.position.y;
            int shotDamage = gun.damage;

            // Особенность пушки magic (id == 4): случайный урон от 1 до 5
            if (gun.id == 4)
            {
                shotDamage = Random.Range(1, 6);
                Debug.Log($"magic пушка: случайный урон {shotDamage}");
            }

            // Обстрел вправо
            if (gun.direction == 1)
            {
                for (int tx = x + 1; tx < GameManager.Instance.gridSize; tx++)
                {
                    Vector2Int pos = new Vector2Int(tx, y);
                    if (GameManager.Instance.cellEffects.TryGetValue(pos, out string effect) && effect == "power")
                    {
                        shotDamage += 2;
                        Debug.Log($"Бафф 'power' на {pos.x},{pos.y} +2 урона");
                    }
                }

                // Применение урона по врагу
                if (GameManager.Instance.currentEnemyHP > 0)
                {
                    GameManager.Instance.currentEnemyHP -= shotDamage;
                    GameManager.Instance.currentEnemyHP = Mathf.Max(GameManager.Instance.currentEnemyHP, 0);

                    GameManager.Instance.UpdateEnemyUI();
                    totalDamage += shotDamage;

                    Debug.Log($"Пушка {gun.id} на ({x}, {y}) нанесла {shotDamage} урона. Осталось HP: {GameManager.Instance.currentEnemyHP}");
                }
            }

            // Обработка времени жизни пушки
            if (gun.lifetime > 0)
            {
                gun.lifetime -= 1;
                Debug.Log($"Пушка {gun.id} на ({x},{y}) - осталось жизни: {gun.lifetime}");

                if (gun.lifetime <= 0)
                {
                    gunsToRemove.Add(gun);
                    Debug.Log($"Пушка {gun.id} на ({x},{y}) самоуничтожена");
                }
            }
        }

        // Удаляем отработавшие пушки
        foreach (var gun in gunsToRemove)
        {
            GameManager.Instance.gunArray.Remove(gun);
        }

        // Проверка победы
        if (GameManager.Instance.currentEnemyHP <= 0)
        {
            ShowMessage("Враг побеждён!");
            GameManager.Instance.ShowStory();
            GameManager.Instance.shootButton.interactable = false;
            Debug.Log("Кнопка стрельбы отключена — враг побеждён");
            GameManager.Instance.EnemyDefeated();
        }
        else // Если враг выжил — он наносит ответный удар
        {
            GameManager.Instance.TakeDamageFromEnemy();
            GameManager.Instance.StartCoroutine(EnableShootButtonAfterDelay(2f));
        }

        if (totalDamage > 0)
        {
            GameManager.Instance.AddGold(15);
            ShowMessage($"Вы нанесли {totalDamage} урона и получили 15 монет!");
        }
        // Обрабатываем уменьшение срока жизни зданий после выстрела
        GameManager.Instance.ProcessBuildingLifetimes();
    }

    private IEnumerator EnableShootButtonAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        GameManager.Instance.shootButton.interactable = true;
        Debug.Log("Кнопка стрельбы снова включена");
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