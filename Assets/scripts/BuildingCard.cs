using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;



public class BuildingCard : MonoBehaviour
{

    

    // Start is called before the first frame update
    void Start()

    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    bool sch = false;

    public int id;
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text priceText;

    public void SetCard(int idn, Sprite icon, string name, string description, int price)
    {
        this.id = idn;
        iconImage.sprite = icon;
        nameText.text = name;
        descriptionText.text = description;
        priceText.text = price + " золота";
    }
    public void OnClick()
    {
        if (sch == false)
        {
            // Загружаем и устанавливаем курсор
            Texture2D texture = iconImage.sprite.texture;
            Cursor.SetCursor(texture, Vector2.zero, CursorMode.Auto);
            Cell[] cells = GameObject.FindObjectsOfType<Cell>();
            GameManager.selectedCardId = id;
            foreach (Cell cell in cells)
            {
                cell.EnableBuildButton(); // Мы явно вызываем метод у каждой клетки
            }
            sch = true;
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

            // Отключаем кнопки на всех клетках
            Cell[] cells = GameObject.FindObjectsOfType<Cell>();
            foreach (Cell cell in cells)
            {
                cell.DisableBuildButton(); // метод ниже, добавим его в Cell.cs
            }

            sch = false;
        }
    }

}
