using UnityEngine;
using UnityEditor;
public class MinigameManager : MonoBehaviour
{
    public static MinigameManager instance;

    public bool areGamesPlayable = true;

    public enum CurrentActiveMinigame
    {
        Basketball,
        Drawing,
        Photography,
    }
    public CurrentActiveMinigame currentActiveMinigame;

    public bool showInDropdown = true;
    //Basketball
    public GameObject BasketballUI;
    public GameObject BasketballHoldUI;
    //Drawing
    public GameObject DrawingUI;
    //Photography
    public GameObject PhotographyUI;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (areGamesPlayable)
        {
            switch (currentActiveMinigame)
            {
                case CurrentActiveMinigame.Basketball:
                    BasketballUI.SetActive(true);
                    BasketballHoldUI.SetActive(true);
                    DrawingUI.SetActive(false);
                    PhotographyUI.SetActive(false);
                    break;
                case CurrentActiveMinigame.Drawing:
                    BasketballUI.SetActive(false);
                    BasketballHoldUI.SetActive(false);
                    DrawingUI.SetActive(true);
                    PhotographyUI.SetActive(false);
                    break;
                case CurrentActiveMinigame.Photography:
                    BasketballUI.SetActive(false);
                    BasketballHoldUI.SetActive(false);
                    DrawingUI.SetActive(false);
                    PhotographyUI.SetActive(true);
                    break;
            }
        }
        else
        {
            BasketballUI.SetActive(false);
            BasketballHoldUI.SetActive(false);
            DrawingUI.SetActive(false);
            PhotographyUI.SetActive(false);
        }
    }

}


[CustomEditor(typeof(MinigameManager))]
public class MinigameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var minigameManager = (MinigameManager)target;

        // Double click to open thingy
        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(minigameManager), typeof(MonoScript), false);

        minigameManager.areGamesPlayable = EditorGUILayout.Toggle("Are Games Playable:", minigameManager.areGamesPlayable);
        minigameManager.currentActiveMinigame = (MinigameManager.CurrentActiveMinigame)EditorGUILayout.EnumPopup("Current Active Minigame:", minigameManager.currentActiveMinigame);

        GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Show Config Stuff");
            minigameManager.showInDropdown = GUILayout.Toggle(minigameManager.showInDropdown, GUIContent.none);
            GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        if(minigameManager.showInDropdown)
        {
            // Q: can you bold the labels? If so, is it easy to do? A: Yes, but it requires a bit of extra work. You can use GUIStyle to create a bold label style and then apply it to the labels. Here's how you can do it:
            GUIStyle boldLabelStyle = new GUIStyle(GUI.skin.label);
            boldLabelStyle.fontStyle = FontStyle.Bold;
            GUILayout.Label("Basketball Config:", boldLabelStyle);
            minigameManager.BasketballUI = (GameObject)EditorGUILayout.ObjectField("Basketball UI:", minigameManager.BasketballUI, typeof(GameObject), true);
            minigameManager.BasketballHoldUI = (GameObject)EditorGUILayout.ObjectField("Basketball Hold UI:", minigameManager.BasketballHoldUI, typeof(GameObject), true);
            GUILayout.Label("Drawing Config:", boldLabelStyle);
            minigameManager.DrawingUI = (GameObject)EditorGUILayout.ObjectField("Drawing UI:", minigameManager.DrawingUI, typeof(GameObject), true);
            GUILayout.Label("Photography Config:", boldLabelStyle);
            minigameManager.PhotographyUI = (GameObject)EditorGUILayout.ObjectField("Photography UI:", minigameManager.PhotographyUI, typeof(GameObject), true);
        }
    }
}
