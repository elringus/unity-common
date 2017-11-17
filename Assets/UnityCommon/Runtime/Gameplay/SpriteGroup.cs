using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SpriteGroupType
{
    Independent, // items will simultaneously crossfade
    Stacked // selected item will be placed on top of the stack, faded-in, then previous one will be hidden
}

public class SpriteGroup : MonoBehaviour
{
    const float STACK_Z_STEP = .0001f;

    public string Name { get { return name; } }
    public string SelectedItemName { get { return selectedItemName; } }
    public SpriteRenderer DefaultItem { get { return _defaultItem; } private set { _defaultItem = value; } }
    public SpriteGroupType SpriteGroupType { get { return _spriteGroupType; } private set { _spriteGroupType = value; } }

    private Tweener<FloatTween> stackedSpriteTweener;
    private List<SpriteRenderer> items = new List<SpriteRenderer>();
    private string selectedItemName;
    private bool isHidden;
    private int zStackDepth;

    [SerializeField] private SpriteRenderer _defaultItem;
    [SerializeField] private SpriteGroupType _spriteGroupType;

    private void Awake ()
    {
        Debug.Assert(transform.parent);

        items.AddRange(this.GetComponentsOnChildren<SpriteRenderer>());
        items.ForEach(sprite => sprite.SetOpacity(0f));

        if (!DefaultItem) DefaultItem = GetItemByName("default");
        else Debug.Assert(DefaultItem.transform.IsChildOf(transform));

        if (string.IsNullOrEmpty(selectedItemName) && DefaultItem)
            selectedItemName = DefaultItem.name;
    }

    public void SetIsHidden (bool isHidden, float fadeTime, Action onComplete = null)
    {
        if (this.isHidden == isHidden) { onComplete.SafeInvoke(); return; }

        this.isHidden = isHidden;
        FadeSprites(fadeTime, onComplete);
    }

    public void SelectItem (string itemName, float fadeTime, Action onComplete = null)
    {
        if (selectedItemName == itemName) { onComplete.SafeInvoke(); return; }

        selectedItemName = itemName;
        FadeSprites(fadeTime, onComplete);
    }

    public SpriteRenderer GetItemByName (string itemName)
    {
        return items.Find(i => i.gameObject.name.LEquals(itemName));
    }

    public List<SpriteRenderer> GetAllItemsExcept (string exceptItemName)
    {
        return items.Where(item => item.gameObject.name.ToLower() != exceptItemName.ToLower()).ToList();
    }

    private void FadeSprites (float fadeTime, Action onComplete = null)
    {
        if (isHidden || string.IsNullOrEmpty(selectedItemName) || selectedItemName.LEquals("none"))
        {
            using (var waitHandle = new UnityEventWaitHandle(WaitFor.AllEvents, onComplete))
                foreach (var item in items)
                    item.FadeOut(fadeTime, this, waitHandle.Wait());
        }
        else
        {
            var selectedSprite = GetItemByName(selectedItemName);
            Debug.Assert(selectedSprite, string.Format("Item {0} not found in group {1}.", selectedItemName, Name));

            if (SpriteGroupType == SpriteGroupType.Independent)
            {
                using (var waitHandle = new UnityEventWaitHandle(WaitFor.AllEvents, onComplete))
                {
                    foreach (var item in GetAllItemsExcept(selectedItemName))
                        item.FadeOut(fadeTime, this, waitHandle.Wait());
                    selectedSprite.FadeIn(fadeTime, this, waitHandle.Wait());
                }
            }
            else
            {
                zStackDepth++;
                var initialLocalPosZ = selectedSprite.transform.localPosition.z;
                selectedSprite.transform.AddPosZ(-STACK_Z_STEP * zStackDepth);

                // To prevent concurrency problems.
                var selectedItemNameCopy = selectedItemName;
                if (stackedSpriteTweener != null && !stackedSpriteTweener.IsComplete)
                    stackedSpriteTweener.CompleteInstantly();

                stackedSpriteTweener = selectedSprite.FadeIn(fadeTime, this, () => {
                    foreach (var item in GetAllItemsExcept(selectedItemNameCopy))
                        item.SetOpacity(0f);
                    selectedSprite.transform.SetPosZ(initialLocalPosZ, true);
                    onComplete.SafeInvoke();
                    zStackDepth--;
                });
            }
        }
    }
}
