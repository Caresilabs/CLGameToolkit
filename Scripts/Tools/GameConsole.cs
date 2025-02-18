using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Cheat console for dev and mods
/// 
/// Note: everything is case insensitive!
/// </summary>
public class GameConsole : MonoBehaviour
{
    public static bool IsActive => showConsole;

    public Action<bool> OnToggleConsole;

    private readonly List<string> logs = new();
    private readonly Dictionary<string, Action<string[]>> commands = new();
    private readonly Dictionary<string, string[]> commandParams = new();

    private static bool showConsole = false;
    private Rect windowRect;
    private bool focusInputField;
    private Vector2 scrollPosition;
    private string input = string.Empty;

    private readonly string[] defaultInputArray = new string[1];
    private static readonly List<string> history = new(); // Persistent history
    private int historyIndex = 0;

    private void Awake()
    {
        windowRect = new(16, 16, Screen.height * .65f, Screen.height * .35f);

        AddCommand("help", ShowHelp);
        AddCommand("clear", ClearLogs);

        Application.logMessageReceived += HandleLog;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    public GameConsole AddCommand(string command, Action<string[]> func)
    {
        commands.Add(command.ToLower(), func);
        return this;
    }

    public GameConsole AddCommand(string command, string[] availableParams, Action<string[]> func)
    {
        for (int i = 0; i < availableParams.Length; i++)
            availableParams[i] = availableParams[i].ToLower();

        commands.Add(command.ToLower(), func);
        commandParams.Add(command.ToLower(), availableParams);

        return this;
    }

    void HandleLog(string message, string stackTrace, LogType type)
    {
        if (type != LogType.Log)
            logs.Add($"[{type}] {message} => {stackTrace}");
        else
            logs.Add($"[{type}] {message}");

        scrollPosition.y = float.MaxValue;
        CheckCleanOldLogs();
    }

    private void Update()
    {
        // Toggle the console with the backtick key (`) (key above Tab)
        if (Keyboard.current.backquoteKey.wasPressedThisFrame)
        {
            showConsole = !showConsole;

            if (showConsole)
            {
                Cursor.lockState = CursorLockMode.None;
                focusInputField = true;

                if (history.Count > 20)
                    history.RemoveRange(0, history.Count - 20);
            }
            else
            {
                input = string.Empty;
            }

            OnToggleConsole?.Invoke(showConsole);
        }
    }

    private void OnGUI()
    {
        if (!showConsole)
            return;

        HandleKeybinds();

        windowRect = GUILayout.Window(433266, windowRect, ConsoleWindow, "Console");
    }

    private void HandleKeybinds()
    {
        if (Event.current.type == EventType.KeyUp)
        {
            if (Event.current.keyCode == KeyCode.Return)
            {
                HandleInput();
                input = string.Empty;
            }

            if (Event.current.keyCode == KeyCode.Tab)
            {
                HandleAutoCorrect();
            }

            // TODO: Clean up
            if (Event.current.keyCode == KeyCode.UpArrow)
            {
                focusInputField = true;
                input = history.Count > 0 ?
                    history[MathfExtentions.CyclePostitive(history.Count - ++historyIndex, history.Count)]
                    : string.Empty;
            }
            else if (Event.current.keyCode == KeyCode.DownArrow)
            {
                focusInputField = true;
                historyIndex = Mathf.Max(historyIndex - 1, 0);
                if (historyIndex == 0)
                    input = string.Empty;
                else
                    input = history[historyIndex];
            }
        }
    }

    private void HandleAutoCorrect()
    {
        if (string.IsNullOrEmpty(input))
            return;

        string[] currentInput = input.ToLower().Split(' ');
        var availableParam = commandParams.GetValueOrDefault(currentInput[0]);
        var matches = commands.Keys.Where(x => x.StartsWith(currentInput[0]));
        var matchCount = matches.Count();

        if (availableParam != null) // Autocorrect param
        {
            string currentParam = currentInput.Length == 1 ? string.Empty : currentInput[1];
            int indexOfMatch = Array.FindIndex(availableParam, x => x.StartsWith(currentParam));
            bool isFullMatch = indexOfMatch >= 0 && availableParam[indexOfMatch] == currentParam;
            int newIndex = MathfExtentions.CyclePostitive(isFullMatch ? indexOfMatch + 1 : Mathf.Max(indexOfMatch, 0), availableParam.Length);

            input = currentInput[0] + " " + availableParam[newIndex];
        }
        else if (matchCount == 1)
        {
            input = matches.FirstOrDefault();
        }
        else if (matchCount > 1)
        {
            logs.Add("> " + string.Join('\n', matches));
        }

        focusInputField = true;
        scrollPosition.y = float.MaxValue;
    }

    private void ConsoleWindow(int windowID)
    {
        GUILayout.BeginVertical();

        // Display the logs
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        foreach (string log in logs)
        {
            GUILayout.Label(log);
        }
        GUILayout.EndScrollView();

        // Input text field
        GUILayout.BeginHorizontal();

        GUI.SetNextControlName("ConsoleInputField");
        input = GUILayout.TextField(input, GUILayout.ExpandWidth(true));

        if (focusInputField && Event.current.type == EventType.Repaint)
        {
            GUI.FocusControl("ConsoleInputField");
            focusInputField = false;

            TextEditor tex = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            tex.text = input;
            tex.cursorIndex = input.Length;
            tex.selectIndex = input.Length;
        }

        if (GUILayout.Button("Submit", GUILayout.ExpandWidth(false)))
        {
            HandleInput();
            input = string.Empty;
            GUI.FocusControl("ConsoleInputField"); // Keep focus after submitting
        }

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        // Make the window draggable
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    private void HandleInput()
    {
        if (string.IsNullOrEmpty(input))
            return;

        logs.Add("> " + input);
        string[] splitInput = input.ToLower().Split(' ');
        string command = splitInput[0];

        if (commands.ContainsKey(command))
        {
            commands[command](splitInput.Length > 1 ? splitInput.Skip(1).ToArray() : defaultInputArray);

            if (history.Count == 0 || history[^1] != input)
                history.Add(input);

            if (history.Count > 10)
                history.RemoveRange(0, 10);
        }
        else
        {
            logs.Add("Unknown command: " + command);
        }

        historyIndex = 0;
        scrollPosition.y = float.MaxValue;

        CheckCleanOldLogs();
    }

    private void ShowHelp(string[] _)
    {
        logs.Add("Available commands:");
        logs.Add("- " + string.Join("\n- ", commands.Keys));
    }

    private void ClearLogs(string[] _)
    {
        logs.Clear();
    }

    private void CheckCleanOldLogs()
    {
        if (logs.Count > 200)
            logs.RemoveRange(0, 200);
    }

}
