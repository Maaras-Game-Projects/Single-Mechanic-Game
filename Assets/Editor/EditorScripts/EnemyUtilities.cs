using UnityEditor;
using UnityEngine;

namespace EternalKeep
{
    public class EnemyUtilities : EditorWindow
    {
        bool showEnemyActivation = false;
        [SerializeField] EnemyDataContainer allEnemies;

        [MenuItem("Tools/EnemyUtilities")]
        public static void ShowWindow()
        {
            EnemyUtilities window = GetWindow<EnemyUtilities>("Enemy Utilities");
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.Label("Enemy Utilities", EditorStyles.label);
            GUILayout.Space(10);

            showEnemyActivation = EditorGUILayout.Foldout(showEnemyActivation, "Enemy Activation");

            if (showEnemyActivation)
            {
                if (allEnemies == null)
                {
                    allEnemies = FindAnyObjectByType<EnemyDataContainer>();
                }

                GUILayout.BeginHorizontal();
                GUILayout.Space(10);
                if (GUILayout.Button("Disable All Enemies"))
                {
                    foreach (GameObject enemy in allEnemies.GetEnemies)
                    {
                        enemy.gameObject.SetActive(false);
                    }
                }

                if (GUILayout.Button("Enable All Enemies"))
                {
                    foreach (GameObject enemy in allEnemies.GetEnemies)
                    {
                        enemy.gameObject.SetActive(true);
                    }
                }
                GUILayout.EndHorizontal();
            }

            // allEnemies = (EnemyDataContainer)EditorGUILayout.ObjectField("Enemy Data Container",
                //     allEnemies, typeof(EnemyDataContainer), true);

                //EditorGUILayout.DropdownButton()

                
            
            
        }
    }


}

