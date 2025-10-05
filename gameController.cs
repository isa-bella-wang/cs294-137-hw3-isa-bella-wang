
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems; // Required for TrackableType

using TMPro; // Text Mesh Pro

public class gameController : MonoBehaviour
{
    // for getting the gameboard position
    public PlaceGameBoard gameBoard;
    public Vector3 boardPosition;
    public Vector3 boardUp;
    public float MainMotionSpeed;
    public float MainMotionDistance;

    //for verifying its been placed
    public bool placed;


    [SerializeField] private TMP_Text player1ScoreText;
    [SerializeField] private TMP_Text player2ScoreText;
    [SerializeField] private TMP_Text StatusTxt;

    //Game Over
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button continueButton;

    //Dart Icons
    private int dartsShown = 3;
    [SerializeField] private Image dartIcon1;
    [SerializeField] private Image dartIcon2;
    [SerializeField] private Image dartIcon3;
    [SerializeField] private Sprite dartSprite;


    //Score and Throws
    int P1Score = 0;
    int P2Score = 0;
    bool P1Turn = true;
    enum ThrowNumber { tmax = 3 };
    ThrowNumber throwNumber = 0;


    //Dart Spawn and Aim
    public Vector3 dartOffset;
    public Vector3 hitPosition;
    public Vector3 dartAngle;
    public GameObject dartPre;
    GameObject currentDart;
    Queue<GameObject> darts = new Queue<GameObject>();

    //Delay for throws
    private float turnEndDelay = 0f;
    private const float TURN_END_WAIT = 0.5f;

    public int currScore = 0;
    public ScoringValue scoringValue;

    // Main == start (nothing happens)
    // MainMotion == dart in motion (throw, player clicked on screen)
    public enum Mode { Main, MainMotion, Dart };
    public float transitionTime;
    public Mode mode;

    private Mode current;
    private Mode last;
    private float progressUnsmoothed;
    private float progress;

    //Dart Spawn
    private float DartoffsetBelow = 0.15f;
    private Vector3 dartStartPosition;
    private float dartAnimationProgress = 0f;

    //Status Notifications
    // private string NotePlaceGB = "Place Game Board";
    // private string NoteStart = "Start Player One";





