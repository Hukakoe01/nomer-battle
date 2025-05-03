using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class enemy : MonoBehaviour
{
    public GameObject hp_barr;
    public TMP_Text hp_text;
    int hp;
    

    // Start is called before the first frame update

    void Start()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        hp = int.Parse(hp_text.text);
    }
    public void zombien()
    {
        
        if (hp == -1)
        {
            hp_text.text = "100";
        }
    }

}
