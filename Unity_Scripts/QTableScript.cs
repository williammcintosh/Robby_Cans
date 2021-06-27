using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class QTableScript : MonoBehaviour
{
    public float eta = 0.2f, gamma = 0.9f;
    public string [] stateConfig = new string [243];
    public float [] actionNorth = new float [243], actionSouth = new float [243], actionEast = new float [243], actionWest = new float [243], actionPickup = new float [243];
    public GameObject QBoard;
    public GameObject QBoardTilePrefab;
    int stringIteration = 0;
    // Start is called before the first frame update
    void Start()
    {
        CreateStateConfigString();
        RandomizeQtableValues();
    }

    // Update is called once per frame
    void Update()
    {
    }
    public void CreateStateConfigString()
    {
        char[] set1 = {'0', '1', '2'}; 
        int k = 5; 
        CreateStateConfigString(set1, k); 
    }
    public void CreateStateConfigString(char[] set, int k) 
    { 
        int n = set.Length;  
        CreateStateConfigString(set, "", n, k); 
    } 
    public void CreateStateConfigString(char[] set, string prefix, int n, int k)
    { 
        if (k == 0)  
        { 
            stateConfig[stringIteration] = prefix; 
            stringIteration++;
            return; 
        } 
        for (int i = 0; i < n; ++i) 
        { 
            string newPrefix = prefix + set[i];  
            CreateStateConfigString(set, newPrefix, n, k - 1);  
        } 
    }
    public void UpdateQTable(string previousStateString, string currentStateString, string action, int rewardThisMove)
    {
        int prevIndex, currIndex;
        float bellmans = 0;
        if (previousStateString != null || currentStateString != null) {
            prevIndex = Array.IndexOf(stateConfig, previousStateString);
            currIndex = Array.IndexOf(stateConfig, currentStateString);
            if (prevIndex != -1 && currIndex != -1) {
                switch (action)
                {
                    case "N": 
                        bellmans = Bellmans(actionNorth, rewardThisMove, prevIndex, currIndex);
                        actionNorth[prevIndex] += bellmans;
                        break;
                    case "S": 
                        bellmans = Bellmans(actionSouth, rewardThisMove, prevIndex, currIndex);
                        actionSouth[prevIndex] += bellmans;
                        break;
                    case "E": 
                        bellmans = Bellmans(actionEast, rewardThisMove, prevIndex, currIndex);
                        actionEast[prevIndex] += bellmans;
                        break;
                    case "W": 
                        bellmans = Bellmans(actionWest, rewardThisMove, prevIndex, currIndex);
                        actionWest[prevIndex] += bellmans;
                        break;
                    case "P": 
                        bellmans = Bellmans(actionPickup, rewardThisMove, prevIndex, currIndex);
                        actionPickup[prevIndex] += bellmans;
                        break;
                    default:
                        //Debug.Log("ACTION NOT UNDERSTOOD");
                        break;
                }
                //Debug.Log("prevIndex of "+previousStateString+" = "+prevIndex);
            }
            //Debug.Log("STATE: "+previousStateString+", PREVINDEX: "+prevIndex+", ACTION: "+action+", REWARD: "+rewardThisMove+", BELLMANS: "+bellmans);
        }
    }
    //Gets the best action, and applies that discounted value to the previous state
    public float Bellmans(float [] actionArray, int reward, int prevIndex, int currIndex)
    {
        float prevStateVal = actionArray[prevIndex];
        float maxCurrStateVal = GetMaxCurrentStateVal(currIndex);
        float bellMansVal = eta*(reward + gamma * (maxCurrStateVal - prevStateVal));
        //Debug.Log("BELLMAN'S = "+bellMansVal);
        return bellMansVal;
    }
    //Of all the actions available in the current state configuration,
    // return the action that has the highest value
    public float GetMaxCurrentStateVal(int currIndex)
    {
        float bestAction = Int32.MinValue;
        float [] actionOptions = new float [5];
        actionOptions[0] = actionNorth[currIndex];
        actionOptions[1] = actionSouth[currIndex];
        actionOptions[2] = actionEast[currIndex];
        actionOptions[3] = actionWest[currIndex];
        actionOptions[4] = actionPickup[currIndex];
        for (int i = 0; i < actionOptions.Length; i++) {
            if (actionOptions[i] > bestAction) {
                bestAction = actionOptions[i];
            }
        }
        return bestAction;
    }
    public string GetBestAction(string stateString)
    {
        int currIndex = Array.IndexOf(stateConfig, stateString);
        if (currIndex != -1) {
            float bestActionVal = Int32.MinValue;
            int bestActionIndex = 0;
            float [] actionOptions = new float [5];
            actionOptions[0] = actionNorth[currIndex];
            actionOptions[1] = actionSouth[currIndex];
            actionOptions[2] = actionEast[currIndex];
            actionOptions[3] = actionWest[currIndex];
            actionOptions[4] = actionPickup[currIndex];
            for (int i = 0; i < actionOptions.Length; i++) {
                if (actionOptions[i] > bestActionVal) {
                    bestActionVal = actionOptions[i];
                    bestActionIndex = i;
                }
            }
            switch (bestActionIndex)
            {
                case 0: 
                    return "N";
                case 1: 
                    return "S";
                case 2: 
                    return "E";
                case 3: 
                    return "W";
                case 4: 
                    return "P";
            }         
        }
        return "";
    }
    public void RandomizeQtableValues()
    {
        for (int i = 0; i < actionNorth.Length; i++) {
            actionNorth[i] = UnityEngine.Random.Range(-0.5f, 0.5f);
        }
        for (int i = 0; i < actionSouth.Length; i++) {
            actionSouth[i] = UnityEngine.Random.Range(-0.5f, 0.5f);
        }
        for (int i = 0; i < actionEast.Length; i++) {
            actionEast[i] = UnityEngine.Random.Range(-0.5f, 0.5f);
        }
        for (int i = 0; i < actionWest.Length; i++) {
            actionWest[i] = UnityEngine.Random.Range(-0.5f, 0.5f);
        }
        for (int i = 0; i < actionPickup.Length; i++) {
            actionPickup[i] = UnityEngine.Random.Range(-0.5f, 0.5f);
        }
    }
}
