using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shoot : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameManager.kolvo_gun = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void shootLogick()
    {
        Debug.Log("������� �� ������� 1 ����");
        for (int y = 0; y < GameManager.Instance.gridSize; y++)
        {
            for (int x = 0; x < GameManager.Instance.gridSize; x++)
            {
                // ���������, ���� �� ������ � ���� ������
                List<int[]> cellBuildings = GameManager.Instance.islands[x, y];
                if (cellBuildings != null)
                {
                    foreach (var data in cellBuildings)
                    {
                        int buildingId = data[0];
                        int check = data[1];

                        // �������� ���� � ������
                        BuildingData building = GameManager.Instance.GetBuildingById(buildingId);

                        // ���������, ��� ��� "tower" � ������ ��� �� �����������
                        if (building != null && building.type == "tower" && check == 0)
                        {
                            Debug.Log($"tower � ID {buildingId} �� ������ {x},{y} ������������");

                            // ����� �������� ��������
                            // �����������, �� �������� ����� ����:
                            // building.ExecuteBuildingAction(x, y);
                            // �� ������ ������ ������� � �������

                            // ��������, ��� ������ �����������
                            data[1] = 1;

                            // ���� ����� ��������������� �� ������ tower � ��������������:
                            // return;
                        }
                        else if (building != null && building.type == "gun")
                        {
                            if (GameManager.max_gun == GameManager.kolvo_gun) 
                            {
                                
                            }
                            GameManager.kolvo_gun += 1;
                        }
                    }
                }
            }
        }
    }

}
