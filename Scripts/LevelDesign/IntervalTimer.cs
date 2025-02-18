using UnityEngine;
using UnityEngine.Events;

public class IntervalTimer : MonoBehaviour
{
    [SerializeField] private float Delay;
    [SerializeField] private float Interval;
    [Space, SerializeField] private UnityEvent OnTick;

    void OnEnable()
    {
        if (Interval > 0)
            InvokeRepeating(nameof(Tick), Delay, Interval);
        else
            Invoke(nameof(Tick), Delay);
    }

    void Tick()
    {
        OnTick.Invoke();
    }

    void OnDisable()
    {
        CancelInvoke(nameof(Tick));
    }

    public void SetInterval(float interval)
    {
        this.Interval = interval;
        CancelInvoke(nameof(Tick));
        InvokeRepeating(nameof(Tick), Delay, Interval);
    }
}
