using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

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
    public string gameSceneName = "1st Map Area"; // Scene to load when New Game is clicked
    
    [Header("Button Animation")]
    public float hoverScaleMultiplier = 1.1f;
    public float clickScaleMultiplier = 0.9f;
    public float animationSpeed = 10f;
    
    // Keep track of original button scales
    private Vector3 newGameOriginalScale;
    private Vector3 loadGameOriginalScale;
    private Vector3 optionsOriginalScale;
    private Vector3 quitOriginalScale;

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
        
        // Store original button scales
        if (newGameButton != null) newGameOriginalScale = newGameButton.transform.localScale;
        if (loadGameButton != null) loadGameOriginalScale = loadGameButton.transform.localScale;
        if (optionsButton != null) optionsOriginalScale = optionsButton.transform.localScale;
        if (quitButton != null) quitOriginalScale = quitButton.transform.localScale;

        // Set up button listeners
        if (newGameButton != null) 
        {
            newGameButton.onClick.AddListener(StartNewGame);
            SetupButtonAnimations(newGameButton);
        }
        
        if (loadGameButton != null) 
        {
            loadGameButton.onClick.AddListener(LoadGame);
            SetupButtonAnimations(loadGameButton);
        }
        
        if (optionsButton != null) 
        {
            optionsButton.onClick.AddListener(OpenOptions);
            SetupButtonAnimations(optionsButton);
        }
        
        if (quitButton != null) 
        {
            quitButton.onClick.AddListener(QuitGame);
            SetupButtonAnimations(quitButton);
        }
        
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
            // Save brightness setting to PlayerPrefs but don't apply it globally
            // This will be read by each scene's lighting controller
            PlayerPrefs.SetFloat("Brightness", brightnessSlider.value);
            
            // Only apply brightness to the current menu scene if needed
            // We're not modifying RenderSettings.ambientIntensity anymore
            // as that affects all scenes
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
    
    // Setup button animations for hover and click
    private void SetupButtonAnimations(Button button)
    {
        // Get or add EventTrigger component
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();
            
        // Add pointer enter event (hover)
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => { OnButtonHover(button); });
        trigger.triggers.Add(enterEntry);
        
        // Add pointer exit event (end hover)
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { OnButtonExit(button); });
        trigger.triggers.Add(exitEntry);
        
        // Add pointer down event (click)
        EventTrigger.Entry downEntry = new EventTrigger.Entry();
        downEntry.eventID = EventTriggerType.PointerDown;
        downEntry.callback.AddListener((data) => { OnButtonClick(button); });
        trigger.triggers.Add(downEntry);
        
        // Add pointer up event (release)
        EventTrigger.Entry upEntry = new EventTrigger.Entry();
        upEntry.eventID = EventTriggerType.PointerUp;
        upEntry.callback.AddListener((data) => { OnButtonRelease(button); });
        trigger.triggers.Add(upEntry);
    }
    
    // Button hover animation
    private void OnButtonHover(Button button)
    {
        Vector3 targetScale = GetOriginalScale(button) * hoverScaleMultiplier;
        StartCoroutine(AnimateButtonScale(button, targetScale));
    }
    
    // Button exit animation
    private void OnButtonExit(Button button)
    {
        Vector3 targetScale = GetOriginalScale(button);
        StartCoroutine(AnimateButtonScale(button, targetScale));
    }
    
    // Button click animation
    private void OnButtonClick(Button button)
    {
        Vector3 targetScale = GetOriginalScale(button) * clickScaleMultiplier;
        StartCoroutine(AnimateButtonScale(button, targetScale));
    }
    
    // Button release animation
    private void OnButtonRelease(Button button)
    {
        Vector3 targetScale = GetOriginalScale(button) * hoverScaleMultiplier; // Return to hover state if still hovering
        StartCoroutine(AnimateButtonScale(button, targetScale));
    }
    
    // Get the original scale for a button
    private Vector3 GetOriginalScale(Button button)
    {
        if (button == newGameButton) return newGameOriginalScale;
        if (button == loadGameButton) return loadGameOriginalScale;
        if (button == optionsButton) return optionsOriginalScale;
        if (button == quitButton) return quitOriginalScale;
        return Vector3.one; // Default fallback
    }
    
    // Animate button scale change
    private System.Collections.IEnumerator AnimateButtonScale(Button button, Vector3 targetScale)
    {
        float time = 0;
        Vector3 startScale = button.transform.localScale;
        
        while (time < 1)
        {
            time += Time.deltaTime * animationSpeed;
            button.transform.localScale = Vector3.Lerp(startScale, targetScale, time);
            yield return null;
        }
        
        button.transform.localScale = targetScale;
    }
}
