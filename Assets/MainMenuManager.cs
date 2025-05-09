using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    public Button newGameButton;
    public Button loadGameButton;
    public Button optionsButton;
    public Button quitButton;

    [Header("Options Panel")]
    public GameObject optionsPanel;
    public Slider brightnessSlider;
    public Slider soundSlider;
    public Button closeOptionsButton;

    [Header("References")]
    public string gameSceneName = "GameScene"; // Change this to your actual game scene name

    private void Start()
    {
        // Initialize the options panel to be hidden
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }

        // Check if there's a saved game to enable/disable the Load Game button
        if (loadGameButton != null)
        {
            loadGameButton.interactable = PlayerPrefs.HasKey("SavedGame");
        }

        // Load saved preferences for brightness and sound
        if (brightnessSlider != null)
        {
            brightnessSlider.value = PlayerPrefs.GetFloat("Brightness", 1.0f);
            AdjustBrightness();
        }

        if (soundSlider != null)
        {
            soundSlider.value = PlayerPrefs.GetFloat("SoundVolume", 1.0f);
            AdjustSound();
        }

        // Set up button listeners
        if (newGameButton != null) newGameButton.onClick.AddListener(StartNewGame);
        if (loadGameButton != null) loadGameButton.onClick.AddListener(LoadGame);
        if (optionsButton != null) optionsButton.onClick.AddListener(OpenOptions);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
        if (closeOptionsButton != null) closeOptionsButton.onClick.AddListener(CloseOptions);
    }

    public void StartNewGame()
    {
        // Load the game scene
        SceneManager.LoadScene(gameSceneName);
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("SavedGame"))
        {
            // Load the saved game scene or state
            // You might want to load the same scene but with different parameters
            SceneManager.LoadScene(gameSceneName);
            
            // Additional logic to restore the game state can be added here
        }
        else
        {
            Debug.Log("No saved game found!");
        }
    }

    public void OpenOptions()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
        }
    }

    public void CloseOptions()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void AdjustBrightness()
    {
        if (brightnessSlider != null)
        {
            // Save brightness setting
            PlayerPrefs.SetFloat("Brightness", brightnessSlider.value);
            
            // Apply brightness setting
            // This is a simple example - you might want to use a post-processing effect
            // or adjust lights in your scene instead
            RenderSettings.ambientIntensity = brightnessSlider.value;
        }
    }

    public void AdjustSound()
    {
        if (soundSlider != null)
        {
            // Save sound volume setting
            PlayerPrefs.SetFloat("SoundVolume", soundSlider.value);
            
            // Apply sound setting to all audio sources
            AudioListener.volume = soundSlider.value;
        }
    }
}
