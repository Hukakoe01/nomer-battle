using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class MultiplayerGameController : MonoBehaviour
{
    public TMP_Text messageText;
    public Button shootButton;

    private void Start()
    {
        if (shootButton != null)
            shootButton.onClick.AddListener(OnShootClicked);
        else
            Debug.LogError("������ �������� �� ���������!");
    }

    public void OnShootClicked()
    {
        shootButton.interactable = false;
        StartCoroutine(EnableShootButtonAfterDelay(5f));

        int totalDamage = CalculateDamage();

        if (totalDamage > 0)
        {
            GameManager.Instance.AddGold(5);
            ShowMessage($"�� ������� {totalDamage} ����� � �������� 5 �����!");
            StartCoroutine(SendDamageToServer(totalDamage));
        }
        else
        {
            ShowMessage("����� �� ������� ����");
        }
    }

    // ��������� ����������� ������:
    private IEnumerator EnableShootButtonAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (shootButton != null)
        {
            shootButton.interactable = true;
            Debug.Log("������ �������� ����� �������");
        }
    }

    private void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
            messageText.gameObject.SetActive(true);
            StartCoroutine(HideMessageAfterDelay());
        }
    }

    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }

    private int CalculateDamage()
    {
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
                            Debug.Log($"���� 'power' �� {pos.x},{pos.y} +2 �����");
                        }
                    }
                }
            }

            Debug.Log($"����� �� {x},{y} ������� {shotDamage} ����� (� ������������)");
            totalDamage += shotDamage;
        }

        return totalDamage;
    }

    private IEnumerator SendDamageToServer(int damage)
    {
        string baseUrl = "http://" + PlayerPrefs.GetString("ServerIP", "localhost") + ":3000";
        int playerId = PlayerPrefs.GetInt("PlayerID", -1);

        WWWForm form = new WWWForm();
        form.AddField("playerId", playerId);
        form.AddField("damage", damage);

        UnityWebRequest request = UnityWebRequest.Post($"{baseUrl}/move", form);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("���� ������� ��������� �� ������");
        }
        else
        {
            Debug.LogError("������ �������� �����: " + request.error);
        }
    }
}