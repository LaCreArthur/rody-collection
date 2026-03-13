using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{

    public AudioClip[] phonemes;
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

    public void PlaySingle(AudioClip clip)
    {
        soundSource.clip = clip;
        soundSource.Play();
    }

    // Choose a sound from clips
    public void RandomSound(params AudioClip[] clips)
    {
        int randomIndex = Random.Range(0, clips.Length);
        soundSource.clip = clips[randomIndex];
        soundSource.Play();
    }


    // Takes a list of phonemes to be played
    public void PlayDialog(List<int> phonemeList, float pitch = 1f)
    {
        if (phonemeList == null)
        {
            Debug.LogWarning("[SoundManager] PlayDialog called with null phoneme list");
            isPlaying = false;
            soundSource.pitch = 1f;
            return;
        }

        if (phonemeList.Count > 0)
        {
            if (!TryAssignPhonemeClip(phonemeList))
            {
                phonemeList.RemoveAt(0);
                PlayDialog(phonemeList, pitch);
                return;
            }

            StartCoroutine(playPhoneme(phonemeList, pitch));
        }
        else
        {
            isPlaying = false;
            Debug.Log("phoneme played");
            soundSource.pitch = 1f;
        }
    }

    // Times phonemes
    IEnumerator playPhoneme(List<int> phonemeList, float pitch = 1f)
    {
        if (soundSource.clip == null)
        {
            Debug.LogWarning($"[SoundManager] playPhoneme started without a clip. Remaining sequence: {FormatPhonemeSequence(phonemeList)}");
            isPlaying = false;
            yield break;
        }

        soundSource.Play();
        float crossTime = 0.01f;
        if (pitch < 1f) {
            crossTime = crossTime - (pitch / 100);
        }
        else if (pitch > 1f) {
            crossTime = crossTime + (pitch / 100);
        }

        //Debug.Log("crossTime is : " + crossTime);
        float time = soundSource.clip.length - crossTime;
        yield return new WaitForSeconds(time);
        phonemeList.RemoveAt(0);
        //Debug.Log (phonemeList.Count);
        PlayDialog(phonemeList, pitch);
    }

    public void InitPhoneme(List<int> phonemeList, float pitch, bool isMastico = false, bool process = true)
    {
        if (isMastico) MasticoSpeak(phonemeList, process);
        else
        {
            isPlaying = true;
            soundSource.pitch = pitch;
            PlayDialog(phonemeList, pitch);
        }
    }

    bool TryAssignPhonemeClip(List<int> phonemeList)
    {
        int phonemeIndex = phonemeList[0];
        if (phonemes == null)
        {
            Debug.LogError($"[SoundManager] Phoneme array is null. Cannot play sequence: {FormatPhonemeSequence(phonemeList)}");
            return false;
        }

        if (phonemeIndex < 0 || phonemeIndex >= phonemes.Length)
        {
            Debug.LogWarning($"[SoundManager] Ignoring out-of-range phoneme index {phonemeIndex}. Array length={phonemes.Length}. Remaining sequence: {FormatPhonemeSequence(phonemeList)}");
            return false;
        }

        AudioClip clip = phonemes[phonemeIndex];
        if (clip == null)
        {
            Debug.LogWarning($"[SoundManager] Ignoring missing phoneme clip at index {phonemeIndex}. Remaining sequence: {FormatPhonemeSequence(phonemeList)}");
            return false;
        }

        soundSource.clip = clip;
        return true;
    }

    string FormatPhonemeSequence(List<int> phonemeList)
    {
        if (phonemeList == null)
            return "<null>";

        if (phonemeList.Count == 0)
            return "<empty>";

        return string.Join(",", phonemeList);
    }

    public IEnumerator MasticoSpeak(List<int> phonemes, bool process)
    {
        Debug.Log("Mastico speak");
        gm.MasticoAnimator.SetBool("isSpeaking", true);
        float pitch = (isZambla)?0.9f:1.0f;
        // make a copy of the phonemes to not consume the original liste
        List<int> phonemeList = new List<int>(phonemes);
        InitPhoneme(phonemeList, pitch);
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

    public List<int> RandomOui()
    {
        
        int rand = Random.Range(0, 19);

        // if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex < 6)
        //     rand = Random.Range(0, 5); 

        // if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex > 12)
        //     rand = Random.Range(3, 15);

        switch (rand)
        {
            case 0:
                return StringToPhonemes("ouu_i _ s_et_b_i_un");
            case 1:
                return StringToPhonemes("b_r_a_v_o");
            case 2:
                return StringToPhonemes(("ouu_i _ b_i_in_j_ou_et"));
            case 3:
                return StringToPhonemes(("ouu_i"));
            case 4:
                return StringToPhonemes("s_et_b_i_un");
            case 5:
                return StringToPhonemes("b_i_in_j_ou_et ");
            case 6:
                return StringToPhonemes("b_r_a_v_o _ l_e_v_o");
            case 7:
                return StringToPhonemes("a ou_i_ou_i_ou_i _ s_et_g_a_gn_et");
            case 8:
                return StringToPhonemes("s_et_b_i_un c_o_p_un");
            case 9:
                return StringToPhonemes("s_et_t_ai_b_i_un");
            case 10:
                return StringToPhonemes("a_a_a_i_l_et_t_ai_f_a_s_i_l_s_e_l_u_i_l_a");
            case 11:
                return StringToPhonemes("f_et_l_i_s_i_t_a_s_i_on _ m_ou_s_a_y_on");
            case 12:
                return StringToPhonemes("u_m_m___t_et_b_a_l_ai_z");
            case 13:
                return StringToPhonemes("ouu_i__b_r_a_v_o__b_i_in_j_ou_et__s_et_b_i_in_s_a__j_o_r_ai_p_a_d_i_m_i_eu__f_et_l_i_s_i_t_a_s_i_on");
            case 14:
                return StringToPhonemes("s_a a_l_oh_r _ c_ai_l_t_a_l_an");
            case 15:
                return StringToPhonemes("l_a_ch_an_s__eu_r_eu_m__b_r_a_v_o");
            case 16:
                return StringToPhonemes("ouu_i _ s_et_b_i_un");
            case 17:
                return StringToPhonemes("b_r_a_v_o");
            case 18:
                return StringToPhonemes(("ouu_i _ b_i_in_j_ou_et"));
            default: return new List<int> { };
        }
    }
    public List<int> RandomNon()
    {
        int rand = Random.Range(0, 16);

        // if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex < 6)
        //     rand = Random.Range(0, 5); 

        // if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex > 12)
        //     rand = Random.Range(3, 15); 

        switch (rand)
        {
            case 0:
                return StringToPhonemes("s_ee_n_ai_p_a_s_a _ et_s_ai_y_d_e_n_ou_v_o");
            case 1:
                return StringToPhonemes("n_on _ r_e_c_o_m_an_s");
            case 2:
                return StringToPhonemes("n_on _ ch_ai_r_ch_an_c_oh_r");
            case 3:
                return StringToPhonemes("r_e_c_o_m_an_s_eu");
            case 4:
                return StringToPhonemes("n_on");
            case 5:
                return StringToPhonemes("ch_ai_r_ch_an_c_oh_r_un_p_ee");
            case 6:
                return StringToPhonemes("ch_ai_r_ch_an_c_oh_r_eu");
            case 7:
                return StringToPhonemes("s_ee_n_ai_p_a_d_u_t_ouu_s_a");
            case 8:
                return StringToPhonemes("t_et_b_ou_r_et ou_p_a");
            case 9:
                return StringToPhonemes("et_et_et_s_et_l_ou_p_et");
            case 10:
                return StringToPhonemes("s_et_n_on _ d_o_m_a_j");
            case 11:
                return StringToPhonemes("b_i_in_s_u_r_c_e_n_on");
            case 12:
                return StringToPhonemes("m_ai_r_et_f_l_et_ch_i _ s_eu_n_ai_p_a_s_a_r_o_d_i");
            case 13:
                return StringToPhonemes("a_r_ai_t_d_e_c_l_i_c_et_o_a_z_a_r");
            case 14:
                return StringToPhonemes("ai_s_c_e_ti_u_a_et_s_ai_y_et_o_m_ou_un");
            case 15:
                return StringToPhonemes("p_ai_r_d_u l_u_l_u");
            default: return new List<int> { };
        }
    }
    public List<int> RandomPresque()
    {
        
        int rand = Random.Range(0, 17);

        // if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex < 6)
        //     rand = Random.Range(0, 2);
        
        // if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex > 12)
        //     rand = Random.Range(3, 15); 

        switch (rand)
        {
            case 0:
                return StringToPhonemes("s_et_p_r_ai_s_c_e_s_a _ ch_ai_r_ch_in_p_e_m_i_e");
            case 1:
                return StringToPhonemes("t_u_i_ai_p_r_ai_s_c _ ch_ai_r_ch_an_c_oh_r");
            case 2:
                return StringToPhonemes("t_u_n_ai_p_a_l_oi _ m_a_r_g_ou_l_in");
            case 3:
                return StringToPhonemes("p_r_ai_s c_");
            case 4:
                return StringToPhonemes("j_e_s_an_c_e_s_a_v_i_un");
            case 5:
                return StringToPhonemes("s_et_t_ai_p_a_l_ou_in _ c_on_s_an_t_r_e_t_oi");
            case 6:
                return StringToPhonemes("an_c_oh_r_un_p_e_t_i_t_et_f_oh_r");
            case 7:
                return StringToPhonemes("r_e_g_a_r_d_un_p_e_m_i_ee");
            case 8:
                return StringToPhonemes("s_et_p_a_l_ou_in m_ai n_on");
            case 9:
                return StringToPhonemes("ou_i _ _ a_n_on");
            case 10:
                return StringToPhonemes("t_et_s_u_r _ _ m_oi_p_a_t_r_o");
            case 11:
                return StringToPhonemes("d_o_m_a_j _ t_u_i_c_r_oi_y_ai_p_ou_r_t_an");
            case 12:
                return StringToPhonemes("s_et_b_i_z_a_r _ s_et_t_ai_s_a_n_oh_r_m_a_l_m_an");
            case 13:
                return StringToPhonemes("d_e_v_i_n");
            case 14:
                return StringToPhonemes("l_a_p_r_o_ch_ai_n_s_et_l_a_b_oh_n");
            case 15:
                return StringToPhonemes("a_un_p_i_c_s_ai_l_p_r_ai_t_u_l_a_v_ai");
            case 16:
                return StringToPhonemes("p_r_ai_ai_ai_s c__d_o_m_a_j_");

            default: return new List<int> { };
        }
    }


    public List<int> StringToPhonemes(string s)
    {
        string[] words = s.Split(' ');
		List<int> phonemesList = new List<int>();
        foreach (string w in words)
        {
            // get all words
            string[] phonemes = w.Split('_');
			List<int> wordList = new List<int>();
            foreach (string p in phonemes)
            {
                wordList.Add(getPhoneme(p));
            }
            wordList.Add(P.rienp);
            // fill the word into the global list
            phonemesList.AddRange(wordList);
        }
        return phonemesList;
    }

    public int getPhoneme (string p) {
        // get all phonemes in words
        switch (p)
        {
            // fill the word list with phonemes
            case "i": return P.i;
            case "u": return P.u;
            case "ou": return P.ou;
            case "a": return P.a;
            case "oh": return P.oh;
            case "o": return P.o;
            case "et": return P.et;
            case "ai": return P.ai;
            case "eu": return P.eu;
            case "ee": return P.ee;
            case "e": return P.e;
            case "an": return P.an;
            case "on": return P.onp;
            case "in": return P.inp;
            case "un": return P.un;
            case "y": return P.y;
            case "oi": return P.oi;
            case "ui": return P.ui;
            case "l": return P.l;
            case "r": return P.r;
            case "p": return P.p;
            case "t": return P.t;
            case "c": return P.c;
            case "b": return P.b;
            case "d": return P.d;
            case "g": return P.g;
            case "m": return P.m;
            case "n": return P.n;
            case "gn": return P.gn;
            case "s": return P.s;
            case "f": return P.f;
            case "ch": return P.ch;
            case "z": return P.z;
            case "v": return P.v;
            case "j": return P.j;
            case ",": return P.rienp;
            case ".": return P.rien;
            case "-": return P.bruitBlanc;
            case "ti": return P.ti;
            case "ouu": return P.ouu;
            case "cuicui": return P.cuicui;
            case "pop": return P.pop;
            default : return P.rienp;
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

