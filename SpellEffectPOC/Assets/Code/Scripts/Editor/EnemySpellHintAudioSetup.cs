using UnityEngine;
using UnityEditor;

namespace MagicalUI.Editor
{
    public class EnemySpellHintAudioSetup : EditorWindow
    {
        [MenuItem("Magical UI/Setup Enemy Spell Hint Audio")]
        public static void ShowWindow()
        {
            GetWindow<EnemySpellHintAudioSetup>("Enemy Spell Hint Audio Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Enemy Spell Hint Audio Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("This tool helps you set up the audio hint system for wrong spell usage.");
            GUILayout.Space(10);

            if (GUILayout.Button("Create Enemy Spell Hint Audio GameObject"))
            {
                CreateEnemySpellHintAudio();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Find Existing Enemy Spell Hint Audio"))
            {
                FindExistingEnemySpellHintAudio();
            }

            GUILayout.Space(20);

            GUILayout.Label("Audio Files Setup:", EditorStyles.boldLabel);
            GUILayout.Label("1. Import your audio files anywhere in the project");
            GUILayout.Label("2. Select the Enemy Spell Hint Audio GameObject");
            GUILayout.Label("3. Drag audio files to the Inspector fields:");
            GUILayout.Label("   • To Kill Basilisk → your basilisk audio");
            GUILayout.Label("   • To Kill Dementor → your dementor audio");
            GUILayout.Label("   • To Kill Troll → your troll audio");

            GUILayout.Space(20);

            GUILayout.Label("How it works:", EditorStyles.boldLabel);
            GUILayout.Label("1. Player uses wrong spell on enemy");
            GUILayout.Label("2. Text hint appears: 'Try using [correct spell]...'");
            GUILayout.Label("3. Audio plays: your assigned hint sound");
            GUILayout.Label("4. Example: Wrong spell on Basilisk → your basilisk audio plays");
        }

        private void CreateEnemySpellHintAudio()
        {
            // Check if one already exists
            EnemySpellHintAudio existing = FindObjectOfType<EnemySpellHintAudio>();
            if (existing != null)
            {
                Debug.LogWarning("Enemy Spell Hint Audio already exists in the scene!");
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            // Create the GameObject
            GameObject hintAudioGO = new GameObject("Enemy Spell Hint Audio");
            EnemySpellHintAudio hintAudio = hintAudioGO.AddComponent<EnemySpellHintAudio>();

            // Add AudioSource component
            AudioSource audioSource = hintAudioGO.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D audio
            audioSource.volume = 1f;

            // Select the created object
            Selection.activeGameObject = hintAudioGO;

            Debug.Log("Enemy Spell Hint Audio GameObject created successfully! Please assign your audio clips in the Inspector.");
        }

        private void FindExistingEnemySpellHintAudio()
        {
            EnemySpellHintAudio existing = FindObjectOfType<EnemySpellHintAudio>();
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                Debug.Log("Found existing Enemy Spell Hint Audio: " + existing.gameObject.name);
            }
            else
            {
                Debug.LogWarning("No Enemy Spell Hint Audio found in the current scene!");
            }
        }
    }
}