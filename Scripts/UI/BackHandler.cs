using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace CLGameToolkit.UI
{
    public static class BackHandler
    {
        private static InputAction cancelAction;

        private static readonly List<Func<bool>> BackHandlers = new();

        private static void OnBack(InputAction.CallbackContext context)
        {
            GoBack();
        }

        public static void GoBack()
        {
            for (int i = BackHandlers.Count - 1; i >= 0; i--)
            {
                if (BackHandlers[i]?.Invoke() == true)
                    break; // Stop as soon as one handler consumes it
            }
        }

        public static void Register(Func<bool> handler)
        {
            Init();

            if (!BackHandlers.Contains(handler))
                BackHandlers.Add(handler);
        }

        public static void Unregister(Func<bool> handler)
        {
            BackHandlers.Remove(handler);
        }

        private static void Init()
        {
            if (cancelAction != null)
                return;

            cancelAction = (EventSystem.current.currentInputModule as InputSystemUIInputModule).cancel.action;
            cancelAction.performed += OnBack; // TODO: Global atm, i.e no unregister.
        }

        public static void Reset()
        {
            BackHandlers.Clear();
            if (cancelAction != null)
                cancelAction.performed -= OnBack;
            cancelAction = null;
        }

    }
}
