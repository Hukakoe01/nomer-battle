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

        // ����������� ��������
        if (iconImage != null)
            iconImage.sprite = icon;
        else
            Debug.LogError("iconImage �� �������� � ����������!");

        if (nameText != null)
            nameText.text = name;
        else
            Debug.LogError("nameText �� �������� � ����������!");

        if (descriptionText != null)
            descriptionText.text = description;
        else
            Debug.LogError("descriptionText �� �������� � ����������!");

        if (priceText != null)
            priceText.text = price + " ������";
        else
            Debug.LogError("priceText �� �������� � ����������!");
    }


    public void OnClick()
    {
        if (sch == false)
        {
            Debug.Log("������� �������� � ID: " + id);  // ��������, ��� ID ����������
            GameManager.selectedCardId = id; // ����������� ��������� ID
            Texture2D texture = iconImage.sprite.texture;
            Cursor.SetCursor(texture, Vector2.zero, CursorMode.Auto);
            Cell[] cells = GameObject.FindObjectsOfType<Cell>();
            foreach (Cell cell in cells)
            {
                cell.EnableBuildButton();
            }
            sch = true;
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            // ��������� ������ �� ���� �������
            Cell[] cells = GameObject.FindObjectsOfType<Cell>();
            foreach (Cell cell in cells)
            {
                cell.DisableBuildButton();
            }
            sch = false;
        }
    }
}
