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
    public int chek;//0 смотреть один раз, 1 надо смотреть каждый раз 
}
