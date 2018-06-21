using UnityEngine.EventSystems;

namespace UnityCommon
{
    public abstract class ScriptableUIComponent<T> : ScriptableUIBehaviour where T : UIBehaviour
    {
        public T UIComponent
        {
            get
            {
                if (!_uiComponent)
                    _uiComponent = GetComponent<T>();
                return _uiComponent;
            }
        }

        private T _uiComponent;
    }
}
