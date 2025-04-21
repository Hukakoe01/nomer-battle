using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public GameObject BuildButton; // ���� ������������� ������

    private int placedBuildingId = -1; // ����� ������������ �����

    public void EnableBuildButton()
    {
        try
        {
            if (BuildButton != null)
            {
                BuildButton.SetActive(true);
                Debug.Log("������ ������������: " + BuildButton.name);
            }
            else
            {
                throw new System.NullReferenceException("������ �� ���������!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"������: {e.Message} �� ������� {gameObject.name}. ������� ���� ������.");

            // ������� ���� ������
            Destroy(gameObject);
        }
    }



    public void BuildSelected()
    {
        int selectedId = GameManager.selectedCardId;

        if (selectedId != -1)
        {
            Debug.Log("������ ������ � ID: " + selectedId + " �� ������ " + gameObject.name);
            placedBuildingId = selectedId;

            // ����� ����� ��������� ��������, ������ � �.�.
            BuildButton.SetActive(false); // �������� ������ ����� ���������
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