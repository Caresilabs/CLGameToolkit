
#if UNITY_EDITOR
#define LOGGER_VERBOSE
#endif

using UnityEngine;
using System.Diagnostics;

public static class Logger
{
    [Conditional("UNITY_EDITOR")]
    public static void Debug(string message, Object obj = null)
    {
        UnityEngine.Debug.Log($"{GetTime()} {message}", obj);
    }

    [Conditional("UNITY_EDITOR")]
    public static void Info(string message, Object obj = null)
    {
        UnityEngine.Debug.Log($"{GetTime()} {message}", obj);
    }

    public static void Warn(string message, Object obj = null)
    {
        UnityEngine.Debug.LogWarning($"{GetTime()} {message}", obj);
    }

    public static void Error(string message, Object obj = null)
    {
        UnityEngine.Debug.LogError($"{GetTime()} {message}", obj);
    }

    public static void Exception(System.Exception e)
    {
        UnityEngine.Debug.LogError($"{GetTime()} {e.Message}");
    }

    private static string GetTime()
    {
        return System.DateTime.Now.ToString("[hh:mm:ss.fff]");
    }
}
