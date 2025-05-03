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
                Debug.Log("������ ������ � ID: " + selectedId + " �� ������ (" + x + ", " + y + ")");


                placedBuildingId = selectedBuilding.id;

                GameManager.Instance.islands[x, y].Add(new int[] { selectedBuilding.id, 0 });

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
        }
    }



    public void DisableBuildButton()
    {
        BuildButton.SetActive(false);
    }
}
