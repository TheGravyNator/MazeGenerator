using System;
using UnityEngine;
using UnityEngine.UI;

public class GenerationUIHandler : MonoBehaviour
{
    #region Fields
    // Reference to the GenerationControls UI object
    [SerializeField]
    private GameObject GenerationControls;

    // Reference to the AdvancedOptions UI object
    [SerializeField]
    private GameObject AdvancedOptions;

    // Input fields where the dimensions are entered by the users
    [SerializeField]
    private InputField Width;
    [SerializeField]
    private InputField Height;

    // Input fields of advanced options
    [SerializeField]
    private InputField Steps;
    [SerializeField]
    private InputField Delay;

    // Buttons that either generate the maze instantly or in a step progression
    [SerializeField]
    private Button GenerateInstant;
    [SerializeField]
    private Button GenerateStep;

    // A function to check whether the given user input is set
    private Func<string, string, bool, bool> checkIfInputIsSet;

    // A function to check whether the given user input is valid
    private Func<string, string, bool> checkIfInputIsValid;

    #endregion

    #region Delegates and Events
    // Delegate and event for when the maze is being generated
    public delegate void GenerateMaze(MazeGenerationData data);
    public static event GenerateMaze OnGenerateMaze;
    #endregion

    void Start()
    {
        // Subscribe to the game state machine from the GameManager
        GameManager.OnGameStateChanged += OpenGenerationMenuControls;

        // Set the buttons to non-interactable as long as there is no input
        GenerateInstant.interactable = false;
        GenerateStep.interactable = false;

        // Function to check whether input values are set, with the option to specify whether the check is inclusive or exclusive
        checkIfInputIsSet = (string first, string second, bool inclusive) =>
        {
            if (inclusive)
            {
                // If there is valid input in the strings, return true
                if (!string.IsNullOrEmpty(first) && !string.IsNullOrEmpty(second))
                    return true;
                // Else, return false
                else
                    return false;
            }
            else
            {
                // If there is valid input in the strings, return true
                if (!string.IsNullOrEmpty(first) || !string.IsNullOrEmpty(second))
                    return true;
                // Else, return false
                else
                    return false;
            }
            
        };

        // Function to check whether the input values are not negative or 0
        checkIfInputIsValid = (string width, string height) =>
        {
            // If the strings are not negative or 0, return true
            if ((int.Parse(width) > 0 && int.Parse(height) > 0))
                return true;
            // Else, return false
            else
                return false;
        };
    }

    void Update()
    {
        // Toggle the interactability of the input buttons 
        ToggleGenerateButtonInteractability();
    }

    private void ToggleGenerateButtonInteractability()
    {
        // Check whether input is set and the values are valid (not negative or 0)
        if (checkIfInputIsSet(Width.text, Height.text, true) && checkIfInputIsValid(Width.text, Height.text))
        {
            // Check if any advanced options have been set. If so, keep the GenerateInstant button disabled. Otherwise, activate it.
            if (checkIfInputIsSet(Steps.text, Delay.text, false))
                GenerateInstant.interactable = false;
            else
                GenerateInstant.interactable = true;
            GenerateStep.interactable = true;
        }
        // Else, keep the buttons uninteractable
        else
        {
            GenerateInstant.interactable = false;
            GenerateStep.interactable = false;
        }
    }

    public void OnGenerateMazeButton(bool isInstant)
    {
        // Turn off the UI
        GenerationControls.gameObject.SetActive(false);

        // Set the game state to Maze Generation
        GameManager.Instance.ChangeGameState(GameStates.MAZE_GENERATION);

        // Set all the standard maze generation data
        MazeGenerationData data = new MazeGenerationData()
        {
            Width = int.Parse(Width.text),
            Height = int.Parse(Height.text),
            isInstant = isInstant
        };

        // If inputted, set the advanced options
        if (Steps.text != "")
            data.Steps = int.Parse(Steps.text);
        if (Delay.text != "")
            data.Delay = float.Parse(Delay.text);

        // Fire off the event to generate a maze, with the given maze generation data
        OnGenerateMaze(data);
    }

    public void OnAdvancedOptions()
    {
        // Toggle the advanced options on or off on the button press
        if(AdvancedOptions.activeSelf)
            AdvancedOptions.SetActive(false);
        else
            AdvancedOptions.SetActive(true);
    }

    private void OpenGenerationMenuControls(GameStates currentGameState, GameStates newGameState)
    {
        // Set the Generation Menu UI on or off depending on the game state
        if (newGameState == GameStates.GENERATION_MENU)
        {
            GenerationControls.gameObject.SetActive(true);
        }
        else if (newGameState == GameStates.MAZE_GENERATION)
        {
            GenerationControls.gameObject.SetActive(false);
        }
    }
}

public class MazeGenerationData
{
    // Dimensions of the maze
    public int Width { get; set; }
    public int Height { get; set; }

    // Boolean indicating if the maze needs to be generated instantly or step-wise
    public bool isInstant { get; set; }

    // Amount of steps algorithm is allowed to take before rendering out the cells
    public int? Steps { get; set; } 

    // Delay between renders
    public float? Delay { get; set; }
}
