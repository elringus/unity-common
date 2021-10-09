using UnityEngine.EventSystems;

namespace UnityCommon
{
    public abstract class ScriptableUIComponent<T> : ScriptableUIBehaviour where T : UIBehaviour
    {
        public virtual T UIComponent => uiComponent ? uiComponent : uiComponent = GetComponent<T>();

        private T uiComponent;
    }
}
