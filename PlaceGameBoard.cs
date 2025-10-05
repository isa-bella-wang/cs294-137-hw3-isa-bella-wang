
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems; // Required for TrackableType


public class PlaceGameBoard : MonoBehaviour
{
    public GameObject gameBoard;

    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    public bool placed = false;
    public delegate void SetBoardPosition(Vector3 newPosition);
    public event SetBoardPosition SetBoardPositionEvent;
    public delegate void SetBoardUp(Vector3 newPosition);
    public event SetBoardUp SetBoardUpEvent;

    private Camera arCamera;


    void Start()
    {
        // arCamera = GetComponent<ARSessionOrigin>().camera; //TO_ADD

        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();

        // Set detection mode to Vertical
        // planeManager.requestedDetectionMode = PlaneDetectionMode.Vertical;

          // If AR Camera not assigned, try to find it
        // if (arCamera == null)
        // {
        //     arCamera = Camera.main;
        // }
    }

    void Update()
    {
        if (!placed)
        {
            // Use touch input if available, otherwise use mouse input (for XR simulation on laptop)
            if (IsTouchOrMousePressed(out Vector2 touchPosition))
            {
                // Perform the AR Raycast
                List<ARRaycastHit> hits = new List<ARRaycastHit>();
                if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
                {

                    Pose hitPose = hits[0].pose;

                    ARPlane hitPlane = planeManager.GetPlane(hits[0].trackableId);
                    if (hitPlane == null)
                    {
                        Debug.LogWarning("Could not find plane");
                        return;
                    }

                    // Get the plane's normal (up direction)
                    Vector3 planeNormal = hitPlane.normal;
                    gameBoard.transform.rotation = Quaternion.FromToRotation(Vector3.up, planeNormal);

                    // Place and activate the game board
                    gameBoard.SetActive(true);
                    gameBoard.transform.position = hitPose.position;

                    //var hitPosition = hits[0].pose.position; //TO_ADD

                    // Calculate direction from board to camera (projected on plane)
                    // Vector3 toCamera = arCamera.transform.position - hitPose.position;

                    // // Project the camera direction onto the plane to keep board flat
                    // Vector3 projectedToCamera = Vector3.ProjectOnPlane(toCamera, planeNormal).normalized;

                    // // Create rotation: board faces camera while staying flat on surface
                    // // The board's forward (face) points toward camera
                    // // The board's up aligns with plane normal
                    // if (projectedToCamera != Vector3.zero)
                    // {
                    //     gameBoard.transform.rotation = Quaternion.LookRotation(projectedToCamera, planeNormal);
                    // }
                    // else
                    // {
                    //     // Fallback: if camera is directly above, just align with plane
                    //     gameBoard.transform.rotation = Quaternion.FromToRotation(Vector3.up, planeNormal);
                    // }

                    placed = true;

                    // Send the board center coordinates and up vector to gameController.cs
                    if (SetBoardPositionEvent != null)
                    {
                        Vector3 adjustedPosition = hitPose.position + gameBoard.transform.forward * 0.01f;
                        SetBoardPositionEvent(adjustedPosition);
                    }
                    else
                        Debug.LogError("SetBoardPositionEvent is NULL");

                    if (SetBoardUpEvent != null)
                    {
                        // Send the up direction relative to board orientation
                        SetBoardUpEvent(gameBoard.transform.up);
                    }
                    else
                        Debug.LogError("SetBoardUpEvent is NULL");

                    // Disable further plane detection
                    planeManager.requestedDetectionMode = PlaneDetectionMode.None;
                    DisableAllPlanes();




                    // Place and activate the game board
                    // gameBoard.SetActive(true);
                    // gameBoard.transform.position = hitPose.position;
                    // placed = true;



                    // // send the board center coordinates and up vector to gameController.cs
                    // if (SetBoardPositionEvent != null)
                    // {
                    //     Vector3 adjustedPosition = hitPose.position + gameBoard.transform.up * 0.25f;
                    //     SetBoardPositionEvent(adjustedPosition);
                    // }
                    // else
                    //     Debug.LogError("WHY IS IT NULL");
                    // if (SetBoardUpEvent != null)
                    //     SetBoardUpEvent(gameBoard.transform.right);
                    // else
                    //     Debug.LogError("WHY IS IT NULL");


                    // // Disable further plane detection
                    // planeManager.requestedDetectionMode = PlaneDetectionMode.None;
                    // DisableAllPlanes();
                }
            }
        }
    }


    // Check if touch or mouse is pressed and return the position
    private bool IsTouchOrMousePressed(out Vector2 touchPosition)
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // Use touch input
            touchPosition = Input.GetTouch(0).position;
            return true;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            // Use mouse input
            touchPosition = Input.mousePosition;
            return true;
        }

        touchPosition = default;
        return false;
    }

    private void DisableAllPlanes()
    {
        foreach (var plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(false);
        }
    }

    // public void AllowMoveGameBoard()
    // {
    //     placed = false;
    //     gameBoard.SetActive(false);
    //     planeManager.requestedDetectionMode = PlaneDetectionMode.Vertical;
    //     EnableAllPlanes();
    // }

    // private void EnableAllPlanes()
    // {
    //     foreach (var plane in planeManager.trackables)
    //     {
    //         plane.gameObject.SetActive(true);
    //     }
    // }

    public bool Placed()
    {
        return placed;
    }
}