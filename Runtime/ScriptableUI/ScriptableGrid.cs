using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UnityCommon
{
    [RequireComponent(typeof(GridLayoutGroup))]
    public abstract class ScriptableGrid<TSlot> : ScriptableUIComponent<GridLayoutGroup> where TSlot : ScriptableGridSlot
    {
        public TSlot SlotPrototype => slotPrototype;
        public int SlotCount => SlotsMap.Count;
        public int PageCount => Mathf.CeilToInt(transform.childCount / (float)slotsPerPage);

        protected Dictionary<string, TSlot> SlotsMap { get; private set; } = new Dictionary<string, TSlot>();
        protected int CurrentPage { get; private set; } = 1;

        [SerializeField] private TSlot slotPrototype = null;

        [Header("Pagination")]
        [SerializeField] private int slotsPerPage = 9;
        [SerializeField] private GameObject paginationPanel = null;
        [SerializeField] private Text pageNumberText = null;
        [SerializeField] private ScriptableButton previousPageButton = null;
        [SerializeField] private ScriptableButton nextPageButton = null;

        public virtual void AddSlot (TSlot slot)
        {
            if (SlotExists(slot.Id)) return;

            slot.RectTransform.SetParent(transform, false);
            SlotsMap.Add(slot.Id, slot);

            Paginate();
        }

        public virtual void RemoveSlot (string slotId)
        {
            if (!SlotExists(slotId)) return;

            var slot = SlotsMap[slotId];
            ObjectUtils.DestroyOrImmediate(slot.gameObject);
            SlotsMap.Remove(slotId);

            Paginate();
        }

        public virtual void RemoveAllSlots ()
        {
            var slotIds = SlotsMap.Values.Select(slot => slot.Id).ToList();
            foreach (var slotId in slotIds)
                RemoveSlot(slotId);
        }

        public virtual TSlot GetSlot (string slotId) => SlotsMap.TryGetValue(slotId, out var slot) ? slot : null;

        public virtual List<TSlot> GetAllSlots () => SlotsMap.Values.ToList();

        public virtual bool SlotExists (string slotId) => SlotsMap.ContainsKey(slotId);

        protected override void Awake ()
        {
            base.Awake();

            this.AssertRequiredObjects(slotPrototype, paginationPanel, pageNumberText, previousPageButton, nextPageButton);

            pageNumberText.text = "1";
            previousPageButton.OnButtonClicked += SelectPreviousPage;
            nextPageButton.OnButtonClicked += SelectNextPage;

            Paginate();
        }

        protected virtual void Paginate ()
        {
            if (PageCount < 2) { paginationPanel.SetActive(false); return; }

            paginationPanel.SetActive(true);

            var endIndex = CurrentPage * slotsPerPage;
            var startIndex = endIndex - slotsPerPage + 1;
            foreach (var slot in SlotsMap.Values)
            {
                var isActive = slot.NumberInGrid.IsWithin(startIndex, endIndex);
                slot.gameObject.SetActive(isActive);
            }

            previousPageButton.SetIsInteractable(CurrentPage > 1);
            nextPageButton.SetIsInteractable(CurrentPage < PageCount);
        }

        protected virtual void SelectPreviousPage ()
        {
            if (CurrentPage == 1) return;

            CurrentPage--;
            pageNumberText.text = CurrentPage.ToString();
            Paginate();
        }

        protected virtual void SelectNextPage ()
        {
            if (CurrentPage == PageCount) return;

            CurrentPage++;
            pageNumberText.text = CurrentPage.ToString();
            Paginate();
        }
    }
}
