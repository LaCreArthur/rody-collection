using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityReusables.ScriptableObjects.Variables;

[RequireComponent(typeof(Canvas))][RequireComponent(typeof(CanvasGroup))]
public class CanvasAnimator : MonoBehaviour
{
    [Header("Start behaviours")]
    public bool startVisible = true;
    public bool resetPosOnStart = true;
    
    [Header("On Show")]
    public BetterEvent onShowEvents;
    public bool playChildTweensOnShow;
    public bool blockRaycastWhenVisible = true;
    public bool fadeIn;
    [ShowIf("fadeIn")] public Ease fadeInEase = Ease.InOutSine;
    [ShowIf("fadeIn")] public float fadeInDuration;
    
    [Header("On Hide")]
    public BetterEvent onHideEvents;
    public bool rewindChildTweensOnHide;
    public bool fadeOut;
    [ShowIf("fadeOut")] public Ease fadeOutEase = Ease.InOutSine;
    [ShowIf("fadeOut")] public float fadeOutDuration;

    [Header("GameState")] 
    [Tooltip("UI Related Game State for auto Show/Hide. Leave empty if none")] 
    public GameStateSO relatedGameState;

    [ShowIf("@this.relatedGameState != null")]
    public GameStateVariable gameStateVariable;

    [Header("Infos")]
    [SerializeField] [ReadOnly]
    private bool isHidden;
    public bool IsHidden => isHidden;

    private List<Tween> childTweens;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        
        if (resetPosOnStart) GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        
        InitChildTweens();
    }

    private void OnEnable()
    {
        if (relatedGameState == null) return;
        gameStateVariable.AddOnChangeCallback(GameStateCallback);
    }

    private void OnDisable()
    {
        if (relatedGameState == null) return;
        gameStateVariable.RemoveOnChangeCallback(GameStateCallback);
    }

    private void GameStateCallback()
    {
        // if entering the related game state, Show the Canvas
        if (gameStateVariable.v == relatedGameState)
        {
            Show();
        }
        // else, if not hidden, Hide the Canvas
        else if (!isHidden) 
        {
            Hide();
        }
    }

    private void InitChildTweens()
    {
        // don't search for tweens if not needed
        if (!playChildTweensOnShow || !rewindChildTweensOnHide) return;

        // Init the list
        childTweens = new List<Tween>();

        // get all DOTweenAnimation components in child
        var tweenAnimations = GetComponentsInChildren<DOTweenAnimation>();

        GameObject previousChild = null;
        foreach (var anim in tweenAnimations)
        {
            // no need to add tweens if there is more than one DOTweenAnimation component on the same GO
            // since anim.GetTweens() returns all the tweens
            if (anim.gameObject == previousChild) continue;
            // add the tweens of this animation to the list
            childTweens.AddRange((anim.GetTweens()));
            // keep track of previous GO to avoid duplicates 
            previousChild = anim.gameObject;
            // Debug.Log($"{anim.gameObject.name} contains {anim.GetTweens().Count} tweens");
        }
    }

    private void Start()
    {
        if (startVisible)
            Show();
        else
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.blocksRaycasts = false;
        }
    }

    public void Show()
    {
        onShowEvents.Invoke();
        _canvasGroup.blocksRaycasts = blockRaycastWhenVisible;
        isHidden = false;

        DOTween.Kill(_canvasGroup);
        
        if (fadeIn) 
            _canvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeInEase).SetUpdate(true);
        else 
            _canvasGroup.alpha = 1;
        
        if (playChildTweensOnShow)
        {
            foreach (var tween in childTweens)
            {
                tween.Play();
            }
        }
    }
    
    public void Hide()
    {
        onHideEvents.Invoke();
        _canvasGroup.blocksRaycasts = false;
        isHidden = true;
        
        DOTween.Kill(_canvasGroup);
        
        if (fadeOut) 
            _canvasGroup.DOFade(0f, fadeOutDuration).SetEase(fadeOutEase).SetUpdate(true);
        else 
            _canvasGroup.alpha = 0;
        
        if (rewindChildTweensOnHide)
        {
            foreach (var tween in childTweens)
            {
                tween.Rewind();
            }
        }
    }
}
