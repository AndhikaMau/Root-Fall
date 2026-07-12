using UnityEngine;

public class ExpProgressResetter : MonoBehaviour
{
    [ContextMenu("Reset EXP Progress")]
    public void ResetExpProgress()
    {
        ExpProgress.ResetProgress();
        Debug.Log("EXP progress reset. Stop and Play again to show collected EXP objects.");
    }
}
