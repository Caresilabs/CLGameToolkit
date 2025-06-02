#if DOTWEEN

using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VisualTween : MonoBehaviour
{
    [SerializeField, Min(0)] private float Duration = 1f;
    [SerializeField, Min(0)] private float Delay;
    [SerializeField] private TweenType Type;
    [SerializeField] private Vector3 EndValue = new Vector3(1, 1, 1);
    [SerializeField] private Color EndColor = Color.black;
    [SerializeField] private Space Space = Space.Self;

    [SerializeField] private bool IsSequence; // TODO Support sequence of components

    // Additional options
    [SerializeField] private Ease Ease = Ease.InOutQuad;
    [SerializeField] private bool AutoPlay = true;
    [SerializeField] private bool DestroyOnComplete = false;
    [SerializeField] private bool ResetOnDisable;
    [SerializeField] private bool IgnoreTimeScale;

    [SerializeField] private bool Loop = false;
    [SerializeField] private int LoopCount = 1;
    [SerializeField] private LoopType LoopType;

    [SerializeField] private UnityEvent OnStart;
    [SerializeField] private UnityEvent OnComplete;

    public float timeScale = 1;

    private Sequence sequence;
    private Vector3 startScale;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Color startColor;
    private bool isReversed;

    private void OnEnable()
    {
        RectTransform rectTransform = transform.AsRect();
        startPosition = rectTransform != null ? rectTransform.anchoredPosition : transform.localPosition;
        startRotation = transform.localRotation;
        startScale = transform.localScale;

        if (Type == TweenType.Color)
        {
            if (transform.TryGetComponent<Renderer>(out var renderer))
                startColor = renderer.sharedMaterial.color;
            else if (transform.TryGetComponent<Graphic>(out var graphic))
                startColor = graphic.color;
        }

        if (AutoPlay)
        {
            PlaySequence();
        }
    }

    public void Play()
    {
        if (sequence != null || !enabled) //|| sequence.IsPlaying()
            return;

        PlaySequence();
    }

    public void PlayReverse()
    {
        if (!enabled)
            return;

        sequence?.Kill();
        ResetTransform();
        PlaySequence().SetInverted();
        isReversed = true;
    }

    public void Stop()
    {
        if (sequence == null)
            return;

        sequence.Kill(true);
        ResetTransform();
    }

    public void SetTimeScale(float timeScale)
    {
        this.timeScale = timeScale;
    }

    public Tween PlaySequence()
    {
        RectTransform rectTransform = transform.AsRect();
        sequence = DOTween.Sequence();
        sequence.timeScale = timeScale;
        isReversed = false;

        switch (Type)
        {
            case TweenType.Translate:
                if (Space == Space.World)
                    sequence.Append(transform.DOMove(transform.position + EndValue, Duration));
                else if (rectTransform != null)
                    sequence.Append(rectTransform.DOAnchorPos(rectTransform.anchoredPosition + new Vector2(EndValue.x, EndValue.y), Duration));
                else
                    sequence.Append(transform.DOLocalMove(transform.localPosition + EndValue, Duration));
                break;

            case TweenType.Position:
                if (Space == Space.World)
                    sequence.Append(transform.DOMove(EndValue, Duration));
                else if (rectTransform != null)
                    sequence.Append(rectTransform.DOAnchorPos(EndValue, Duration));
                else
                    sequence.Append(transform.DOLocalMove(EndValue, Duration));
                break;

            case TweenType.Scale:
                sequence.Append(transform.DOScale(EndValue, Duration));
                break;

            case TweenType.Rotate:
                if (Space == Space.World)
                    sequence.Append(transform.DORotate(EndValue, Duration, RotateMode.WorldAxisAdd));
                else
                    sequence.Append(transform.DOLocalRotate(EndValue, Duration));
                break;

            case TweenType.Color:
                if (transform.TryGetComponent<Renderer>(out var renderer))
                    sequence.Join(renderer.material.DOColor(EndColor, Duration));
                else if (transform.TryGetComponent<Graphic>(out var graphic))
                    sequence.Join(graphic.DOColor(EndColor, Duration));
                break;
        }

        ApplyAdditionalOptions(sequence);

        OnStart.Invoke();
        return sequence;
    }

    private void ApplyAdditionalOptions(Tween sequence)
    {
        if (Loop)
            sequence.SetLoops(LoopCount, LoopType);

        if (IgnoreTimeScale)
            sequence.SetUpdate(true);

        if (Delay > 0)
            sequence.SetDelay(Delay, false);

        sequence.SetEase(Ease);
        sequence.OnComplete(OnTweenComplete);
    }

    private void OnTweenComplete()
    {
        OnComplete.Invoke();

        if (DestroyOnComplete && !isReversed)
        {
            Destroy(gameObject);
        }

        sequence = null;
    }

    private void OnDisable()
    {
        if (gameObject.activeInHierarchy) // Don't clean up if only behaviour is disabled
            return;

        sequence?.Kill(true);

        if (ResetOnDisable || sequence != null)
            ResetTransform();
    }

    private void ResetTransform()
    {
        sequence = null;

        switch (Type)
        {
            case TweenType.Position:
            case TweenType.Translate:
                RectTransform rectTransform = transform.AsRect();
                if (rectTransform != null)
                    rectTransform.anchoredPosition = startPosition;
                else
                    transform.localPosition = startPosition;

                break;
            case TweenType.Scale:
                transform.localScale = startScale;
                break;
            case TweenType.Rotate:
                transform.localRotation = startRotation;
                break;
            case TweenType.Color:
                // if (transform.TryGetComponent<Renderer>(out var renderer))
                //     startColor = renderer.sharedMaterial.color;
                // else
                if (transform.TryGetComponent<Graphic>(out var graphic))
                    graphic.color = startColor;
                break;
            default:
                break;
        }
    }
}

public enum TweenType
{
    Translate,
    Position,
    Scale,
    Rotate,
    Color
}
#endif
