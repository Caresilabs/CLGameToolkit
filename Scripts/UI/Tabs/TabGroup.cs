using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CLGameToolkit.UI
{
    public class TabGroup : MonoBehaviour
    {
        [SerializeField] private GameObject TabContainer;
        [SerializeField] private bool RememberLastTab = true;

        [SerializeField] private AudioClip TabSound;

        [Header("Input")]
        [SerializeField] private InputActionReference InputCycleAction;

        public int TabIndex { get; private set; }

        public System.Action OnTabChanged;

        private readonly List<TabButton> tabButtons = new();
        private List<GameObject> tabPages;
        private TabButton selectedTab;

        private void Awake()
        {
            tabPages = new(TabContainer.transform.childCount);

            foreach (Transform transform in TabContainer.transform)
                tabPages.Add(transform.gameObject);

            foreach (GameObject page in tabPages)
                page.SetActive(page == tabPages[0]); // Disable all but first
        }

        protected void Start()
        {
            // Hack to force layout update
            Canvas.ForceUpdateCanvases();

            foreach (TabButton tabButton in tabButtons)
            {
                int index = tabButtons.IndexOf(tabButton);

                // Don't allow selection
                tabButton.navigation = new Navigation() { mode = Navigation.Mode.None };

                if (index == TabIndex)
                    OnTabSelected(tabButton, true);
            }
        }

        protected void OnEnable()
        {
            // Resect tab when re-open
            if (selectedTab != null)
            {
                var oldTab = selectedTab;
                selectedTab = null;
                OnTabSelected(oldTab, true);
            }

            InputCycleAction.action.performed += OnCycleTabs;
        }

        private void OnDisable()
        {
            InputCycleAction.action.performed -= OnCycleTabs;

            // Disable active page, so that we can do a refresh in correct order if opened again
            if (selectedTab != null)
            {
                tabPages[TabIndex].SetActive(false);
                selectedTab.Deselect();
            }

            if (!RememberLastTab)
                selectedTab = tabButtons.FirstOrDefault();
        }

        private void OnCycleTabs(InputAction.CallbackContext context)
        {
            if (context.ReadValue<float>() < 0)
            {
                PreviousTab();
            }
            else
            {
                NextTab();
            }
        }

        public void Subscribe(TabButton tabButton)
        {
            tabButtons.Add(tabButton);
            tabButtons.Sort((x, y) => x.transform.GetSiblingIndex().CompareTo(y.transform.GetSiblingIndex()));     // Sort by order in hierarchy
        }

        public void OnTabEnter(TabButton tabButton)
        {

        }

        public void OnTabExit(TabButton tabButton)
        {

        }

        public void OnTabSelected(TabButton tabButton, bool isInitial = false)
        {
            if (tabButton == selectedTab)
                return;

            if (selectedTab != null)
            {
                selectedTab.Deselect();
            }

            selectedTab = tabButton;

            TabIndex = tabButtons.IndexOf(tabButton);
            for (int i = 0; i < tabPages.Count; i++)
            {
                tabPages[i].SetActive(i == TabIndex);
            }

            selectedTab.SelectTab();

            // Update focus
            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
            bool shouldFocusTab = currentSelected == null || !currentSelected.activeInHierarchy || !currentSelected.transform.IsChildOf(tabPages[TabIndex].transform);
            /*if (shouldFocusTab)
            {
                GlobalInputManager.UpdateSelectables(tabPages[TabIndex]);
            }*/

            OnTabChanged?.Invoke();

            if (!isInitial)
                AudioManager.PlayUI(TabSound);
        }

        public void SetTab(int index)
        {
            if (tabPages == null) // We haven't loaded all tabs yet
            {
                TabIndex = index;
                return;
            }

            OnTabSelected(tabButtons[index]);
        }

        public void NextTab()
        {
            int nextIndex = TabIndex < tabButtons.Count - 1 ? TabIndex + 1 : 0;
            OnTabSelected(tabButtons[nextIndex]);
        }

        public void PreviousTab()
        {
            int previousIndex = TabIndex > 0 ? TabIndex - 1 : tabButtons.Count - 1;
            OnTabSelected(tabButtons[previousIndex]);
        }

    }
}
