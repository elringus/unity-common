using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityCommon
{
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public class ScriptableGridSlot : ScriptableButton, IPointerEnterHandler, IPointerExitHandler
    { 
        public class Constructor<TSlot> where TSlot : ScriptableGridSlot
        {
            public readonly TSlot ConstructedSlot;

            public Constructor (TSlot prototype, string id, OnClicked onClicked = null)
            {
                ConstructedSlot = Instantiate(prototype);
                ConstructedSlot.Id = id;
                ConstructedSlot.onClickedAction = onClicked;
            }
        }

        /// <summary>
        /// Action to invoke when the slot body is clicked by the user.
        /// </summary>
        /// <param name="slotId">ID of the clicked slot.</param>
        public delegate void OnClicked (string slotId);

        public string Id { get; private set; }
        public int NumberInGrid => transform.GetSiblingIndex() + 1;

        private OnClicked onClickedAction;

        public virtual void OnPointerEnter (PointerEventData eventData)
        {
            
        }

        public virtual void OnPointerExit (PointerEventData eventData)
        {
            
        }

        protected override void OnButtonClick ()
        {
            base.OnButtonClick();

            onClickedAction?.Invoke(Id);
        }
    }
}
