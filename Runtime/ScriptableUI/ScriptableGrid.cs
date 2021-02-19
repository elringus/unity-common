using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UnityCommon
{
    [RequireComponent(typeof(GridLayoutGroup))]
    public abstract class ScriptableGrid<TSlot> : ScriptableUIComponent<GridLayoutGroup>
        where TSlot : ScriptableGridSlot
    {
        [Serializable]
        private class OnPageChangedEvent : UnityEvent<int> { }

        public virtual IReadOnlyList<TSlot> Slots { get; private set; }
        public virtual int CurrentPage { get; private set; } = 1;
        public virtual int PageCount => Mathf.CeilToInt(SourceList.Count / (float)SlotsPerPage);
        public virtual int SlotsPerPage => slotsPerPage;

        protected abstract IList SourceList { get; }
        protected virtual TSlot SlotPrototype => slotPrototype;
        protected virtual GameObject PaginationPanel => paginationPanel;
        protected virtual Button PreviousPageButton => previousPageButton;
        protected virtual Button NextPageButton => nextPageButton;

        [Tooltip("Prefab representing grid slot.")]
        [SerializeField] private TSlot slotPrototype = null;
        [Tooltip("How many slots should be visible per page."), Range(1, 99)]
        [SerializeField] private int slotsPerPage = 9;
        [Tooltip("Container for the page number controls (optional). Will be disabled when grid has only one page.")]
        [SerializeField] private GameObject paginationPanel = null;
        [Tooltip("Button inside pagination panel to select next grid page.")]
        [SerializeField] private Button previousPageButton = null;
        [Tooltip("Button inside pagination panel to select previous grid page.")]
        [SerializeField] private Button nextPageButton = null;
        [Tooltip("Event invoked when grid page number changes.")]
        [SerializeField] private OnPageChangedEvent onPageChanged = default;

        /// <summary>
        /// Attempts to select grid page with the specified number (starting with 1).
        /// </summary>
        public virtual void SelectPage (int pageNumber)
        {
            if (pageNumber == CurrentPage) return;
            if (pageNumber < 1 || pageNumber > PageCount)
                throw new ArgumentOutOfRangeException(nameof(pageNumber), $"Page number should be between 1 and {PageCount}.");
            CurrentPage = pageNumber;
            Paginate();
            onPageChanged?.Invoke(pageNumber);
        }

        /// <summary>
        /// Attempts to select next grid page; no effect when last page is selected.
        /// </summary>
        public virtual void SelectNextPage ()
        {
            if (CurrentPage == PageCount) return;
            SelectPage(CurrentPage + 1);
        }

        /// <summary>
        /// Attempts to select previous grid page; no effect when first page is selected.
        /// </summary>
        public virtual void SelectPreviousPage ()
        {
            if (CurrentPage == 1) return;
            SelectPage(CurrentPage - 1);
        }

        protected override void Awake ()
        {
            base.Awake();
            this.AssertRequiredObjects(SlotPrototype);

            Slots = PopulateGrid();
            FocusOnNavigation = Slots[Slots.Count - 1].gameObject;
            if (PreviousPageButton)
                PreviousPageButton.onClick.AddListener(SelectPreviousPage);
            if (NextPageButton)
                NextPageButton.onClick.AddListener(SelectNextPage);
            Paginate();
        }

        protected virtual TSlot[] PopulateGrid ()
        {
            var slots = new TSlot[SlotsPerPage];
            for (int i = 0; i < SlotsPerPage; i++)
            {
                slots[i] = InstantiateSlot();
                slots[i].RectTransform.SetParent(transform, false);
            }
            return slots;
        }

        /// <summary>
        /// Creates a new instance of the slot prototype.
        /// Invoked to populate the grid on initialization.
        /// </summary>
        protected abstract TSlot InstantiateSlot ();

        /// <summary>
        /// Binds the slot to the specified <see cref="SourceList"/> index.
        /// Invoked on pagination to re-use instantiated slot objects.
        /// </summary>
        protected abstract void BindSlot (TSlot slot, int sourceIndex);

        protected virtual void Paginate ()
        {
            if (PageCount < 2)
            {
                if (PaginationPanel)
                    PaginationPanel.SetActive(false);
                return;
            }

            if (PaginationPanel)
                PaginationPanel.SetActive(true);

            for (int slotIndex = 0; slotIndex < SlotsPerPage; slotIndex++)
            {
                var sourceIndex = (CurrentPage - 1) * SlotsPerPage + slotIndex;
                BindSlot(Slots[slotIndex], sourceIndex);
            }

            if (PreviousPageButton)
                PreviousPageButton.interactable = CurrentPage > 1;
            if (NextPageButton)
                NextPageButton.interactable = CurrentPage < PageCount;
        }
    }
}
