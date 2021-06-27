using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QBoardTileScript : MonoBehaviour
{
    public Text myText;
    public string myTextString;
    public Image mybackground;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //UpdateMyText("20");
    }
    public void UpdateMyText(string newString)
    {
        myText.text = newString;
    }
    public void UpdateMyBackGround(int count)
    {
        if (count%2 == 0) 
            mybackground.color = new Color32(180,180,180,255);
        else
            mybackground.color = new Color32(160,160,160,255);
    }

}
