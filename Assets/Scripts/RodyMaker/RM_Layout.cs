using UnityEngine;

public abstract class RM_Layout : MonoBehaviour
{
    protected RM_GameManager gm;

    protected virtual void Awake() => gm = GameObject.Find("GameManager").GetComponent<RM_GameManager>();

    protected void SetLayouts(params GameObject[] layouts)
    {
        foreach (var layout in layouts) layout.SetActive(true);
    }

    protected void UnsetLayouts(params GameObject[] layouts)
    {
        foreach (var layout in layouts) layout.SetActive(false);
    }

    protected static bool SameSpeech(SpeechDocument left, SpeechDocument right)
    {
        if (left.sourceText != right.sourceText || left.words.Count != right.words.Count) return false;
        for (int i = 0; i < left.words.Count; i++)
        {
            var a = left.words[i];
            var b = right.words[i];
            if (a.start != b.start || a.length != b.length || a.score != b.score || a.corrected != b.corrected)
                return false;
        }
        return true;
    }
}
