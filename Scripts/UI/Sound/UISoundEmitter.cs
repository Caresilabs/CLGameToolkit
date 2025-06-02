using UnityEngine;
using UnityEngine.EventSystems;

namespace CLGameToolkit.UI
{
    public class UISoundEmitter : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private UISoundManager.UISoundType SelectType = UISoundManager.UISoundType.Select;
        [SerializeField] private UISoundManager.UISoundType HoverType = UISoundManager.UISoundType.Hover;
        [SerializeField] private UISoundManager.UISoundType ClickType = UISoundManager.UISoundType.Click;

        private bool isSelecting;

        public void OnPointerEnter(PointerEventData eventData)
        {
            UISoundManager.Instance.Play(HoverType);
        }

        public void OnSelect(BaseEventData eventData)
        {
            UISoundManager.Instance.Play(SelectType);
            isSelecting = true;
        }

        public void OnPointerDown(PointerEventData eventData)
        {

        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isSelecting)
            {
                UISoundManager.Instance.Play(ClickType);
            }

            isSelecting = false;
        }
    }
}
