using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private GameObject selectedOutline;
    [SerializeField] private ItemInfoDictionary info;
    public void UpdateIcon(ItemID item)
    {
        if (item == ItemID.NONE)
        {
            icon.sprite = null;
            icon.color = Color.clear;
        } else
        {
            icon.color = Color.white;
            icon.sprite = info.GetItemInfo(item).icon;
        }
    }

    public void SetSelected(bool selected)
    {
        selectedOutline.SetActive(selected);
    }
}
