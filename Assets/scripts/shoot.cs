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
            ShowMessage("������� ����� �����! ��������: " + GameManager.max_gun);
            return;
        }

        Debug.Log("������ ��������");
        GameManager.Instance.shootButton.interactable = false;

        // ����� �������� ���������� ��������
        GameManager.Instance.StartCoroutine(EnableShootButtonAfterDelay(10f));

        int totalDamage = 0;
        List<GunInstance> gunsToRemove = new List<GunInstance>();

        foreach (GunInstance gun in GameManager.Instance.gunArray)
        {
            int x = gun.position.x;
            int y = gun.position.y;
            int shotDamage = gun.damage;

            // ����������� ����� magic (id == 4): ��������� ���� �� 1 �� 5
            if (gun.id == 4)
            {
                shotDamage = Random.Range(1, 6);
                Debug.Log($"magic �����: ��������� ���� {shotDamage}");
            }

            // ������� ������
            if (gun.direction == 1)
            {
                for (int tx = x + 1; tx < GameManager.Instance.gridSize; tx++)
                {
                    Vector2Int pos = new Vector2Int(tx, y);
                    if (GameManager.Instance.cellEffects.TryGetValue(pos, out string effect) && effect == "power")
                    {
                        shotDamage += 2;
                        Debug.Log($"���� 'power' �� {pos.x},{pos.y} +2 �����");
                    }
                }

                // ���������� ����� �� �����
                if (GameManager.Instance.currentEnemyHP > 0)
                {
                    GameManager.Instance.currentEnemyHP -= shotDamage;
                    GameManager.Instance.currentEnemyHP = Mathf.Max(GameManager.Instance.currentEnemyHP, 0);

                    GameManager.Instance.UpdateEnemyUI();
                    totalDamage += shotDamage;

                    Debug.Log($"����� {gun.id} �� ({x}, {y}) ������� {shotDamage} �����. �������� HP: {GameManager.Instance.currentEnemyHP}");
                }
            }

            // ��������� ������� ����� �����
            if (gun.lifetime > 0)
            {
                gun.lifetime -= 1;
                Debug.Log($"����� {gun.id} �� ({x},{y}) - �������� �����: {gun.lifetime}");

                if (gun.lifetime <= 0)
                {
                    gunsToRemove.Add(gun);
                    Debug.Log($"����� {gun.id} �� ({x},{y}) ��������������");
                }
            }
        }

        // ������� ������������ �����
        foreach (var gun in gunsToRemove)
        {
            GameManager.Instance.gunArray.Remove(gun);
        }

        // �������� ������
        if (GameManager.Instance.currentEnemyHP <= 0)
        {
            ShowMessage("���� �������!");
            GameManager.Instance.ShowStory();
            GameManager.Instance.shootButton.interactable = false;
            Debug.Log("������ �������� ��������� � ���� �������");
            GameManager.Instance.EnemyDefeated();
        }
        else // ���� ���� ����� � �� ������� �������� ����
        {
            GameManager.Instance.TakeDamageFromEnemy();
            GameManager.Instance.StartCoroutine(EnableShootButtonAfterDelay(2f));
        }

        if (totalDamage > 0)
        {
            GameManager.Instance.AddGold(15);
            ShowMessage($"�� ������� {totalDamage} ����� � �������� 15 �����!");
        }
        // ������������ ���������� ����� ����� ������ ����� ��������
        GameManager.Instance.ProcessBuildingLifetimes();
    }

    private IEnumerator EnableShootButtonAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        GameManager.Instance.shootButton.interactable = true;
        Debug.Log("������ �������� ����� ��������");
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