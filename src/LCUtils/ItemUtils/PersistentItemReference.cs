using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LCUtils;

public class PersistentItemReference
{
    private readonly static List<PersistentItemReference> _pool = [];
    public readonly string name;
    public readonly string itemName;
    public readonly int itemId;
    public readonly string nodeText;
    public readonly string configName;
    public readonly Type? grabbableObjectType;
    public readonly string grabbableObjectName;
    public GameObject? SpawnPrefab {
        get
        {
            GameObject? spawnPrefab = Item?.spawnPrefab;
            if (spawnPrefab == null) return null;

            return spawnPrefab;
        }
    }
    public GrabbableObject? GrabbableObject => Item?.GetGrabbableObject(); 
    private Item? _Item;
    public Item? Item => (_Item != null) ? _Item : (_Item = ItemUtils.AllItems.FirstOrDefault<Item?>(this.LooselyEquals));
    private PersistentItemReference(Item item)
    {
        name = item.name;
        itemName = item.itemName;
        itemId = item.itemId;
        nodeText = item.GetNodeText();
        configName = item.GetConfigName();
        grabbableObjectType = item.GetGrabbableObjectType();
        grabbableObjectName = item.GetGrabbableObjectName();
        _Item = item;

        _pool.Add(this);
    }

    public static PersistentItemReference? GetPersistentItemReference(Item? item)
    {
        if (item == null) return null;

        return _pool.FirstOrDefault<PersistentItemReference?>(item.LooselyEquals) ?? new PersistentItemReference(item);
    }
}

public static class ItemStuffExtensions
{
    public static PersistentItemReference? GetPersistentReference(this Item? item)
    {
        return PersistentItemReference.GetPersistentItemReference(item);
    }

    public static bool LooselyEquals(this PersistentItemReference? itemRef, PersistentItemReference? otherItemRef)
    {
        // Check if these are the same reference
        if (ReferenceEquals(itemRef, otherItemRef)) return true;

        // If either is null at this point, they cannot be equal
        if (itemRef == null || otherItemRef == null) return false;

        // Disqualify if properties are different
        if (itemRef.itemId != otherItemRef.itemId) return false;
        if (itemRef.configName != otherItemRef.configName) return false;
        if (itemRef.grabbableObjectType != otherItemRef.grabbableObjectType) return false;

        return true;
    }

    public static bool LooselyEquals(this PersistentItemReference? itemRef, Item? otherItem)
    {
        // If one is null, they are equal if the other is null
        if (itemRef == null) return otherItem == null;
        if (otherItem == null) return false;

        // Disqualify if properties are different
        if (itemRef.itemId != otherItem.itemId) return false;
        if (itemRef.configName != otherItem.GetConfigName()) return false;
        if (itemRef.grabbableObjectType != otherItem.GetGrabbableObjectType()) return false;

        return true;
    }

    public static bool LooselyEquals(this Item? item, PersistentItemReference? otherItemRef)
    {
        return otherItemRef.LooselyEquals(item);
    }
}