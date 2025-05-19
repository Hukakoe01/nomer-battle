using UnityEngine;

[System.Serializable]
public class BuildingData
{
    public int id;
    public string name;
    public string description;
    public int price;
    public string imageName;
    public string type;
    public Sprite icon;
    public int chek;//0 �������� ���� ���, 1 ���� �������� ������ ��� 
    public int damage;
    public int cost;
    public float lifetime;
}

// GunInstance.cs
[System.Serializable]
public class GunInstance
{
    public int id;
    public Vector2Int position;
    public int direction; // 0 - �����, 1 - ������, 2 - ����, 3 - �����
    public int damage;
    public int cost;
    public float lifetime;
}

// BuildingInstance.cs
[System.Serializable]
public class BuildingInstance
{
    public int id;
    public Vector2Int pos;
    public int influenceRadius; // ��� ������ ���������
    public int cost;
}

[System.Serializable]
public class EnemyData
{
    public int id;
    public string name;
    public int maxHP;
    public int armor;
    public string special; // ��������: "fire_resist", "double_speed", "split_on_death"
    public int damage;
    public int gold;
    public int exp;
    public Sprite sprite;
}
