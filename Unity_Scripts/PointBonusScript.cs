using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PointBonusScript : MonoBehaviour
{
    TextMeshPro GVal;
    public GameObject myText;
    Animator myAnim;
    // Start is called before the first frame update
    void Start()
    {
        GVal = myText.GetComponent<TextMeshPro>();
        myAnim = myText.GetComponent<Animator>();
        StartCoroutine(MoveUpwards());    
    }
    // Update is called once per frame
    void Update()
    {
    }
    public IEnumerator MoveUpwards()
    {
        yield return new WaitForSeconds(1f);
        Vector3 startPos = this.transform.position;
        Vector3 desPos = this.transform.position + (Vector3.up * 2);
        float timeOfTravel = 1f;
        float currentTime = 0;
        float normalizedValue;
        while (currentTime <= timeOfTravel) { 
            currentTime += Time.deltaTime; 
            normalizedValue = currentTime / timeOfTravel; // we normalize our time 
            this.transform.position = Vector3.Lerp(startPos,desPos, normalizedValue); 
            yield return null;
        }
        StartCoroutine(SelfDestruct());
        yield return null;
    }
    public IEnumerator SelfDestruct()
    {
        myAnim.SetBool("Fade", true);
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
        yield return null;
    }
    public void ChangeFont(int newVal)
    {
        string myStr = "";
        if (newVal < 0)
            myStr += "-";
        else
            myStr += "+"; 
        myStr += Mathf.Abs(newVal).ToString();
        myText.GetComponent<TextMeshPro>().text = myStr;
    }
}
