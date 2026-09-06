using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{

    [SerializeField] AudioClip noise;
    [SerializeField] AudioClip bird;
    [SerializeField] AudioClip pop;
    public AudioClip[] sounds_fx_debutObj;
    public AudioClip[] sounds_fx_debutNgp;
    public AudioClip[] sounds_fx_fin;
    public AudioClip[] sounds_oui;
    public AudioClip[] sounds_presque;
    public AudioClip[] sounds_non;
    public AudioClip[] musics;
    public GameManager gm;
    public AudioSource soundSource;
    //[HideInInspector]
    public float pitch1 = 1f, pitch2 = 1f, pitch3 = 1f;
    //[HideInInspector]
    public bool isPlaying = false, isMastico1 = false, isMastico2 = false, isMastico3 = false, isZambla = false;
    public int currentDialIndex = 0;

    static RodySpeechEngine engine;
    Coroutine speechRoutine;
    AudioClip speechClip;

    static RodySpeechEngine Engine => engine ??= new RodySpeechEngine(
        Resources.Load<TextAsset>("Speech/Rody1").bytes,
        Resources.Load<TextAsset>("Speech/Tables").bytes);

    public void RandomSound(params AudioClip[] clips)
    {
        StopSpeech();
        soundSource.clip = clips[Random.Range(0, clips.Length)];
        soundSource.Play();
    }

    public void Speak(string dialogue, float pitch = 1f)
    {
        StopSpeech();
        var parts = Engine.RenderDialogue(dialogue,
            token => Debug.LogWarning($"Unknown speech token '{token}' plays as a pause."));
        if (parts.Count == 0) return;
        isPlaying = true;
        soundSource.loop = false;
        soundSource.pitch = pitch;
        speechRoutine = StartCoroutine(PlaySpeech(parts));
    }

    IEnumerator PlaySpeech(System.Collections.Generic.List<RodySpeechPart> parts)
    {
        foreach (var part in parts)
        {
            if (part.Effect == RodySpeechEffect.None)
            {
                if (part.Samples.Length == 0) continue;
                var samples = new float[part.Samples.Length];
                for (int i = 0; i < samples.Length; i++) samples[i] = (part.Samples[i] - 128) / 128f;
                speechClip = AudioClip.Create("Rody speech", samples.Length, 1, RodySpeechEngine.SampleRate, false);
                speechClip.SetData(samples, 0);
                soundSource.clip = speechClip;
            }
            else
            {
                soundSource.clip = part.Effect == RodySpeechEffect.Noise ? noise :
                    part.Effect == RodySpeechEffect.Bird ? bird : pop;
            }
            soundSource.Play();
            // Playback owns completion, including character pitch. No guessed per-phoneme waits.
            while (soundSource.isPlaying) yield return null;
            soundSource.clip = null;
            ReleaseSpeechClip();
        }
        soundSource.pitch = 1f;
        isPlaying = false;
        speechRoutine = null;
    }

    void ReleaseSpeechClip()
    {
        if (speechClip == null) return;
        Destroy(speechClip);
        speechClip = null;
    }

    void StopSpeech()
    {
        if (speechRoutine != null) StopCoroutine(speechRoutine);
        speechRoutine = null;
        soundSource.Stop();
        soundSource.clip = null;
        soundSource.pitch = 1f;
        isPlaying = false;
        ReleaseSpeechClip();
    }

    void OnDisable() => StopSpeech();

    public IEnumerator MasticoSpeak(string dialogue, bool process)
    {
        Debug.Log("Mastico speak");
        gm.MasticoAnimator.SetBool("isSpeaking", true);
        float pitch = (isZambla)?0.9f:1.0f;
        Speak(dialogue, pitch);
        while (isPlaying)
        {
            yield return null;
        }
        if (process)
        {
            gm.MasticoAnimator.SetTrigger("Process");
            yield return new WaitForSeconds(0.1f);
        }
        gm.MasticoAnimator.SetBool("isSpeaking", false);
    }

    public string RandomOui()
    {
        
        int rand = Random.Range(0, 19);

        // if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex < 6)
        //     rand = Random.Range(0, 5); 

        // if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex > 12)
        //     rand = Random.Range(3, 15);

        switch (rand)
        {
            case 0:
                return "ouu_i _ s_et_b_i_un";
            case 1:
                return "b_r_a_v_o";
            case 2:
                return "ouu_i _ b_i_in_j_ou_et";
            case 3:
                return "ouu_i";
            case 4:
                return "s_et_b_i_un";
            case 5:
                return "b_i_in_j_ou_et ";
            case 6:
                return "b_r_a_v_o _ l_e_v_o";
            case 7:
                return "a ou_i_ou_i_ou_i _ s_et_g_a_gn_et";
            case 8:
                return "s_et_b_i_un c_o_p_un";
            case 9:
                return "s_et_t_ai_b_i_un";
            case 10:
                return "a_a_a_i_l_et_t_ai_f_a_s_i_l_s_e_l_u_i_l_a";
            case 11:
                return "f_et_l_i_s_i_t_a_s_i_on _ m_ou_s_a_y_on";
            case 12:
                return "u_m_m___t_et_b_a_l_ai_z";
            case 13:
                return "ouu_i__b_r_a_v_o__b_i_in_j_ou_et__s_et_b_i_in_s_a__j_o_r_ai_p_a_d_i_m_i_eu__f_et_l_i_s_i_t_a_s_i_on";
            case 14:
                return "s_a a_l_oh_r _ c_ai_l_t_a_l_an";
            case 15:
                return "l_a_ch_an_s__eu_r_eu_m__b_r_a_v_o";
            case 16:
                return "ouu_i _ s_et_b_i_un";
            case 17:
                return "b_r_a_v_o";
            case 18:
                return "ouu_i _ b_i_in_j_ou_et";
            default: return "";
        }
    }
    public string RandomNon()
    {
        int rand = Random.Range(0, 16);

        // if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex < 6)
        //     rand = Random.Range(0, 5); 

        // if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex > 12)
        //     rand = Random.Range(3, 15); 

        switch (rand)
        {
            case 0:
                return "s_ee_n_ai_p_a_s_a _ et_s_ai_y_d_e_n_ou_v_o";
            case 1:
                return "n_on _ r_e_c_o_m_an_s";
            case 2:
                return "n_on _ ch_ai_r_ch_an_c_oh_r";
            case 3:
                return "r_e_c_o_m_an_s_eu";
            case 4:
                return "n_on";
            case 5:
                return "ch_ai_r_ch_an_c_oh_r_un_p_ee";
            case 6:
                return "ch_ai_r_ch_an_c_oh_r_eu";
            case 7:
                return "s_ee_n_ai_p_a_d_u_t_ouu_s_a";
            case 8:
                return "t_et_b_ou_r_et ou_p_a";
            case 9:
                return "et_et_et_s_et_l_ou_p_et";
            case 10:
                return "s_et_n_on _ d_o_m_a_j";
            case 11:
                return "b_i_in_s_u_r_c_e_n_on";
            case 12:
                return "m_ai_r_et_f_l_et_ch_i _ s_eu_n_ai_p_a_s_a_r_o_d_i";
            case 13:
                return "a_r_ai_t_d_e_c_l_i_c_et_o_a_z_a_r";
            case 14:
                return "ai_s_c_e_ti_u_a_et_s_ai_y_et_o_m_ou_un";
            case 15:
                return "p_ai_r_d_u l_u_l_u";
            default: return "";
        }
    }
    public string RandomPresque()
    {
        
        int rand = Random.Range(0, 17);

        // if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex < 6)
        //     rand = Random.Range(0, 2);
        
        // if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex > 12)
        //     rand = Random.Range(3, 15); 

        switch (rand)
        {
            case 0:
                return "s_et_p_r_ai_s_c_e_s_a _ ch_ai_r_ch_in_p_e_m_i_e";
            case 1:
                return "t_u_i_ai_p_r_ai_s_c _ ch_ai_r_ch_an_c_oh_r";
            case 2:
                return "t_u_n_ai_p_a_l_oi _ m_a_r_g_ou_l_in";
            case 3:
                return "p_r_ai_s c_";
            case 4:
                return "j_e_s_an_c_e_s_a_v_i_un";
            case 5:
                return "s_et_t_ai_p_a_l_ou_in _ c_on_s_an_t_r_e_t_oi";
            case 6:
                return "an_c_oh_r_un_p_e_t_i_t_et_f_oh_r";
            case 7:
                return "r_e_g_a_r_d_un_p_e_m_i_ee";
            case 8:
                return "s_et_p_a_l_ou_in m_ai n_on";
            case 9:
                return "ou_i _ _ a_n_on";
            case 10:
                return "t_et_s_u_r _ _ m_oi_p_a_t_r_o";
            case 11:
                return "d_o_m_a_j _ t_u_i_c_r_oi_y_ai_p_ou_r_t_an";
            case 12:
                return "s_et_b_i_z_a_r _ s_et_t_ai_s_a_n_oh_r_m_a_l_m_an";
            case 13:
                return "d_e_v_i_n";
            case 14:
                return "l_a_p_r_o_ch_ai_n_s_et_l_a_b_oh_n";
            case 15:
                return "a_un_p_i_c_s_ai_l_p_r_ai_t_u_l_a_v_ai";
            case 16:
                return "p_r_ai_ai_ai_s c__d_o_m_a_j_";

            default: return "";
        }
    }


    public AudioClip getMusic(string music) {
        switch (music) {
            case "i1"  : return musics[0];
            case "i2"  : return musics[1];
            case "i3"  : return musics[2];
            case "l1"  : return musics[3];
            case "l2"  : return musics[4];
            case "l3"  : return musics[5];
            case "l4"  : return musics[6];
            case "l5"  : return musics[7];
            case "l6"  : return musics[8];
            case "l7"  : return musics[9];
            case "l8"  : return musics[10];
            case "l9"  : return musics[11];
            case "l10" : return musics[12];
            case "l11" : return musics[13];
            case "l12" : return musics[14];
            case "l13" : return musics[15];
            case "l14" : return musics[16];
            case "l15" : return musics[17];
            case "l2oiseaux" : return musics[18];
            case "torrent" : return musics[19];
            case "bim": return musics[20];
            default : return musics[0];
        }
    }

}

