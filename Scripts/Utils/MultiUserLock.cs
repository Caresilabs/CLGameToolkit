using System;

public class MultiUserLock
{
    private int lockCount = 0;
    private readonly Action onLock;
    private readonly Action onUnlocked;

    public bool IsLocked => lockCount > 0;

    public MultiUserLock(Action onLock, Action onUnlocked)
    {
        this.onLock = onLock;
        this.onUnlocked = onUnlocked;
    }

    public void Lock()
    {
        if (lockCount == 0)
            onLock?.Invoke();

        lockCount++;
    }

    public void Unlock()
    {
        if (lockCount <= 0)
            return;

        lockCount--;
        if (lockCount == 0)
        {
            onUnlocked?.Invoke();
        }
    }
}
