using UnityEngine.UI;

namespace CLGameToolkit.UI
{
    public class BackButton : Button
    {
        protected override void Awake()
        {
            base.Awake();
            onClick.AddListener(BackHandler.GoBack);
        }
    }
}
