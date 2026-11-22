using UnityEngine;
using UnityEditor;

public class ConfigManager : MonoBehaviour
{
    public static ConfigManager Instance;
    public bool IsDither = true;
    public GameObject DitherCamPrefab;
    public GameObject NormalCamPrefab;
    public GameObject pixelArtRawImage;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ToggleDither();
    }

    private void Update()
    {
        bool isDitherActive = pixelArtRawImage.activeInHierarchy;
        if (isDitherActive != IsDither)
        {
            ToggleDither();
        }
    }

    private void ToggleDither()
    {
        if (IsDither)
        {
            Camera camToDelete = Camera.main;
            GameObject DitherCam = Instantiate(DitherCamPrefab, camToDelete.transform.parent, transform);
            DitherCam.transform.localRotation = camToDelete.transform.localRotation;
            Destroy(camToDelete.gameObject);
            pixelArtRawImage.SetActive(true);
            DitherCam.transform.localPosition = new Vector3(0, 0.5f, 0);
            DitherCam.tag = "MainCamera";
            Camera ditherMainCam = DitherCam.GetComponent<Camera>();
            Inventory.Instance.hand = ditherMainCam.transform.Find("Hand");
            Inventory.Instance.cam = ditherMainCam; 
            MoveCamera.Instance.viewCamera = ditherMainCam;
        }
        else
        {
            Camera camToDelete = Camera.main;
            GameObject NormalCam = Instantiate(NormalCamPrefab, camToDelete.transform.parent, transform);
            NormalCam.transform.localRotation = camToDelete.transform.localRotation;
            Destroy(camToDelete.gameObject);
            pixelArtRawImage.SetActive(false);
            NormalCam.transform.localPosition = new Vector3(0, 0.5f, 0);
            NormalCam.tag = "MainCamera";
            Camera normalMainCam = NormalCam.GetComponent<Camera>();
            Inventory.Instance.hand = normalMainCam.transform.Find("Hand");
            Inventory.Instance.cam = normalMainCam; 
            MoveCamera.Instance.viewCamera = normalMainCam;
        }
    }


}


[CustomEditor(typeof(ConfigManager))]
public class ConfigManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var configManager = (ConfigManager)target;

        // Double click to open thingy
        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(configManager), typeof(MonoScript), false);

        //Is Dither Stuff
        GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Is Dither");
            configManager.IsDither = GUILayout.Toggle(configManager.IsDither, GUIContent.none);
            GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        if(configManager.IsDither)
        {
            configManager.DitherCamPrefab = (GameObject)EditorGUILayout.ObjectField("Dither Cam Prefab:", configManager.DitherCamPrefab, typeof(GameObject), false);
            configManager.pixelArtRawImage = (GameObject)EditorGUILayout.ObjectField("Pixel Art Raw Image:", configManager.pixelArtRawImage, typeof(GameObject), false);
            configManager.NormalCamPrefab = (GameObject)EditorGUILayout.ObjectField("Normal Cam Prefab:", configManager.NormalCamPrefab, typeof(GameObject), false);
        }
    }
}
