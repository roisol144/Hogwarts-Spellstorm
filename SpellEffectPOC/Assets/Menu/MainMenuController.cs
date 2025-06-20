using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button scoreboardButton;
    [SerializeField] private Button tutorialsButton;
    [SerializeField] private Button exitButton;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem menuParticles;
    [SerializeField] private float buttonHoverScale = 1.1f;
    [SerializeField] private float buttonAnimationDuration = 0.3f;
    [SerializeField] private Color buttonHoverColor = new Color(0.8f, 0.8f, 1f, 1f);

    private void Start()
    {
        InitializeButtons();
        if (menuParticles != null)
        {
            menuParticles.Play();
        }
    }

    private void InitializeButtons()
    {
        // Start Game Button
        startGameButton.onClick.AddListener(() => {
            PlayButtonEffect(startGameButton);
            // Load the YardScene
            SceneManager.LoadScene("YardScene");
        });

        // Scoreboard Button
        scoreboardButton.onClick.AddListener(() => {
            PlayButtonEffect(scoreboardButton);
            // Add your scoreboard scene name here
            SceneManager.LoadScene("ScoreboardScene");
        });

        // Tutorials Button
        tutorialsButton.onClick.AddListener(() => {
            PlayButtonEffect(tutorialsButton);
            // Add your tutorials scene name here
            SceneManager.LoadScene("TutorialsScene");
        });

        // Exit Button
        exitButton.onClick.AddListener(() => {
            PlayButtonEffect(exitButton);
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        });

        // Add hover effects to all buttons
        AddHoverEffects(startGameButton);
        AddHoverEffects(scoreboardButton);
        AddHoverEffects(tutorialsButton);
        AddHoverEffects(exitButton);
    }

    private void AddHoverEffects(Button button)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        // Pointer Enter
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => {
            button.transform.DOScale(buttonHoverScale, buttonAnimationDuration);
            button.GetComponent<Image>().DOColor(buttonHoverColor, buttonAnimationDuration);
        });
        trigger.triggers.Add(enterEntry);

        // Pointer Exit
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => {
            button.transform.DOScale(1f, buttonAnimationDuration);
            button.GetComponent<Image>().DOColor(Color.white, buttonAnimationDuration);
        });
        trigger.triggers.Add(exitEntry);
    }

    private void PlayButtonEffect(Button button)
    {
        // Create a magical effect when button is clicked
        Sequence sequence = DOTween.Sequence();
        sequence.Append(button.transform.DOScale(0.9f, 0.1f));
        sequence.Append(button.transform.DOScale(1.1f, 0.1f));
        sequence.Append(button.transform.DOScale(1f, 0.1f));
    }
} 