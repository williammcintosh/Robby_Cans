using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ChartAndGraph;
using System.Linq;
using System;

public class BoardManagerScript : MonoBehaviour
{
    public bool training = true, allDone = false;
    int firstTime = 1;
    public GameObject QTable;
    QTableScript qTableScript;
    public GameObject menuManager;
    MenuManagerScript menuManagerScript;
    public GameObject speedSlider, epsilonSlider, episodeCounter, rewardCounter;
    [HideInInspector]
    public float simSpeed;
    public int rewardThisEpisode, totalTrainingReward, totalTrainedReward;
    int episodeCount = 1, totalEpisodes = 5000;
    float [] trainingRewards, trainedRewards;
    public GraphChart trainingRewardchart, trainedModelRewardChart;
    //PREFABS
    public GameObject floorTilePrefab, robbyPrefab, tilesFolder, cansFolder, canPrefab;
    GameObject [,] myFloorTiles, myQBoardTiles;
    public GameObject [] myCans = new GameObject [1], tempMyCans;
    //BOARD SIZES
    [HideInInspector]
    public int floorWidth, floorDepth;
    //WORLD COMPONENTS
    public Camera theCam;
    //ROBBY
    GameObject robby;
    RobbyRobotScript robbyScript;
    bool decreasedEpsilon = false;
    // Start is called before the first frame update
    void Start()
    {
        trainingRewards = new float [totalEpisodes];
        trainedRewards = new float [totalEpisodes];
        menuManagerScript = menuManager.GetComponent<MenuManagerScript>();
        menuManagerScript.UpdateEpisodeText(episodeCount, totalEpisodes);
        qTableScript = QTable.GetComponent<QTableScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!allDone) {
            if (Input.GetKeyDown(KeyCode.R)) {
                ResetSimulator();
            } 
            menuManagerScript.UpdateRewardCountVal(rewardThisEpisode);

            if (robbyScript != null) {
                if (myCans[0] == null && robbyScript.isGrounded && myCans.Length == 1) {
                    ResetSimulator();
                }
                StartCoroutine(DecreaseEpisilon());
            }
        }
    }
    public void Begin()
    {
        speedSlider.SetActive(true);
        epsilonSlider.SetActive(true);
        episodeCounter.SetActive(true);
        rewardCounter.SetActive(true);
        trainingRewardchart.gameObject.SetActive(true);
        trainedModelRewardChart.gameObject.SetActive(true);
        myFloorTiles = new GameObject [floorWidth, floorDepth];
        myQBoardTiles = new GameObject [floorWidth, floorDepth];
        MakeFloor();
        MakeRobby();
        MakeCans();
        PlaceRobby();
        MoveCamera();
        trainingRewardchart.DataSource.ClearCategory("Thingy");
        trainedModelRewardChart.DataSource.ClearCategory("Thingy");
    }
    public void MakeRobby()
    {
        robby = Instantiate(robbyPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        robbyScript = robby.GetComponent<RobbyRobotScript>();
        robbyScript.HandShake(this, qTableScript);
    }
    public void MakeFloor()
    {
        int count = 0;
        for (int i = 0; i < floorWidth; i++) {
            for (int j = 0; j < floorDepth; j++) {
                Vector3 loc = new Vector3(i,-0.1f,j);
                myFloorTiles[i,j] = Instantiate(floorTilePrefab, loc, Quaternion.identity) as GameObject;
                MeshRenderer meshRenderer = myFloorTiles[i,j].GetComponent <MeshRenderer>();
                myFloorTiles[i,j].transform.SetParent(tilesFolder.transform);
                if (count %2 == 0)
                    meshRenderer.materials[0].color = Color.grey;
                else
                    meshRenderer.materials[0].color = Color.black;
                count++;
            }
            if (floorDepth >1) count++;
        }
    }
    public void MakeCans()
    {
        myCans = new GameObject [1];
        for (int i = 0; i < floorWidth; i++) {
            for (int j = 0; j < floorDepth; j++) {
                float rollTheDie = UnityEngine.Random.Range(0f,1f); 
                if (rollTheDie <= 0.5f) {
                    Vector3 loc = new Vector3(i,3,j);
                    if (CanAlreadyOccupied(i, j) == false) {
                        GameObject newCan = Instantiate(canPrefab, loc, Quaternion.identity) as GameObject;
                        newCan.transform.SetParent(cansFolder.transform);
                        newCan.name = "Can at ("+i+","+j+")";
                        AppendToArray(newCan);
                    }
                }
            }
        }
    }
    public void AppendToArray(GameObject newObject)
    {
        tempMyCans = new GameObject [myCans.Length+1-firstTime];
        myCans.CopyTo(tempMyCans, 0);
        if (firstTime >0) firstTime = 0;
        tempMyCans[tempMyCans.Length-1] = newObject;
        myCans = new GameObject[tempMyCans.Length];
        tempMyCans.CopyTo(myCans, 0);       // SOURCE.CopyTo(TARGET, 0);
    }
    public bool CanAlreadyOccupied(int xCord, int zCord)
    {
        for (int i = 0; i < myCans.Length; i++) {
            if (myCans[i] != null) {
                int canXCord = (int) Mathf.Round(myCans[i].transform.position.x);
                int canZCord = (int) Mathf.Round(myCans[i].transform.position.z);
                if (xCord == canXCord && zCord == canZCord) {
                    return true;
                }
            }
        }
        return false;
    }

    public void MoveCamera()
    {
        theCam.transform.position += Vector3.right * (floorWidth+(floorWidth/4));
        theCam.transform.position += Vector3.up * (floorWidth);
        theCam.transform.position += Vector3.back * (floorWidth);
    }
    public void KickACan(GameObject aCan) {
        if (myCans.Length > 1) {
            for (int i = 0; i < myCans.Length; i++) {
                if (myCans[i] == aCan) {
                    RemoveCanAt(i);
                }
            }
        } else {
            ResetSimulator();
        }
    }
    public void RemoveCanAt(int pos)
    {
        myCans[pos] = null;
        for (int i = pos; i < myCans.Length-1; i++) {
            myCans[i] = myCans[i+1];
        }
        myCans = myCans.Take(myCans.Length-1).ToArray();
    }
    public void UpdateBoardWidth(int newWdith)
    {
        floorWidth = newWdith;
    }
    public void UpdateBoardDepth(int newDepth)
    {
        floorDepth = newDepth;
    }
    public void UpdateSimSpeed(float newSpeed)
    {
        Time.timeScale = newSpeed;
        menuManagerScript.UpdateSpeedText(newSpeed);
    }
    public void UpdateEpsion(float newEpsilon)
    {
        menuManagerScript.UpdateEpsilonText(newEpsilon);
        robbyScript.UpdateEpsilon(newEpsilon);
    }
    public void KillAllCans()
    {
        for (int i = 0; i < myCans.Length; i++) {
            GameObject canToDestroy = myCans[i];
            //KickACan(canToDestroy);
            if (canToDestroy != null)
                Destroy(canToDestroy);
        }
    }
    public void ResetSimulator()
    {
        KillAllCans();
        firstTime = 1;
        MakeCans();
        PlaceRobby();
        //Switch from training to trained
        if (episodeCount >= totalEpisodes+1) {
            if (training == true) {
                episodeCount = 1;
                training = false;
            } else {
                allDone = true;
            }
        }
        if (training == true) {
            trainingRewardchart.DataSource.AddPointToCategory("Thingy", episodeCount, rewardThisEpisode);
            trainingRewards[episodeCount] = (float) rewardThisEpisode;
            totalTrainingReward += rewardThisEpisode;
            UpdateTrainingVals();
        } else {
            trainedModelRewardChart.DataSource.AddPointToCategory("Thingy", episodeCount, rewardThisEpisode);
            trainedRewards[episodeCount] = (float) rewardThisEpisode;
            totalTrainedReward += rewardThisEpisode;
            UpdateTrainedVals();
        }
        rewardThisEpisode = 0;
        episodeCount++;
        menuManagerScript.UpdateEpisodeText(episodeCount, totalEpisodes);
    }
    public void PlaceRobby()
    {
        int xCord = UnityEngine.Random.Range(0, floorWidth);
        int zCord = UnityEngine.Random.Range(0, floorDepth);
        robbyScript.transform.position = new Vector3(xCord, 1, zCord);
    }
    public void UpdateEpisodeTotal(int newEpisodeTotal)
    {
        totalEpisodes = newEpisodeTotal;
    }
    public IEnumerator DecreaseEpisilon()
    {
        if (episodeCount % 50 == 0 && robbyScript.epsilon > 0) {
            if (!decreasedEpsilon) {
                float epsilon = robbyScript.epsilon - 0.01f;
                if (epsilon < 0) epsilon = 0;
                robbyScript.epsilon = epsilon;
                UpdateEpsion(epsilon);
                menuManagerScript.UpdateEpsilonSlider(epsilon);
                menuManagerScript.UpdateEpsilonText(epsilon);
                decreasedEpsilon = true;
            }
        } else {
            decreasedEpsilon = false;
        }
        yield return new WaitForSeconds(100);
    }
    public void UpdateTrainingVals()
    {
        float rewardAvg = GetAverage(trainingRewards);
        menuManagerScript.UpdateTrainingAvgRewardVal(rewardAvg);
        float stdDev = GetStandardDeviation(trainingRewards, rewardAvg);
        menuManagerScript.UpdateTrainingStdDeviationVal(stdDev);
    }
    public void UpdateTrainedVals()
    {
        float rewardAvg = GetAverage(trainedRewards);
        menuManagerScript.UpdateTrainedAvgRewardVal(rewardAvg);
        float stdDev = GetStandardDeviation(trainedRewards, rewardAvg);
        menuManagerScript.UpdateTrainedStdDeviationVal(stdDev);
    }
    public float GetAverage(float [] list)
    {
        float total = 0;
        for (int i = 0; i < list.Length; i++) {
            total += list[i];
        }
        return total / list.Length;
    }
    public float GetStandardDeviation(float [] list, float avg)
    {
        float stdDev = 0f;
        float [] squareDiff = new float [list.Length];
        for (int i = 0; i < list.Length; i++) {
            float reward = list[i];
            float sqrDif = (float) Math.Pow(reward - avg, 2);
            squareDiff[i] = sqrDif;
        }
        float stdDiffAvg = GetAverage(squareDiff);
        stdDev = (float) Math.Sqrt(stdDiffAvg);
        return stdDev;
    }

}