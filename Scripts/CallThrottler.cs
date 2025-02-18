using UnityEngine;

public class CallThrottler
{
    private readonly float maxRate;
    private readonly bool ignoreTimeScale;

    private float nextCallTime;

    public CallThrottler(float maxRateSeconds, bool ignoreTimeScale = false)
    {
        this.maxRate = maxRateSeconds;
        this.ignoreTimeScale = ignoreTimeScale;
    }

    public bool CanCall()
    {
        float time = CurrentTime;
        if (time >= nextCallTime)
        {
            nextCallTime = time + maxRate;
            return true;
        }

        return false;
    }

    /// <summary>
    /// No side effect
    /// </summary>
    /// <returns></returns>
    public bool IsReady()
    {
        return CurrentTime >= nextCallTime;
    }

    public void Reset(bool resetToNextCall = true)
    {
        nextCallTime = CurrentTime + (resetToNextCall ? maxRate : 0);
    }

    public void Reset(float cooldown)
    {
        nextCallTime = CurrentTime + cooldown;
    }

    private float CurrentTime => ignoreTimeScale ? Time.unscaledTime : Time.time;
}