    void Start()
    {
        gameBoard.SetBoardPositionEvent += GetBoardPosition;
        gameBoard.SetBoardUpEvent += GetBoardUp;
        current = mode;
        last = mode;
        progress = 0;
        placed = false;

            // Hide game over panel at start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        // Setup button listeners
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetGame);
        
        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueGame);

    }
    void GetBoardPosition(Vector3 newPosition)
    {
        boardPosition = newPosition;
        placed = true;
        // StatusTxt.text = "Start Player one"; // Announcement
    }
    void GetBoardUp(Vector3 newPosition)
    {
        boardUp = newPosition;
    }


    // The animation function
    void AnimateDartThrow()
    {
        dartAnimationProgress += Time.deltaTime / transitionTime;

        // Animate dart from start to board
        if (dartAnimationProgress <= 1f)
        {
            // Lerp position from start to hit point (straight line)
            currentDart.transform.position = Vector3.Lerp(dartStartPosition, hitPosition, dartAnimationProgress);

            // Rotate dart to point in throw direction
            Vector3 throwDirection = (hitPosition - dartStartPosition).normalized;
            Quaternion baseRotation = Quaternion.LookRotation(throwDirection);

            // Add screw/spin rotation around the forward axis
            float spinSpeed = 2000f; // Degrees per second (2 full rotations)
            float spinAngle = dartAnimationProgress * spinSpeed * transitionTime;
            Quaternion spinRotation = Quaternion.AngleAxis(spinAngle, Vector3.forward);

            // Combine base direction with spin
            currentDart.transform.rotation = baseRotation * spinRotation;
        }
        else
        {
            // Animation finished - switch back to Main mode
            current = Mode.Main;
            dartAnimationProgress = 0f;
        }
        // Animation finished - embed dart in board
        // else if (dartAnimationProgress > 2f)
        // {
        //     EmbedDartInBoard();
        // }
    }

    // Separate function to embed the dart into the board
    // void EmbedDartInBoard()
    // {
    //     float dartLength = 0.08f;
    //     Vector3 toCenter = (boardPosition - hitPosition).normalized;

    //     currentDart.transform.rotation = Quaternion.LookRotation(toCenter, boardUp);
    //     currentDart.transform.position = hitPosition - toCenter * dartLength;

    //     dartAnimationProgress = 0f;
    //     current = Mode.Main;
    // }

    //Hide based on reverse icon value
    private void HideNextDart()
    {
        if (throwNumber == 0) dartIcon3.enabled = false;
        else if (throwNumber == (ThrowNumber)1) dartIcon2.enabled = false;
        else if (throwNumber == (ThrowNumber)2) dartIcon1.enabled = false;
    }
    private void ShowAllDarts()
    {
        if (dartIcon1 != null) dartIcon1.enabled = true;
        if (dartIcon2 != null) dartIcon2.enabled = true;
        if (dartIcon3 != null) dartIcon3.enabled = true;
    }


    //MoveText
    void MoveStatusTxtTo(Vector3 newPosition, Quaternion newRotation)
    {

        //Closer to camera
        Vector3 directionToCamera = (Camera.main.transform.position - newPosition).normalized;
        newPosition += directionToCamera * 0.5f;
        //Move Up
        newPosition += new Vector3(0, 0.3f, 0f);

        // Move the text
        StatusTxt.transform.position = newPosition;

        StartCoroutine(FadeInOut(newPosition));
    }

    private IEnumerator FadeInOut(Vector3 startPos)
    {

        float totalDuration = 1.5f;
        float fadeInTime = 0.2f;
        float fadeOutTime = totalDuration - fadeInTime;
        float moveDistance = 0.15f; // upward slide distance

        Vector3 endPos = startPos + new Vector3(0, moveDistance, 0);
        

        // --- Fade In + Slide Up ---
        for (float t = 0; t < fadeInTime; t += Time.deltaTime)
        {
            float alpha = Mathf.Lerp(0f, 1f, t / fadeInTime);
            StatusTxt.color = new Color(StatusTxt.color.r, StatusTxt.color.g, StatusTxt.color.b, alpha);

            // Move upward during fade-in
            StatusTxt.transform.position = Vector3.Lerp(startPos, endPos, t / totalDuration);
            yield return null;
        }

        // Ensure full opacity
        StatusTxt.color = new Color(StatusTxt.color.r, StatusTxt.color.g, StatusTxt.color.b, 1f);

        // --- Fade Out + Continue Sliding Up ---
        for (float t = 0; t < fadeOutTime; t += Time.deltaTime)
        {
            float alpha = Mathf.Lerp(1f, 0f, t / fadeOutTime);
            StatusTxt.color = new Color(StatusTxt.color.r, StatusTxt.color.g, StatusTxt.color.b, alpha);

            StatusTxt.transform.position = Vector3.Lerp(startPos, endPos, (fadeInTime + t) / totalDuration);
            yield return null;
        }

        // --- Reset to start ---
        StatusTxt.color = new Color(StatusTxt.color.r, StatusTxt.color.g, StatusTxt.color.b, 0f);
        StatusTxt.transform.position = startPos;
    }



    // Update is called once per frame
    void Update()
    {
        // Check if board hasn't been placed yet
        // if (!placed)
        // {
        //     player1ScoreText.text = $"Place Dart Board";
        //    // StatusTxt.text = "Place Dart Board";
        //     return; // Exit early, don't process game logic
        // }

        // if (P1Turn)
        // {
        //     player1ScoreText.text = $"Player One";
        //     //StatusTxt.text = "Player one";
        // }
        // else
        // {
        //     player1ScoreText.text = $"Player Two";
        //     //StatusTxt.text = "Player two";
        // }
        // player1ScoreText.text = $" scored " {currScore};
       //StatusTxt.text += " scored " + currScore;

        // if you click the mouse
        if (Input.GetMouseButtonDown(0))
        {
            // checks to see if you hit the board
            RaycastHit hit;
            // if hits the board
            if (placed && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                // Hide the dart icon immediately when thrown
                HideNextDart();

                throwNumber++;
                current = Mode.MainMotion;
                dartAnimationProgress = 0f;

                //Start position calculated depending on the current location of camera
                Vector3 dartStartPos = Camera.main.transform.position - (Camera.main.transform.up * DartoffsetBelow);
                dartStartPosition = dartStartPos;
                //Calculate Throw angle
                GameObject dart = (GameObject)Instantiate(dartPre, dartStartPos, Quaternion.LookRotation(Vector3.zero));


                // set currentDart to this new dart
                currentDart = dart;
                hitPosition = hit.point;
                // add dart to queue
                darts.Enqueue(dart);

                currScore += decodeScore(hit.point);

                if (P1Turn)
                {
                    P1Score += currScore;
                    player1ScoreText.text = $"{P1Score}";
                    StatusTxt.text = $"{currScore}";
                }
                else
                {
                    P2Score += currScore;
                    player2ScoreText.text = $"{P2Score}";
                    StatusTxt.text = $"{currScore}";

                }

                MoveStatusTxtTo(hit.point, Quaternion.LookRotation(hit.normal));

                Debug.Log($"{throwNumber}");

            }
        }


        // if dart is moving
        if (current == Mode.MainMotion)
        {
            AnimateDartThrow();
        }

        // if dart is moving
        // if (current == Mode.MainMotion)
        // {
        //     // used to smooth the animation
        //     progressUnsmoothed += Time.deltaTime / transitionTime;
        //     // check to see if animation is finished
        //     if (progressUnsmoothed > 2)
        //     {
        //         float dartLength = .3f; 

        //         Vector3 toCenter = (boardPosition - hitPosition).normalized;

        //         // Set rotation to point toward board center
        //         currentDart.transform.rotation = Quaternion.LookRotation(toCenter, boardUp) * Quaternion.Euler(0, 0, 0);
        //         // Position off the center of the board
        //         currentDart.transform.position = hitPosition + toCenter * dartLength;

        //         progressUnsmoothed = 0;

        //         // currentDart.transform.localRotation = Quaternion.Euler(0, 0, 0);
        //         // progressUnsmoothed = 0;

        //         current = Mode.Main;


        //     }

        //     // not finished, update animation
        //     // else
        //     // {
        //     //     progress = smooth(progressUnsmoothed);
        //     //     // i think this is redundant, but for safety, should probably throw an error
        //     //     // if (currentDart != null)
        //     //     // {
        //     //     //     // update psoition
        //     //     //     updatePos(currentDart);
        //     //     // }
        //     // }
        // }


        // if dart is in hand
        else if (current == Mode.Main)
        {
            // checks if all the darts have been thrown
            if (throwNumber >= ThrowNumber.tmax)
            {
                turnEndDelay += Time.deltaTime;

                // animates darts coming back to hand
                if (turnEndDelay >= TURN_END_WAIT)
                {
                    // animates darts coming back to hand
                    while (darts.Count > 0)
                    {
                        GameObject d = darts.Dequeue();
                        d.GetComponentInChildren<Animation>().Play("Drop");
                        StartCoroutine(HideDartAfterAnimation(d, .5f));
                        //Destroy(d, 1f);
                    }

                    //Announce for winner after round
                    if (!P1Turn)
                    {
                        // Both players played one round - Game Over
                        ShowGameOver();
                    }
                    else
                    {
                        // Switch to player 2
                        P1Turn = !P1Turn;
                        currScore = 0;
                        throwNumber = 0;
                        turnEndDelay = 0f;
                        ShowAllDarts();
                    }
                }
            }


        }


    }


    ///GAME OVER SECTION
    private void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Determine winner
        string winner = "";
        if (P1Score > P2Score)
        {
            winner = "Player 1 Wins!";
        }
        else if (P2Score > P1Score)
        {
            winner = "Player 2 Wins!";
        }
        else
        {
            winner = "It's a Tie!";
        }

        if (winnerText != null)
        {
            winnerText.text = $"{winner}\n\nPlayer 1: {P1Score}\nPlayer 2: {P2Score}";
        }
    }

    private void ResetGame()
    {
        // Reset all scores and states
        P1Score = 0;
        P2Score = 0;
        P1Turn = true;
        currScore = 0;
        throwNumber = 0;
        turnEndDelay = 0f;
       // roundsPlayed = 0;

        // Update UI
        player1ScoreText.text = "0";
        player2ScoreText.text = "0";
        ShowAllDarts();

        // Hide game over panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Debug.Log("Game Reset");
    }

    private void ContinueGame()
    {
        // Keep scores, continue for another round
        P1Turn = true;
        currScore = 0;
        throwNumber = 0;
        turnEndDelay = 0f;
        ShowAllDarts();

        // Hide game over panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Debug.Log("Game Continued");
    }



    //Fade Away dart after throwing
    private IEnumerator HideDartAfterAnimation(GameObject dart, float animationLength)
    {
        yield return new WaitForSeconds(animationLength);

        // Scale down over 0.3 seconds
        Vector3 startScale = dart.transform.localScale;
        float scaleTime = .3f;
        float elapsed = 0f;

        while (elapsed < scaleTime)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 0f, elapsed / scaleTime);
            dart.transform.localScale = startScale * scale;
            yield return null;
        }

        dart.SetActive(false);

    }

    float smooth(float t)
    {
        return t;

    }

    //animate
    void updatePos(GameObject currDart)
    {
        // Dictionary<Mode, posRot> positions = new Dictionary<Mode, posRot>();

        posRot startPos = new posRot(
            new Vector3(
                0, 0, -20),
            Quaternion.Euler(90, 0, 0));

        posRot endPos = new posRot(
            new Vector3(
               Mathf.Sin(Time.time * MainMotionSpeed) * MainMotionDistance,
               Mathf.Cos(Time.time * MainMotionSpeed) * MainMotionDistance,
               -20
            ),
            Quaternion.Euler(0, 90, 0));

        Vector3 finalPos;
        Quaternion finalRot;

        finalPos = Vector3.Lerp(startPos.pos, endPos.pos, progress);
        finalRot = Quaternion.Lerp(startPos.rot, endPos.rot, progress);

        transform.position = finalPos;
        transform.rotation = finalRot;

    }


    struct posRot
    {
        public Vector3 pos;
        public Quaternion rot;
        public posRot(Vector3 _pos, Quaternion _rot)
        {
            pos = _pos;
            rot = _rot;
        }
    }

    [System.Serializable]
    public struct ScoringValue
    {
        public Vector3 center;
        public float BE1xRadius;
        public float BE2xRadius;
        public float min3X;
        public float max3X;
        public float min2X;
        public float max2X;
        public ScoringAngles scoringAngles;
    }
    [System.Serializable]
    public struct ScoringAngles
    {

        public float angle5_20;
        public float angle20_1;
        public float angle1_18;
        public float angle18_4;
        public float angle4_13;
        public float angle13_6;
        public float angle6_10;
        public float angle10_15;
        public float angle15_2;
        public float angle2_17;
        public float angle17_3;
        public float angle3_19;
        public float angle19_7;
        public float angle7_16;
        public float angle16_8;
        public float angle8_11;
        public float angle11_14;
        public float angle14_9;
        public float angle9_12;
        public float angle12_5;
    }

    // function for determining score (MAKE IT 2D WITH THE Y AND Z COORDINATES FOR BETTER ACCURACY)
    int decodeScore(Vector3 pos)
    {
        Vector3 offset = pos - boardPosition;
        float angle = Vector3.Angle(boardUp, offset.normalized);
        // Debug.Log("Board Pos: " + boardPosition);
        // Debug.Log("Pos: " + pos);
        Debug.Log("Offset: " + offset.magnitude);
        // Debug.Log("Up: " + boardUp);
        // Debug.Log("Angle is " + angle);

        // BULLSEYE
        if (offset.magnitude < .1f)
        {
            return 50;
        }
        // SECOND RING
        if (offset.magnitude < .2f)
        {
            return 25;
        }

        if (offset.magnitude < .5f)
        {
            // THIRD RING
            if (offset.magnitude > .25f && offset.magnitude < .35f)
            {
                return 10; // multiply by 3
            }
            // OUTER RING
            else if (offset.magnitude > .45f)
            {
                return 15; // multply by 2
            }
            else
            {
                return 1; // multiply by 1
            }
        }

        return 0;


    }


}

