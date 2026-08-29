using System;
using System.Collections.Generic;
using UnityEngine;

namespace CLGameToolkit.Timers
{
    public class Timer : PersistedMonoSingleton<Timer>
    {
        private readonly List<TimerData> activeTimers = new();
        private readonly Stack<TimerData> timerPool = new(32);

        static Timer()
        {
            AllowCreation = true;
        }

        public static TimerData Delay(float delay, Action callback, bool useTimeScale = true)
        {
            TimerData timer = Instance.GetFromPool();
            timer.Start((useTimeScale ? Time.time : Time.unscaledTime) + delay, callback, useTimeScale);

            Instance.activeTimers.Add(timer);
            return timer;
        }

        public static TimerData Interval(float interval, Action callback, bool instant = false, bool useTimeScale = true)
        {
            if (interval <= 0f)
                throw new ArgumentOutOfRangeException(nameof(interval), interval, "Interval must be greater than zero.");

            TimerData timer = Instance.GetFromPool();
            timer.StartInterval((useTimeScale ? Time.time : Time.unscaledTime) + (instant ? 0 : interval), interval, callback, useTimeScale);

            Instance.activeTimers.Add(timer);
            return timer;
        }

        private void Update()
        {
            float time = Time.time;
            float unscaledTime = Time.unscaledTime;

            for (int i = activeTimers.Count - 1; i >= 0; i--)
            {
                TimerData call = activeTimers[i];

                if (call.isCancelled)
                {
                    ReturnToPool(call);
                    activeTimers.RemoveAt(i);
                    continue;
                }

                float currentTime = call.useTimeScale ? time : unscaledTime;

                if (currentTime >= call.targetTime)
                {
                    call.callback?.Invoke();

                    if (call.isRepeating)
                    {
                        call.targetTime += call.interval;
                    }
                    else
                    {
                        activeTimers.RemoveAt(i);
                        ReturnToPool(call);
                    }
                }
            }
        }

        private TimerData GetFromPool()
        {
            return timerPool.Count > 0 ? timerPool.Pop() : new TimerData();
        }

        private void ReturnToPool(TimerData call)
        {
            timerPool.Push(call);
        }
    }

    public class TimerData
    {
        internal float targetTime;
        internal float interval;
        internal Action callback;
        internal bool useTimeScale;
        internal bool isCancelled;
        internal bool isRepeating;

        internal void Start(float targetTime, Action callback, bool respectTimeScale)
        {
            this.targetTime = targetTime;
            this.callback = callback;
            this.useTimeScale = respectTimeScale;
            this.isCancelled = false;
            this.isRepeating = false;
        }

        internal void StartInterval(float targetTime, float interval, Action callback, bool respectTimeScale)
        {
            this.targetTime = targetTime;
            this.interval = interval;
            this.callback = callback;
            this.useTimeScale = respectTimeScale;
            this.isCancelled = false;
            this.isRepeating = true;
        }

        public void Stop(bool complete = false)
        {
            isCancelled = true;

            if (complete)
                callback?.Invoke();
        }
    }
}
