using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoomer : MonoBehaviour
{
    #region Fields
    // The starting position on the screen when it is pressed/touched
    [SerializeField]
    private Vector3 TouchStart;

    // The limits of how far the camera is allowed to zoom in or out
    [SerializeField]
    private float ZoomMin;
    [SerializeField]
    private float ZoomMax;

    // The speed at which the camera zooms
    [SerializeField]
    private float ZoomSpeed;

    // Boolean indicating whether the program is currently showing a maze on screen or not
    [SerializeField]
    private bool isGenerating;

    // Reference to the touch object when someone pinch zooms
    private Touch TouchZero;
    private Touch TouchOne;

    // Previous positions for the touches during a pinch zoom
    private Vector2 TouchZeroPreviousPosition;
    private Vector2 TouchOnePreviousPosition;

    // The magnitudes of the pinch zoom vectors
    private float PreviousMagnitude;
    private float CurrentMagnitude;

    // The difference between the 2 magnitudes
    private float Difference;
    #endregion

    void Start()
    {
        // Subscribe to game state machine
        GameManager.OnGameStateChanged += SetGeneratingBool;
    }

    void Update()
    {
        // Only use the zoom controls while the maze is being generated or on screen
        if (isGenerating)
        {
            // When the left mouse button or a touch control is pressed, set the starting position of the press/touch
            if (Input.GetMouseButtonDown(0))
                SetTouchStartingPosition();
            // When 2 finger touches are recognized, pinch zoom the screen
            if (Input.touchCount == 2)
                PinchZoom();
            // When the press/touch is being held and dragged, pan the screen
            else if (Input.GetMouseButton(0))
                Pan();
            // Zoom when the scrollwheel is being used
            Zoom(Input.GetAxis("Mouse ScrollWheel"));
        }
    }

    private void Zoom(float increment)
    {
        // Zoom in the camera, clamped on the provided minimum and maximum zoom factors
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - increment, ZoomMin, ZoomMax);
    }

    private void PinchZoom()
    {
        // Get the touch objects
        TouchZero = Input.GetTouch(0);
        TouchOne = Input.GetTouch(1);

        // Calculate the distance between where the touches started when touching the screen and where they are now
        TouchZeroPreviousPosition = TouchZero.position - TouchZero.deltaPosition;
        TouchOnePreviousPosition = TouchOne.position - TouchOne.deltaPosition;

        // Calculate the magnitudes (length of the vector from the origin) of the previous position and the current position
        PreviousMagnitude = (TouchZeroPreviousPosition - TouchOnePreviousPosition).magnitude;
        CurrentMagnitude = (TouchZero.position - TouchOne.position).magnitude;

        // Calculate the difference between these two magnitudes
        Difference = CurrentMagnitude - PreviousMagnitude;

        // Zoom in using the difference in magnitudes multiplied by the zoom speed
        Zoom(Difference * ZoomSpeed);
    }

    private void Pan()
    {
        // Calculate the direction the drag is going and pan the camera accordingly
        Vector3 direction = TouchStart - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Camera.main.transform.position += direction;
    }

    private void SetTouchStartingPosition()
    {
        // Set the start of the press/touch to the location where the press/touch is done
        TouchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void SetGeneratingBool(GameStates previousState, GameStates newState)
    {
        // Keep track of the game state to decide whether the code is currently in the generation state or not
        if (newState == GameStates.MAZE_GENERATION)
            isGenerating = true;
        else if (newState == GameStates.GENERATION_MENU)
            isGenerating = false;
    }
}
