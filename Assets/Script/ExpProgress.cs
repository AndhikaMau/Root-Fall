using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class ExpProgress
{
    public const int RequiredExpForEvolve = 5;

    private const string ExpCountKey = "RootFall_ExpCount";
    private const string CollectedIdListKey = "RootFall_CollectedExpIds";
    private const string CollectedExpPrefix = "RootFall_CollectedExp_";
    private const string HasEvolvedKey = "RootFall_HasEvolved";

    public static int ExpCount => PlayerPrefs.GetInt(ExpCountKey, 0);
    public static bool HasEvolved => PlayerPrefs.GetInt(HasEvolvedKey, 0) == 1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterAutoResetOnQuit()
    {
        Application.quitting -= ResetProgressOnQuit;
        Application.quitting += ResetProgressOnQuit;
    }

    public static bool HasCollected(string collectibleId)
    {
        if (string.IsNullOrWhiteSpace(collectibleId))
            return false;

        return PlayerPrefs.GetInt(CollectedExpPrefix + collectibleId, 0) == 1;
    }

    public static bool TryCollect(string collectibleId)
    {
        if (string.IsNullOrWhiteSpace(collectibleId) || HasCollected(collectibleId))
            return false;

        PlayerPrefs.SetInt(CollectedExpPrefix + collectibleId, 1);
        PlayerPrefs.SetInt(ExpCountKey, ExpCount + 1);
        SaveCollectedId(collectibleId);
        PlayerPrefs.Save();

        return true;
    }

    public static bool CanEvolve()
    {
        return ExpCount >= RequiredExpForEvolve;
    }

    public static void MarkEvolved()
    {
        PlayerPrefs.SetInt(HasEvolvedKey, 1);
        PlayerPrefs.Save();
    }

    public static void ResetProgress()
    {
        string collectedIds = PlayerPrefs.GetString(CollectedIdListKey, string.Empty);
        string[] ids = collectedIds.Split('|');

        for (int i = 0; i < ids.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(ids[i]))
                PlayerPrefs.DeleteKey(CollectedExpPrefix + ids[i]);
        }

        PlayerPrefs.DeleteKey(ExpCountKey);
        PlayerPrefs.DeleteKey(CollectedIdListKey);
        PlayerPrefs.DeleteKey(HasEvolvedKey);
        PlayerPrefs.Save();
    }

    private static void SaveCollectedId(string collectibleId)
    {
        string collectedIds = PlayerPrefs.GetString(CollectedIdListKey, string.Empty);

        if (("|" + collectedIds + "|").Contains("|" + collectibleId + "|"))
            return;

        PlayerPrefs.SetString(
            CollectedIdListKey,
            string.IsNullOrWhiteSpace(collectedIds) ? collectibleId : collectedIds + "|" + collectibleId);
    }

    private static void ResetProgressOnQuit()
    {
        ResetProgress();
    }

#if UNITY_EDITOR
    [MenuItem("Root Fall/Reset EXP Progress")]
    private static void ResetProgressFromEditorMenu()
    {
        ResetProgress();
        Debug.Log("EXP progress reset. Play the scene again to show collected EXP objects.");
    }
#endif
}
