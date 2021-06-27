using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using Random=UnityEngine.Random;
using TMPro;

public class MenuManagerScript : MonoBehaviour
{
    public TextMeshProUGUI sliderSpeedVal, sliderEpsilonVal, episodeCount, rewardCountVal;
    public TextMeshProUGUI trainingAvgReward, trainingStdDeviation, trainedAvgReward, trainedStdDeviation;
    public Slider epsilonSlider;
    public GameObject boardManager;
    BoardManagerScript boardScript;
    public Button startButton;
    public GameObject menuPanel;
    // Start is called before the first frame update
    void Start()
    {
        boardScript = boardManager.GetComponent<BoardManagerScript>();
        menuPanel.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartButtonPressed()
    {
        menuPanel.SetActive(false);
        boardScript.Begin();
    }
    public void UpdateBoardWidth(string newWidth)
    {
        int floorWidth = 0;
        if(int.TryParse(newWidth, out floorWidth)) {
            boardScript.UpdateBoardWidth(floorWidth);
            startButton.interactable = true;
        } else {
            startButton.interactable = false;
        }
    }
    public void UpdateBoardDepth(string newDepth)
    {
        int floorDepth = 0;
        if(int.TryParse(newDepth, out floorDepth)) {
            boardScript.UpdateBoardDepth(floorDepth);
            startButton.interactable = true;
        } else {
            startButton.interactable = false;
        }
    }
    public void UpdateEpisodeCount(string newEpisodeVal)
    {
        int episodeVal = 0;
        if(int.TryParse(newEpisodeVal, out episodeVal)) {
            boardScript.UpdateEpisodeTotal(episodeVal);
            UpdateEpisodeText(0, episodeVal);
            startButton.interactable = true;
        } else {
            startButton.interactable = false;
        }
    }
    public void UpdateSpeedText(float newspeed)
    {
        sliderSpeedVal.text = "x"+newspeed.ToString();
    }
    public void UpdateEpsilonText(float newEpsilon)
    {
        sliderEpsilonVal.text = "ε"+newEpsilon.ToString();
    }
    public void UpdateEpisodeText(int newEpisodeText, int maxEpisodeText)
    {
        episodeCount.text = newEpisodeText.ToString()+"/"+maxEpisodeText.ToString();
    }
    public void UpdateRewardCountVal(float newRewardVal)
    {
        rewardCountVal.text = newRewardVal.ToString();
    }
    public void UpdateEpsilonSlider(float newVal)
    {
        epsilonSlider.value = newVal;
    }
    public void UpdateTrainingAvgRewardVal(float newVal)
    {
        trainingAvgReward.text = newVal.ToString();
    }
    public void UpdateTrainingStdDeviationVal(float newVal)
    {
        trainingStdDeviation.text = newVal.ToString();
    }
    public void UpdateTrainedAvgRewardVal(float newVal)
    {
        trainedAvgReward.text = newVal.ToString();
    }
    public void UpdateTrainedStdDeviationVal(float newVal)
    {
        trainedStdDeviation.text = newVal.ToString();
    }
}
