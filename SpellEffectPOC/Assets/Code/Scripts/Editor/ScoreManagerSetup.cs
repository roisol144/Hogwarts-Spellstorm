using UnityEngine;
using UnityEditor;

namespace ScoreSystem.Editor
{
    public class ScoreManagerEditorSetup : EditorWindow
    {
        [MenuItem("Scoring System/Setup Score Manager")]
        public static void ShowWindow()
        {
            GetWindow<ScoreManagerEditorSetup>("Score Manager Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Score Manager Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("This tool helps you set up the scoring system for your Harry Potter VR experience.");
            GUILayout.Space(10);

            if (GUILayout.Button("Create Score Manager GameObject"))
            {
                CreateScoreManager();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Find Existing Score Manager"))
            {
                FindExistingScoreManager();
            }

            GUILayout.Space(10);

            GUILayout.Label("Quick Actions:", EditorStyles.boldLabel);

            if (GUILayout.Button("Add ScoreManagerSetup to Scene"))
            {
                AddScoreManagerSetupToScene();
            }

            if (Application.isPlaying)
            {
                GUILayout.Space(10);
                GUILayout.Label("Testing (Play Mode Only):", EditorStyles.boldLabel);

                if (GUILayout.Button("Test Kill Score (+10 points)"))
                {
                    TestKillScore();
                }

                if (GUILayout.Button("Test Custom Score (+50 points)"))
                {
                    TestCustomScore();
                }

                if (GUILayout.Button("Reset Score"))
                {
                    TestResetScore();
                }
            }
            else
            {
                GUILayout.Space(10);
                GUILayout.Label("Enter Play mode to test scoring", EditorStyles.helpBox);
            }

            GUILayout.Space(10);
            GUILayout.Label("Instructions:", EditorStyles.boldLabel);
            GUILayout.Label("1. Click 'Create Score Manager GameObject' to set up the system");
            GUILayout.Label("2. The score will appear in the top right corner during play");
            GUILayout.Label("3. Every enemy kill awards points (configurable in ScoreManager)");
            GUILayout.Label("4. The score UI follows the camera like health bar and spell debug");
        }

        private void CreateScoreManager()
        {
            // Check if one already exists
            ScoreManager existing = FindObjectOfType<ScoreManager>();
            if (existing != null)
            {
                EditorUtility.DisplayDialog("Score Manager Exists", 
                    $"A ScoreManager already exists on GameObject: {existing.gameObject.name}", 
                    "OK");
                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing.gameObject);
                return;
            }

            // Create new ScoreManager
            GameObject scoreManagerGO = new GameObject("ScoreManager");
            ScoreManager scoreManager = scoreManagerGO.AddComponent<ScoreManager>();

            // Configure default settings
            scoreManager.SetPointsPerKill(10);

            // Select the new object
            Selection.activeGameObject = scoreManagerGO;
            EditorGUIUtility.PingObject(scoreManagerGO);

            Debug.Log("[ScoreManager Setup] Created ScoreManager GameObject with default settings");

            EditorUtility.DisplayDialog("Score Manager Created", 
                "ScoreManager has been created and configured!\n\n" +
                "- Points per kill: 10 (configurable in inspector)\n" +
                "- Score UI will appear in top right corner\n" +
                "- UI follows camera like health bar and spell debug", 
                "OK");
        }

        private void FindExistingScoreManager()
        {
            ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
            if (scoreManager != null)
            {
                Selection.activeGameObject = scoreManager.gameObject;
                EditorGUIUtility.PingObject(scoreManager.gameObject);
                Debug.Log("[ScoreManager Setup] Found ScoreManager on GameObject: " + scoreManager.gameObject.name);
                
                EditorUtility.DisplayDialog("Score Manager Found", 
                    $"Found ScoreManager on GameObject: {scoreManager.gameObject.name}\n\n" +
                    $"Current points per kill: {scoreManager.GetPointsPerKill()}", 
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Not Found", 
                    "No ScoreManager found in the current scene.\n\nClick 'Create Score Manager GameObject' to create one.", 
                    "OK");
            }
        }

        private void AddScoreManagerSetupToScene()
        {
            // Check if one already exists
            ScoreManagerSetup existing = FindObjectOfType<ScoreManagerSetup>();
            if (existing != null)
            {
                EditorUtility.DisplayDialog("ScoreManagerSetup Exists", 
                    $"A ScoreManagerSetup already exists on GameObject: {existing.gameObject.name}", 
                    "OK");
                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing.gameObject);
                return;
            }

            // Create new ScoreManagerSetup
            GameObject setupGO = new GameObject("ScoreManagerSetup");
            ScoreManagerSetup setup = setupGO.AddComponent<ScoreManagerSetup>();

            // Select the new object
            Selection.activeGameObject = setupGO;
            EditorGUIUtility.PingObject(setupGO);

            Debug.Log("[ScoreManager Setup] Created ScoreManagerSetup GameObject");

            EditorUtility.DisplayDialog("ScoreManagerSetup Created", 
                "ScoreManagerSetup has been added to the scene!\n\n" +
                "This will automatically create a ScoreManager when the scene starts.\n" +
                "Right-click on the component to access setup and testing functions.", 
                "OK");
        }

