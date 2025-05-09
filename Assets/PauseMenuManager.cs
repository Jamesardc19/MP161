using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Pause Menu")]
    public GameObject pauseMenuPanel;
    public Button mainMenuButton;
    public Button loadGameButton;
    public Button resumeButton;

    [Header("References")]
    public string mainMenuSceneName = "MainMenuScene"; // Change this to your actual main menu scene name

    private bool isPaused = false;

    private void Start()
    {
        // Initialize pause menu to be hidden
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Set up button listeners
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
        if (loadGameButton != null) loadGameButton.onClick.AddListener(LoadGame);
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);

        // Check if there's a saved game to enable/disable the Load Game button
        if (loadGameButton != null)
        {
            loadGameButton.interactable = PlayerPrefs.HasKey("SavedGame");
        }
    }

    private void Update()
    {
        // Toggle pause menu when Escape key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        isPaused = !isPaused;
        
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(isPaused);
        }
        
        // Pause or resume game time
        Time.timeScale = isPaused ? 0f : 1f;

        // Update load game button interactability
        if (loadGameButton != null)
        {
            loadGameButton.interactable = PlayerPrefs.HasKey("SavedGame");
        }
    }

    public void ResumeGame()
    {
        // Simply unpause the game
        if (isPaused)
        {
            TogglePauseMenu();
        }
    }

    public void GoToMainMenu()
    {
        // Before loading the main menu, make sure to reset the time scale
        Time.timeScale = 1f;
        
        // Optionally save the game state here before returning to main menu
        
        // Load the main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("SavedGame"))
        {
            // Reset time scale before loading
            Time.timeScale = 1f;
            
            // Load the saved game
            // You might want to load the same scene but with different parameters
            // or load a specific saved game scene
            string savedScene = PlayerPrefs.GetString("SavedGameScene", SceneManager.GetActiveScene().name);
            SceneManager.LoadScene(savedScene);
            
            // Additional logic to restore the game state can be added here
        }
        else
        {
            Debug.Log("No saved game available.");
        }
    }

    // Optional: Method to save the current game state
    public void SaveGameState()
    {
        // Save the current scene name
        PlayerPrefs.SetString("SavedGameScene", SceneManager.GetActiveScene().name);
        
        // Set a flag indicating that a saved game exists
        PlayerPrefs.SetInt("SavedGame", 1);
        
        // Additional logic to save player position, inventory, etc. can be added here
        
        // Save all PlayerPrefs changes to disk
        PlayerPrefs.Save();
        
        Debug.Log("Game state saved.");
    }
}
