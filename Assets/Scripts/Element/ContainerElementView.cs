using UnityEngine.EventSystems;

namespace Element
{
    public class ContainerElementView : ElementView, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public virtual void SetTransparency(float alpha)
        {
            //if (canvasGroup != null)
                //canvasGroup.alpha = alpha;
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
        }

        public void OnDrag(PointerEventData eventData)
        {
            
        }
    }
}