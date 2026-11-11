// Copyright (c) 2025 PinePie. All rights reserved.

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace PinePie.PieTabs
{
    [InitializeOnLoad]
    public static class PieTabsLoader
    {
        private const string InstallPathKey = "PieTabs_LastInstallPath";

        static PieTabsLoader()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;

            ObjectChangeEvents.changesPublished += OnChangesPublished;

            EditorApplication.update += RunOnceOnLoad;

            EditorPrefs.SetString(InstallPathKey, PathUtility.GetPieTabsPath());

            AssemblyReloadEvents.beforeAssemblyReload += () =>
            {
                if (!Directory.Exists($"{EditorPrefs.GetString("PieTabs_LastInstallPath", "Assets")}/PinePie/PieTabs"))
                {
                    GameObject[] allObjects;

#if UNITY_6000_4_OR_NEWER
                    allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
#elif UNITY_2023_1_OR_NEWER
                    allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
                    allObjects = Object.FindObjectsOfType<GameObject>(true);
#endif

                    foreach (GameObject go in allObjects)
                    {
                        if (!go.scene.IsValid())
                            continue;

                        if (go.name == "[PieTabs_Data]")
                        {
                            go.hideFlags |= HideFlags.HideInHierarchy;
                            continue;
                        }

                        go.hideFlags &= ~HideFlags.HideInHierarchy;
                    }
                }
            };

        }

        static void RunOnceOnLoad()
        {
            EditorApplication.update -= RunOnceOnLoad;

            if (!Directory.Exists($"{PathUtility.GetPieTabsPath()}/PinePie/PieTabs"))
                return;

            BrowserTabs.Setup();
            SceneTabs.Setup();
        }


        public static void EnsurePieDeskOverlay()
        {
            var lastFocused = EditorWindow.focusedWindow;
            if (lastFocused != null && lastFocused.GetType().Name == "ProjectBrowser")
            {
                Selection.selectionChanged -= EnsurePieDeskOverlay;

                BrowserTabs.Setup();
                SceneTabs.Setup();
            }
        }

        /// please press F5 if anyway PieTabs UI is not visible. ///

        [MenuItem("Tools/Refresh PieTabs _F5")]
        public static void RefreshPieTabs()
        {
            BrowserTabs.Setup();
            SceneTabs.Setup();
        }

        static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            RefreshPieTabs();
        }


        private static void OnChangesPublished(ref ObjectChangeEventStream stream)
        {
            for (int i = 0; i < stream.length; i++)
            {
                if (stream.GetEventType(i) != ObjectChangeKind.CreateGameObjectHierarchy)
                    continue;

                stream.GetCreateGameObjectHierarchyEvent(i, out var createData);

                GameObject newGO = null;

#if UNITY_6000_4_OR_NEWER
                newGO = EditorUtility.EntityIdToObject(createData.entityId) as GameObject;
#else
                newGO = EditorUtility.EntityIdToObject(createData.instanceId) as GameObject;
#endif

                if (newGO != null && SceneTabsUI.ActiveSceneTabID != -1)
                {
                    SceneTabsUI.ActiveTab?.refs.Add(newGO);
                }
            }
        }

    }

}
#endif
