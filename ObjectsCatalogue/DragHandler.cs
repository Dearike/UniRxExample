using UnityEngine;
using UnityEngine.EventSystems;

namespace Planner.MainEditor.ObjectsCatalogue
{
    /*!
        \ingroup  mainEditorObjectCatalog
    */
    public class DragHandler : MonoBehaviour, IDragHandler
    {
        public void OnDrag (PointerEventData eventData)
        {
            Debug.Log ("Drag");
        }
    }
}