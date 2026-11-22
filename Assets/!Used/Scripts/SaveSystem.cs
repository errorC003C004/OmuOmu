using UnityEngine;
using System.IO;

public static class SaveSystem
{
    [System.Serializable]
    public class OptionsSaveData
    {
        public float MasterVolume;
        public float SFXVolume;
        public float MusicVolume;
        public float BackgroundVolume;
        public int Quality;
        public int FullscreenMode;
        public bool IsPixel;
    }

    public class PlayerSaveData
    {
        public float[] Position;
        public int sceneIndex;
    }

    private static string OptionsFilePath => Path.Combine(Application.persistentDataPath, "options.json");
    private static string PlayerFilePath => Path.Combine(Application.persistentDataPath, "player.json");

    // Options Save/Load
    public static void SaveOptions(OptionsSaveData optionsData)
    {
        File.WriteAllText(
            OptionsFilePath,
            JsonUtility.ToJson(optionsData, true)
        );

        Debug.Log($"<color=black>[Save System]</color> Options saved to {OptionsFilePath}");
    }

    public static OptionsSaveData LoadOptions()
    {
        if (!File.Exists(OptionsFilePath))
        {
            Debug.LogWarning("<color=black>[Save System]</color>No options file found.");
            return null;
        }

        return JsonUtility.FromJson<OptionsSaveData>(
            File.ReadAllText(OptionsFilePath)
        );
    }

    public static string OptionsFileName()
    {
        return OptionsFilePath;
    }


    // Player Save/Load
    public static void SavePlayer(PlayerSaveData playerData)
    {
        File.WriteAllText(
            PlayerFilePath,
            JsonUtility.ToJson(playerData, true)
        );

        Debug.Log($"<color=black>[Save System]</color> Player saved to {PlayerFilePath}");
    }

    public static PlayerSaveData LoadPlayer()
    {
        if (!File.Exists(PlayerFilePath))
        {
            Debug.LogWarning("<color=black>[Save System]</color>No player file found.");
            return null;
        }

        return JsonUtility.FromJson<PlayerSaveData>(
            File.ReadAllText(PlayerFilePath)
        );
    }

    public static string PlayerFileName()
    {
        return PlayerFilePath;
    }
}