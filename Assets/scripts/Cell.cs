using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    public GameObject BuildButton; // ���� ������������� ������
    public Image ImgOnIsland; // ���������, ��� ��� ��������� Image

    private int placedBuildingId = -1; // ����� ������������ �����

    public int x; // ���������� ������ �� ��������
    public int y;

    public void EnableBuildButton()
    {
        BuildButton.SetActive(true);
        Debug.Log("������ ������������: " + BuildButton.name);
    }

    public void BuildSelected()
    {
        int selectedId = GameManager.selectedCardId;

        if (selectedId != -1)
        {
            // ������ ������ �� ID
            BuildingData selectedBuilding = GameManager.allBuildings.Find(building => building.id == selectedId);

            if (selectedBuilding != null)
            {
                Debug.Log("������ ������ � ID: " + selectedId + " �� ������ " + gameObject.name);

                placedBuildingId = selectedBuilding.id;


                // ���������, ��� ����������� ����������
                if (selectedBuilding.icon != null)
                {
                    ImgOnIsland.sprite = selectedBuilding.icon;
                }
                else
                {
                    Debug.LogError("�� ������� ����� ����������� ��� ������ � ID: " + selectedId);
                }

                BuildButton.SetActive(false); // �������� ������ ����� ���������
            }
            else
            {
                Debug.LogError("�� ������� ����� ������ � ID: " + selectedId);
            }
        }
        else
        {
            Debug.LogError("������ ������������ ID ������: " + selectedId);
        }
    }



    public void DisableBuildButton()
    {
        BuildButton.SetActive(false);
    }
}
