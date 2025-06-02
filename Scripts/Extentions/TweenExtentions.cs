#if DOTWEEN
using DG.Tweening;
using TMPro;
using UnityEngine;

public static class TweenExtentions
{
    public static Tween DONumber(this TMP_Text text, int endValue, float duration)
    {
        int.TryParse(text.text, out int startValue);

        return DOVirtual.Float(0, 1, duration, (alpha) => {
            text.text = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, alpha)).ToString();
        });
    }

    public static Tween FadeTimeScale(float timeScale, float duration = .5f)
    {
        return DOTween.To(() => Time.timeScale, x => Time.timeScale = x, timeScale, duration)
            .SetEase(Ease.InQuad)
            .SetUpdate(true);
    }

}
#endif