        private void TestKillScore()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Play Mode Required", 
                    "This test can only be run in Play mode. Enter Play mode and try again.", 
                    "OK");
                return;
            }

            ScoreManager.NotifyKill();
            Debug.Log("[ScoreManager Setup] Test kill score triggered!");
        }

        private void TestCustomScore()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Play Mode Required", 
                    "This test can only be run in Play mode. Enter Play mode and try again.", 
                    "OK");
                return;
            }

            ScoreManager.NotifyScore(50);
            Debug.Log("[ScoreManager Setup] Test custom score (50 points) triggered!");
        }

        private void TestResetScore()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Play Mode Required", 
                    "This test can only be run in Play mode. Enter Play mode and try again.", 
                    "OK");
                return;
            }

            ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
            if (scoreManager != null)
            {
                scoreManager.ResetScore();
                Debug.Log("[ScoreManager Setup] Score reset!");
            }
            else
            {
                Debug.LogWarning("[ScoreManager Setup] No ScoreManager found to reset");
            }
        }
    }

    // Custom Inspector for ScoreManager
    [CustomEditor(typeof(ScoreManager))]
    public class ScoreManagerInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(10);
            GUILayout.Label("Quick Actions:", EditorStyles.boldLabel);

            ScoreManager scoreManager = (ScoreManager)target;

            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("Current Score:", scoreManager.GetCurrentScore().ToString(), EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Points Per Kill:", scoreManager.GetPointsPerKill().ToString());

                GUILayout.Space(5);

                if (GUILayout.Button("Test Kill Score (+points per kill)"))
                {
                    ScoreManager.NotifyKill();
                }

                if (GUILayout.Button("Test Custom Score (+50 points)"))
                {
                    ScoreManager.NotifyScore(50);
                }

                if (GUILayout.Button("Reset Score"))
                {
                    scoreManager.ResetScore();
                }
            }
            else
            {
                GUILayout.Label("Enter Play mode to test scoring and see current values", EditorStyles.helpBox);
            }

            GUILayout.Space(10);
            
            if (GUILayout.Button("Open Setup Window"))
            {
                ScoreManagerEditorSetup.ShowWindow();
            }
        }
    }

    // Custom Inspector for ScoreManagerSetup
    [CustomEditor(typeof(ScoreManagerSetup))]
    public class ScoreManagerSetupInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(10);
            GUILayout.Label("Quick Actions:", EditorStyles.boldLabel);

            ScoreManagerSetup setup = (ScoreManagerSetup)target;

            if (GUILayout.Button("Setup Score Manager Now"))
            {
                setup.SetupScoreManager();
            }

            if (Application.isPlaying)
            {
                if (GUILayout.Button("Test Kill Score"))
                {
                    setup.TestAddKillScore();
                }

                if (GUILayout.Button("Test Custom Score"))
                {
                    setup.TestAddCustomScore();
                }

                if (GUILayout.Button("Reset Score"))
                {
                    setup.TestResetScore();
                }
            }
            else
            {
                GUILayout.Label("Enter Play mode to test scoring", EditorStyles.helpBox);
            }

            GUILayout.Space(10);
            
            if (GUILayout.Button("Open Setup Window"))
            {
                ScoreManagerEditorSetup.ShowWindow();
            }
        }
    }
} 