using System.IO;
using UnityEditor;
using UnityEngine;

namespace EternalKeep
{
    public class ResetSaveMenu : EditorWindow
    {
        [MenuItem("Tools/Save System/Reset SaveData")]
        public static void ShowWindow()
        {
            // Create a new window instance
            ResetSaveMenu window = GetWindow<ResetSaveMenu>("Reset Save");
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Reset Save Data", EditorStyles.boldLabel);

            if (GUILayout.Button("Reset"))
            {
                if (!File.Exists(SaveSystem.GetSaveFilePath()))
                {
                    // Display an error message if no save file exists
                    EditorUtility.DisplayDialog("Error", "No save file found to reset.", "OK");
                    return;
                }
                SaveSystem.ResetSave();
            }

            if (GUILayout.Button("Delete Save File"))
            {
                if (!File.Exists(SaveSystem.GetSaveFilePath()))
                {
                    // Display an error message if no save file exists
                    EditorUtility.DisplayDialog("Error", "No save file found to reset.", "OK");
                    return;
                }
                SaveSystem.DeleteSaveFile();
                
            }
        }
    }



}


