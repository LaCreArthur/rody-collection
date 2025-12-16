using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveToast : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Text content that will display the title")]
    public TMPro.TextMeshProUGUI titleTextContent;
    [Tooltip("Text content that will display the description")]
    public TMPro.TextMeshProUGUI descriptionTextContent;
    [Tooltip("Text content that will display the counter")]
    public TMPro.TextMeshProUGUI counterTextContent;

    [Tooltip("Rect that will display the description")]
    public RectTransform subTitleRect;
    [Tooltip("Canvas used to fade in and out the content")]
    public CanvasGroup canvasGroup;

    [Tooltip("Layout group containing the objective")]
    public HorizontalOrVerticalLayoutGroup layoutGroup;

    [Header("Transitions")]
    [Tooltip("Delay before moving complete")]
    public float completionDelay;
    [Tooltip("Duration of the fade in")]
    public float fadeInDuration = 0.5f;
    [Tooltip("Duration of the fade out")]
    public float fadeOutDuration = 2f;
    public float displayDuration = 5f;

    [Header("Sound")]
    [Tooltip("Sound that will be player on initialization")]
    public AudioClip initSound;
    [Tooltip("Sound that will be player on completion")]
    public AudioClip completedSound;

    [Header("Movement")]
    [Tooltip("Time it takes to move in the screen")]
    public float moveInDuration = 0.5f;
    [Tooltip("Animation curve for move in, position in x over time")]
    public AnimationCurve moveInCurve;

    [Tooltip("Time it takes to move out of the screen")]
    public float moveOutDuration = 2f;
    [Tooltip("Animation curve for move out, position in x over time")]
    public AnimationCurve moveOutCurve;

    float m_StartFadeTime;
    float m_StartDisplayTime;
    bool m_IsFadingIn;
    bool m_IsFadingOut;
    bool m_IsMovingIn;
    bool m_IsMovingOut;
    bool m_IsDisplay;
    AudioSource m_AudioSource;
    RectTransform m_RectTransform;

    public void Initialize(string titleText, string descText, string counterText, bool isOptionnal, float delay, bool waitForShow = false)
    {
        // set the description for the objective, and forces the content size fitter to be recalculated
        Canvas.ForceUpdateCanvases();

        titleTextContent.text = titleText;
        descriptionTextContent.text = descText;
        counterTextContent.text = counterText;

        if (GetComponent<RectTransform>())
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }

        if (!waitForShow)
            Show(delay);
    }

    public void Show(float delay)
    {
        m_StartFadeTime = Time.time + delay;
        // start the fade in
        m_IsFadingIn = true;
        m_IsMovingIn = true;
        m_IsDisplay = false;
    }

    [Button]
    public void Complete()
    {
        m_StartFadeTime = Time.time + completionDelay;
        m_IsFadingIn = false;
        m_IsMovingIn = false;

        // start the fade out
        m_IsMovingOut = true;
        m_IsFadingOut = true;
        // if a sound was set, play it
        PlaySound(completedSound);
    }
    
    [Button]
    public void MoveOut()
    {
        if (!m_IsDisplay) return;
        Debug.Log("moving out");
        m_IsMovingIn = false;
        m_IsMovingOut = true;
        m_StartFadeTime = Time.time + displayDuration;
    }
    [Button]
    public void MoveIn()
    {
        if (m_IsDisplay) return;
        Debug.Log("move in");
        m_IsMovingOut = false;
        m_IsMovingIn = true;
        m_StartFadeTime = Time.time;
    }

    void Update()
    {
        if (Input.GetButtonDown(GameConstants.k_ButtonNameMoveObjective))
        {
            if (m_IsMovingIn || m_IsDisplay) MoveOut();
            else MoveIn();
        }
        
        float timeSinceFadeStarted = Time.time - m_StartFadeTime;

        subTitleRect.gameObject.SetActive(!string.IsNullOrEmpty(descriptionTextContent.text));

        if (m_IsFadingIn && !m_IsFadingOut)
        {
            // fade in
            if (timeSinceFadeStarted < fadeInDuration)
            {
                // calculate alpha ratio
                canvasGroup.alpha = timeSinceFadeStarted / fadeInDuration;
            }
            else
            {
                canvasGroup.alpha = 1f;
                // end the fade in
                m_IsFadingIn = false;
                m_IsDisplay = true;
                m_StartDisplayTime = Time.time;
                PlaySound(initSound);
                Debug.Log("start display, end fade in, display set to true");
                MoveOut();
            }
        }

        if (m_IsMovingIn && !m_IsMovingOut)
        { 
            // move in
            if (timeSinceFadeStarted < moveInDuration)
            {
                layoutGroup.padding.left = (int)moveInCurve.Evaluate(timeSinceFadeStarted / moveInDuration);

                if (GetComponent<RectTransform>())
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
                }
            }
            else
            {
                // making sure the position is exact
                layoutGroup.padding.left = 0;

                if (GetComponent<RectTransform>())
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
                }

                m_IsMovingIn = false;
                m_IsDisplay = true;Debug.Log("end move in, display set to true");
                m_StartDisplayTime = Time.time;
                MoveOut();
            }

        }

        if (m_IsFadingOut)
        {
            // fade out
            if (timeSinceFadeStarted < fadeOutDuration)
            {
                // calculate alpha ratio
                canvasGroup.alpha = 1 - (timeSinceFadeStarted) / fadeOutDuration;
            }
            else
            {
                canvasGroup.alpha = 0f;

                // end the fade out, then destroy the object
                m_IsFadingOut = false;
                Destroy(gameObject);
            }
        }

        if (m_IsMovingOut)
        {
            //Debug.Log($"timeSinceFadeStarted: {timeSinceFadeStarted}, moveOutDuration: {moveOutDuration}");
            // move out
            if (timeSinceFadeStarted < moveOutDuration)
            {
                layoutGroup.padding.left = (int)moveOutCurve.Evaluate(timeSinceFadeStarted / moveOutDuration);

                if (GetComponent<RectTransform>())
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
                }
            }
            else
            {
                m_IsMovingOut = false;
                m_IsDisplay = false;Debug.Log("end move out, display set to false");
            }
        }
    }

    void PlaySound(AudioClip sound)
    {
        if (!sound)
            return;

        if (!m_AudioSource)
        {
            m_AudioSource = gameObject.AddComponent<AudioSource>();
            m_AudioSource.outputAudioMixerGroup = AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.HUDObjective);
        }

        m_AudioSource.PlayOneShot(sound);
    }
}
