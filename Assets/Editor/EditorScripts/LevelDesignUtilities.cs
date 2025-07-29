using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EternalKeep
{
    public class LevelDesignUtilities : EditorWindow
    {
        bool showMaterialReplace = false;

        Material materialToFind;
        Material materialToReplace;

        [MenuItem("Tools/Level Design")]
        public static void ShowWindow()
        {
            LevelDesignUtilities window = GetWindow<LevelDesignUtilities>("Level Design Tools");
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.Label("Level Design Tools");
            GUILayout.Space(10);

            showMaterialReplace = EditorGUILayout.Foldout(showMaterialReplace, "Material Find-Replace");

            if (showMaterialReplace)
            {
                materialToFind = (Material)EditorGUILayout.ObjectField("Material To Find", materialToFind,
                    typeof(Material), true);

                if (materialToFind == null)
                {
                    EditorGUILayout.HelpBox("Material to Find Not Assigned", MessageType.Warning);
                }

                materialToReplace = (Material)EditorGUILayout.ObjectField("Material To Replace With", materialToReplace,
                    typeof(Material), true);


                if (materialToReplace == null)
                {
                    EditorGUILayout.HelpBox("Material to Replace With Not Assigned", MessageType.Warning);
                }

                GUILayout.Space(10);
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Select Objects"))
                {
                    HandleMaterialSelection();
                }

                if (GUILayout.Button("Replace Material"))
                {
                    HandleMaterialReplace();

                }

                GUILayout.Space(10);
                GUILayout.EndHorizontal();

                // if (GUILayout.Button("Select Objects"))
                // {
                //     Selection.objects = gameObjectsWithGivenMaterial.ToArray();
                // }

            }

            


        }

        private void HandleMaterialReplace()
        {
            List<GameObject> gameObjectsWithGivenMaterial = new List<GameObject>();
            Renderer[] rendererGameObjects = Resources.FindObjectsOfTypeAll<Renderer>();
            gameObjectsWithGivenMaterial = GetMGameObjectsWithMatList(rendererGameObjects);

            foreach (GameObject gameObject in gameObjectsWithGivenMaterial)
            {
                string name = gameObject.name;
                Debug.Log($"<color=green> {name}");
            }

            ReplaceMaterials(gameObjectsWithGivenMaterial);

            EditorUtility.DisplayDialog("Material Replace", "Materials have been replaced", "OK");

            Selection.objects = gameObjectsWithGivenMaterial.ToArray();
        }

        private void HandleMaterialSelection()
        {
            List<GameObject> gameObjectsWithGivenMaterial = new List<GameObject>();
            Renderer[] rendererGameObjects = Resources.FindObjectsOfTypeAll<Renderer>();
            gameObjectsWithGivenMaterial = GetMGameObjectsWithMatList(rendererGameObjects);

            foreach (GameObject gameObject in gameObjectsWithGivenMaterial)
            {
                string name = gameObject.name;
                Debug.Log($"<color=green> {name}");
            }

            Selection.objects = gameObjectsWithGivenMaterial.ToArray();
        }

        private void ReplaceMaterials(List<GameObject> gameObjectsWithGivenMaterial)
        {
            foreach (GameObject gameObject in gameObjectsWithGivenMaterial)
            {
                
                Renderer renderer = gameObject.GetComponent<Renderer>();
                if (renderer == null) continue;

                Undo.RecordObject(renderer, "Replace Material Object");

                Material[] mats = renderer.sharedMaterials;
                bool matReplaced = false;

                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == materialToFind)
                    {
                        mats[i] = materialToReplace;
                        matReplaced = true;

                    }
                }

                if (matReplaced)
                {
                    renderer.sharedMaterials = mats;
                    EditorUtility.SetDirty(renderer);
                }

            }
        }

        private List<GameObject> GetMGameObjectsWithMatList(Renderer[] rendererGameObjects)
        {
            List<GameObject> gameObjects = new List<GameObject>();
            foreach (Renderer rend in rendererGameObjects)
            {
                if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(rend)))
                {
                    Material[] mat = rend.sharedMaterials;

                    foreach (Material mt in mat)
                    {
                        if (mt == materialToFind)
                        {
                            gameObjects.Add(rend.gameObject);
                            break;
                        }
                    }
                }


            }

            return gameObjects;
        }
    }

}
