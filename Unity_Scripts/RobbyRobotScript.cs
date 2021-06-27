using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobbyRobotScript : MonoBehaviour
{
    public bool waving = false;
    public float epsilon = 0.1f;
    public int rewardThisMove;
    public string myAction;
    QTableScript qTableScript;
    public string currentStateString = "", previousStateString = "";
    public GameObject prefabPointBonus;
    GameObject myPointBonus;
    public LayerMask whatIsGround;
    public GameObject groundCheck, sensor;
    public bool currentlyMoving, isLocked, IFell;
    private Rigidbody myRigidbody;
    public bool isGrounded;
    private Animator myAnim;
    public Vector3 lastSpot;
    private BoardManagerScript boardManager;
    float simSpeed;
    private void Start()
    {
        myRigidbody = gameObject.GetComponent<Rigidbody>();
        myAnim = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        if (!waving) {
            simSpeed = boardManager.simSpeed;
            bool wasLocked = isLocked;
            isLocked = IsLocked();
            bool wasGrounded = isGrounded;
            isGrounded = GroundedCheck();
            if (!isGrounded)
                myRigidbody.isKinematic = false;
            //Records the last location
            if (!wasLocked && isLocked && isGrounded) {
                previousStateString = currentStateString;
                ActivateSensors(myAction);
                RecordLastPhysicalPosition();
                rewardThisMove = 0;
                qTableScript.UpdateQTable(previousStateString, currentStateString, myAction, rewardThisMove);
            }
            //Only considers when initially landed
            if (!wasGrounded && isGrounded) {
                myAnim.SetBool("Grounded", true);
                UpdatePreviousState();
            }
            if (!isGrounded)
                myAnim.SetBool("Grounded", false);
            UserTestInput();
            OHPOOPImFalling();
            if (!currentlyMoving) {
                SelectAnAction();
            }
            if (boardManager.allDone) {
                if (!waving && isGrounded) {
                    myAnim.SetBool("WaveForever", true);
                    TurnRobby(135);
                    waving = true;
                }
            }
        }
    }
    public void UserTestInput()
    {
        if (Input.GetKey(KeyCode.Space)) {
            myAnim.SetTrigger("Pickup");
            myAction = "P";
            if (PickUpCan()) {
                rewardThisMove = 10;
                boardManager.rewardThisEpisode += rewardThisMove;
                MakeBonusPoint(10);
            } else {
                rewardThisMove = -1;
                boardManager.rewardThisEpisode += rewardThisMove;
                MakeBonusPoint(-1);
            }
            qTableScript.UpdateQTable(previousStateString, currentStateString, myAction, rewardThisMove);
        }
        if (Input.GetKey(KeyCode.RightArrow) && !currentlyMoving) {
            TurnRobby(90);
            myAction = "E";
            StartCoroutine(MoveAndWait( 1,0));
        }
        if (Input.GetKey(KeyCode.LeftArrow) && !currentlyMoving) {
            TurnRobby(270);
            myAction = "W";
            StartCoroutine(MoveAndWait(-1,0));
        }
        if (Input.GetKey(KeyCode.UpArrow) && !currentlyMoving) {
            TurnRobby(0);
            myAction = "N";
            StartCoroutine(MoveAndWait(0,1));
        }
        if (Input.GetKey(KeyCode.DownArrow) && !currentlyMoving) {
            TurnRobby(180);
            myAction = "S";
            StartCoroutine(MoveAndWait(0,-1));
        }
    }
    public IEnumerator MoveAndWait(int i, int j)
    {
        currentlyMoving = true;
        yield return new WaitForSeconds(0.5f);
        myAnim.SetFloat("MoveSpeed", 0.45f);
        myRigidbody.isKinematic = false;
        Vector3 startPos = this.transform.position;
        Vector3 desPos = new Vector3(Mathf.Round(startPos.x+i), startPos.y, Mathf.Round(startPos.z+j));
        float timeOfTravel = 1f;
        float currentTime = 0;
        float normalizedValue;
        while (currentTime <= timeOfTravel) { 
            currentTime += Time.deltaTime;
            normalizedValue = currentTime / timeOfTravel; // we normalize our time 
            this.transform.position = Vector3.Lerp(startPos,desPos, normalizedValue); 
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        if (isGrounded) {
            this.transform.position = desPos;
            myRigidbody.isKinematic = true;
            currentlyMoving = false;
        }
        myAnim.SetFloat("MoveSpeed", 0.0f);
        TurnRobby(0);
        yield return null;
    }
    public bool IsLocked()
    {
        int lockedOnX = (int) Mathf.Round(this.transform.position.x);
        int lockedOnZ = (int) Mathf.Round(this.transform.position.z);
        Vector3 destVec = new Vector3(lockedOnX, this.transform.position.y, lockedOnZ);
        if (Vector3.Distance(destVec, this.transform.position) <= 0.1f) {
            //this.transform.position = destVec;
            return true;
        }
        return false;
    }
    public void TurnRobby(int deg)
    {
        Vector3 myRot = Vector3.up * deg;
        transform.localRotation = Quaternion.Euler(myRot);
    }
    public bool GroundedCheck()
    {
        Collider[] hitColliders = Physics.OverlapSphere(groundCheck.transform.position, 0.05f, whatIsGround);
        return hitColliders.Length > 0;
    }
    public void ActivateSensors(string dir)
    {
        TurnRobby(0);
        currentStateString = "";
        currentStateString += SenseSurroundings(Vector3.forward);
        currentStateString += SenseSurroundings(Vector3.back);
        currentStateString += SenseSurroundings(Vector3.right);
        currentStateString += SenseSurroundings(Vector3.left);
        currentStateString += SenseSurroundings(Vector3.zero);
    }
    public string SenseSurroundings(Vector3 dir)
    {
        sensor.transform.localPosition = dir;
        Collider[] hitColliders = Physics.OverlapSphere(sensor.transform.position, 0.2f);
        if (hitColliders.Length == 0) {
            return "0"; //Empty Space
        } else {
            for (int i = 0; i < hitColliders.Length; i++) {
                if (hitColliders[i].gameObject.layer == 9) {
                    return "2"; //Found a can
                }
            }
        }
        return "1"; //Found nothing, just floor tile
    }
    public void OHPOOPImFalling()
    {
        if (myRigidbody.velocity.y < -20) {
            this.transform.position = lastSpot;
            myRigidbody.velocity = Vector3.zero;
            IFell = true;
        }
        if (IFell && isGrounded) {
            myRigidbody.isKinematic = true;
            currentlyMoving = false;
            rewardThisMove = -5;
            boardManager.rewardThisEpisode += rewardThisMove;
            qTableScript.UpdateQTable(previousStateString, currentStateString, myAction, rewardThisMove);
            MakeBonusPoint(-5);
            IFell = false;
        }
    }
    public void RecordLastPhysicalPosition()
    {
        lastSpot = new Vector3(this.transform.position.x, 1, this.transform.position.z);
    }
    public void HandShake(BoardManagerScript theBoard, QTableScript theQTableScript)
    {
        boardManager = theBoard;
        qTableScript = theQTableScript;
    }
    public bool PickUpCan()
    {
        Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, 0.2f);
        for (int i = 0; i < hitColliders.Length; i++) {
            if (hitColliders[i].gameObject.layer == 9) {
                boardManager.KickACan(hitColliders[i].gameObject);
                Destroy(hitColliders[i].gameObject);
                return true;
            }
        }
        return false;
    }
    public void MakeBonusPoint(int val)
    {
        Vector3 pos = this.transform.position + (Vector3.up * 2);
        GameObject newBonusPoint = Instantiate(prefabPointBonus, pos, Quaternion.identity) as GameObject;
        PointBonusScript bonusPointScript = newBonusPoint.GetComponent<PointBonusScript>();
        bonusPointScript.ChangeFont(val);
    }
    public void UpdatePreviousState()
    {
        previousStateString = currentStateString;
        ActivateSensors(myAction);
        RecordLastPhysicalPosition(); 
    }
    //Give the Qtable your current state. Get the best action.
    public void SelectAnAction()
    {
        string bestAction = qTableScript.GetBestAction(currentStateString);
        if (EpsilonRandomNess() == true) {
            bestAction = RandomOtherAction(bestAction);
        }
        switch (bestAction) {
            case "N": 
                TurnRobby(0);
                myAction = "N";
                StartCoroutine(MoveAndWait(0,1));
                break;
            case "S": 
                TurnRobby(180);
                myAction = "S";
                StartCoroutine(MoveAndWait(0,-1));
                break;
            case "E": 
                TurnRobby(90);
                myAction = "E";
                StartCoroutine(MoveAndWait( 1,0));
                break;
            case "W": 
                TurnRobby(270);
                myAction = "W";
                StartCoroutine(MoveAndWait(-1,0));
                break;
            case "P": 
                myAnim.SetTrigger("Pickup");
                myAction = "P";
                StartCoroutine(PickUpAndWait());
                break;
            default:
                //Debug.Log("BEST ACTION NOT UNDERSTOOD");
                return;
        }
    }
    public IEnumerator PickUpAndWait()
    {
        currentlyMoving = true;
        yield return new WaitForSeconds(0.5f);
        if (PickUpCan()) {
            rewardThisMove = 10;
            boardManager.rewardThisEpisode += rewardThisMove;
            MakeBonusPoint(10);
        } else {
            rewardThisMove = -1;
            boardManager.rewardThisEpisode += rewardThisMove;
            MakeBonusPoint(-1);
        }
        yield return new WaitForSeconds(0.5f);
        qTableScript.UpdateQTable(previousStateString, currentStateString, myAction, rewardThisMove);
        currentlyMoving = false;
    }
    public bool EpsilonRandomNess()
    {
        float roleTheDie = Random.Range(0f, 1.0f);
        if (roleTheDie <= epsilon) {
            return true;
        }
        return false;
    }
    public string RandomOtherAction(string bestAction)
    {
        string [] actionList = new string [5] {"N", "S", "E", "W", "P"};
        int rollTheDie = (int) Mathf.Round(Random.Range(0, 4));
        //Debug.Log("ACTION SELECTED = "+actionList[rollTheDie]);
        return actionList[rollTheDie];
    }
    public void UpdateEpsilon(float newEpsilon)
    {
        epsilon = newEpsilon;
    }
}
