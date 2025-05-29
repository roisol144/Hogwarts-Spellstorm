using UnityEngine;
using UnityEditor;

namespace MagicalUI.Editor
{
    public class MagicalDebugUISetup : EditorWindow
    {
        [MenuItem("Magical UI/Setup Debug UI")]
        public static void ShowWindow()
        {
            GetWindow<MagicalDebugUISetup>("Magical Debug UI Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Magical Debug UI Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("This tool helps you set up the magical debug UI system for your Harry Potter VR experience.");
            GUILayout.Space(10);

            if (GUILayout.Button("Create Magical Debug UI GameObject"))
            {
                CreateMagicalDebugUI();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Find Existing Magical Debug UI"))
            {
                FindExistingMagicalDebugUI();
            }

            GUILayout.Space(10);

            GUILayout.Label("Font Setup:", EditorStyles.boldLabel);
            GUILayout.Label("1. Place Harry Potter font (.ttf) in Assets/Resources/Fonts/");
            GUILayout.Label("2. Create TextMeshPro Font Asset from the font");
            GUILayout.Label("3. Name it 'HarryPotter', 'MagicalFont', or 'WizardFont'");

            GUILayout.Space(10);

            if (GUILayout.Button("Open Resources/Fonts Folder"))
            {
                OpenResourcesFontsFolder();
            }

            GUILayout.Space(10);

            GUILayout.Label("Quick Actions:", EditorStyles.boldLabel);

            if (GUILayout.Button("Select SpellCastingManager"))
            {
                SelectSpellCastingManager();
            }

            if (GUILayout.Button("Test Spell Cast (Play Mode Only)"))
            {
                TestSpellCast();
            }
        }

        private void CreateMagicalDebugUI()
        {
            // Check if one already exists
            MagicalDebugUI existing = FindObjectOfType<MagicalDebugUI>();
            if (existing != null)
            {
                EditorUtility.DisplayDialog("Already Exists", 
                    "A MagicalDebugUI component already exists in the scene. Use 'Find Existing' to locate it.", 
                    "OK");
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            // Create new GameObject
            GameObject magicalUI = new GameObject("MagicalDebugUI");
            magicalUI.AddComponent<MagicalDebugUI>();

            // Position it appropriately in the scene
            magicalUI.transform.position = Vector3.zero;

            // Select it in the hierarchy
            Selection.activeGameObject = magicalUI;

            // Mark scene as dirty
            EditorUtility.SetDirty(magicalUI);

            Debug.Log("[MagicalDebugUI Setup] Created new MagicalDebugUI GameObject. The component will auto-configure when you enter Play mode.");
            
            EditorUtility.DisplayDialog("Success", 
                "MagicalDebugUI GameObject created successfully!\n\n" +
                "The component will automatically find your XR Rig and Camera when you enter Play mode.\n\n" +
                "Check the Inspector to adjust positioning if needed.", 
                "OK");
        }

        private void FindExistingMagicalDebugUI()
        {
            MagicalDebugUI existing = FindObjectOfType<MagicalDebugUI>();
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing.gameObject);
                Debug.Log("[MagicalDebugUI Setup] Found existing MagicalDebugUI on GameObject: " + existing.gameObject.name);
            }
            else
            {
                EditorUtility.DisplayDialog("Not Found", 
                    "No MagicalDebugUI component found in the current scene. Use 'Create Magical Debug UI GameObject' to create one.", 
                    "OK");
            }
        }

        private void OpenResourcesFontsFolder()
        {
            string path = "Assets/Resources/Fonts";
            
            // Create the folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder(path))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
                AssetDatabase.CreateFolder("Assets/Resources", "Fonts");
                AssetDatabase.Refresh();
                Debug.Log("[MagicalDebugUI Setup] Created Resources/Fonts folder");
            }

            // Select the folder in the Project window
            Object folderObject = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
            if (folderObject != null)
            {
                Selection.activeObject = folderObject;
                EditorGUIUtility.PingObject(folderObject);
            }
        }

        private void SelectSpellCastingManager()
        {
            SpellCastingManager scm = FindObjectOfType<SpellCastingManager>();
            if (scm != null)
            {
                Selection.activeGameObject = scm.gameObject;
                EditorGUIUtility.PingObject(scm.gameObject);
                Debug.Log("[MagicalDebugUI Setup] Found SpellCastingManager on GameObject: " + scm.gameObject.name);
            }
            else
            {
                EditorUtility.DisplayDialog("Not Found", 
                    "No SpellCastingManager found in the current scene.", 
                    "OK");
            }
        }

        private void TestSpellCast()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Play Mode Required", 
                    "This test can only be run in Play mode. Enter Play mode and try again.", 
                    "OK");
                return;
            }

            MagicalDebugUI.NotifySpellCast("Expelliarmus (Test)");
            Debug.Log("[MagicalDebugUI Setup] Test spell cast triggered!");
        }
    }

    // Custom Inspector for MagicalDebugUI
    [CustomEditor(typeof(MagicalDebugUI))]
    public class MagicalDebugUIInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(10);
            GUILayout.Label("Quick Actions:", EditorStyles.boldLabel);

            MagicalDebugUI magicalUI = (MagicalDebugUI)target;

            if (Application.isPlaying)
            {
                if (GUILayout.Button("Test Spell: Expelliarmus"))
                {
                    MagicalDebugUI.NotifySpellCast("Expelliarmus");
                }

                if (GUILayout.Button("Test Spell: Avada Kedavra"))
                {
                    MagicalDebugUI.NotifySpellCast("Avada Kedavra");
                }

                if (GUILayout.Button("Test Spell: Expecto Patronum"))
                {
                    MagicalDebugUI.NotifySpellCast("Expecto Patronum");
                }
            }
            else
            {
                GUILayout.Label("Enter Play mode to test spell notifications", EditorStyles.helpBox);
            }

            GUILayout.Space(10);
            
            if (GUILayout.Button("Open Setup Window"))
            {
                MagicalDebugUISetup.ShowWindow();
            }
        }
    }
} 