using System;
using System.Collections.Generic;
using Mirror.BouncyCastle.Bcpg.OpenPgp;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "ItemInfoDictionary", menuName = "Scriptable Objects/ItemInfoDictionary")]
public class ItemInfoDictionary : ScriptableObject
{
    [SerializeField] private List<ItemInfo> items;

    public ItemInfo GetItemInfo(ItemID id)
    {
        foreach (var item in items)
            if (item.item == id)
                return item;
        
        Debug.LogWarning($"Failed to fetch item info for item id [{id}]");
        return default;
    }
}

[Serializable] public struct ItemInfo
{
    public ItemID item;
    public GameObject prefab;
    public Sprite icon;
    public int value;
    public bool unsellable;
    public UseData action1, action2, action3;
}

[Serializable] public struct UseData
{
    public UnityEvent<PlayerController, ItemID> action;
    public string verb;
}

public enum ItemID
{
    NONE, FOOD_CHICKEN, QUEST_POSTING, EQUIP_SWORD
}