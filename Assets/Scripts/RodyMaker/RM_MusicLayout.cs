using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RM_MusicLayout : RM_Layout
{
    public Dropdown introDropDrown, loopDropDown;
    public AudioSource audioSource;

    static readonly string[] Music =
    {
        "i1", "i2", "i3", "l1", "l2", "l3", "l4", "l5", "l6", "l7", "l8", "l9", "l10", "l11", "l12", "l13",
        "l14", "l15", "l2oiseaux", "torrent", "bim"
    };

    public void RM_ReturnClick()
    {
        if (!gm.CanEdit) return;
        SetLayouts(gm.introLayout);
        UnsetLayouts(gm.musicLayout, gm.title);
    }

    public void ListenClick()
    {
        if (!gm.CanEdit) return;
        StopAllCoroutines();
        StartCoroutine(PlayMusics());
    }

    IEnumerator PlayMusics()
    {
        audioSource.clip = gm.sm.getMusic(gm.CurrentScene.music.introMusic);
        audioSource.Play();
        while (audioSource.isPlaying) yield return null;
        audioSource.clip = gm.sm.getMusic(gm.CurrentScene.music.sceneMusic);
        audioSource.Play();
    }

    public void SaveMusic(int dropDown)
    {
        if (!gm.CanEdit) return;
        string selected = Music[(dropDown == 0 ? introDropDrown : loopDropDown).value];
        var music = gm.CurrentScene.music;
        string previous = dropDown == 0 ? music.introMusic : music.sceneMusic;
        if (selected == previous) return;
        if (dropDown == 0) music.introMusic = selected;
        else music.sceneMusic = selected;
        StoryRoot.Session.NotifyEdited();
        StopAllCoroutines();
        audioSource.clip = gm.sm.getMusic(selected);
        audioSource.Play();
    }

    public void SetMusic()
    {
        introDropDrown.SetValueWithoutNotify(System.Array.IndexOf(Music, gm.CurrentScene.music.introMusic));
        loopDropDown.SetValueWithoutNotify(System.Array.IndexOf(Music, gm.CurrentScene.music.sceneMusic));
    }

    void OnDisable()
    {
        StopAllCoroutines();
        audioSource.Stop();
    }
}
