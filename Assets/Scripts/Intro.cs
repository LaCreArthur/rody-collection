using UnityEngine;
using System.Collections;

public class Intro : MonoBehaviour
{

    public AudioClip introMusic;
    public AudioSource sceneMusic;
    public GameManager gm;
    public bool isPlaying = true;
    public void Play()
    {
        StartCoroutine(Sequence());
    }


    IEnumerator Sequence()
    {

        // introMusic
        gm.source.Play();
        while (gm.source.isPlaying)
        {
            yield return null;
        }

        // Dialog
        StartCoroutine(Dialog());
    }

    public IEnumerator Dialog()
    {
        yield return new WaitForSeconds(0.5f);
        
        // intro dialog
        if (gm.sm.isMastico1)
            StartCoroutine(gm.sm.MasticoSpeak(gm.getDial(1), false));
        else
        {
            gm.sm.currentDialIndex = 1;
            gm.sceneAnimator.isSpeaking = true;
            gm.sm.InitPhoneme(gm.getDial(1), gm.sm.pitch1); // Other speak
        }

        while (gm.sm.isPlaying)
        {
            yield return null;
        }
        
        // little break 
        gm.sceneAnimator.isSpeaking = false;
        yield return new WaitForSeconds(0.5f);

        if (gm.getDial(2).Count > 0)
        {
            if (gm.sm.isMastico2)
                StartCoroutine(gm.sm.MasticoSpeak(gm.getDial(2), false));
            else
            {
                gm.sm.currentDialIndex = 2;
                gm.sceneAnimator.isSpeaking = true;
                gm.sm.InitPhoneme(gm.getDial(2), gm.sm.pitch2);
            }
            while (gm.sm.isPlaying)
            {
                yield return null;
            }
        }

        gm.sceneAnimator.isSpeaking = false;

        if (gm.getDial(6).Count > 0)
        {
            if (gm.sm.isMastico3) 
                StartCoroutine(gm.sm.MasticoSpeak(gm.getDial(6), false));
            else
            {
                gm.sm.currentDialIndex = 3;
                gm.sm.InitPhoneme(gm.getDial(6), gm.sm.pitch3);
            }
        }

        while (gm.sm.isPlaying)
        {
            yield return null;
        }

        gm.clickIntro = true;
        isPlaying = false;

        if (gm.currentScene == WorkingStory.SceneCount) // last scene animation loop
        {
            gm.sceneAnimator.isSpeaking = true;
        }
    }

    void Update()
    {
        // start the music and restart it after repeat
        if (gm.clickIntro && !sceneMusic.isPlaying && !gm.introOver)
        {
            sceneMusic.Play();
        }
    }

}
