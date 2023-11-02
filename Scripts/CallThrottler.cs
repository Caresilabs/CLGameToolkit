using UnityEngine;

public class CallThrottler
{
    private readonly float maxRate;

    private float nextCallTime;

    public CallThrottler(float maxRateSeconds)
    {
        maxRate = maxRateSeconds;
    }

    public bool CanCall()
    {
        if (Time.time > nextCallTime)
        {
            nextCallTime = Time.time + maxRate;
            return true;
        }

        return false;
    }
}
