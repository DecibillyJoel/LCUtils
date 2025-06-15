using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LCUtils;

/// <summary>
/// Provides a persistent reference to an Item that maintains key identifying properties
/// even when the original Item object becomes unavailable or is recreated.
/// </summary>
public class PersistentItemReference
{
    /// <summary>
    /// Static pool of all created PersistentItemReference instances to enable reuse and prevent duplicates.
    /// </summary>
    private readonly static List<PersistentItemReference> _pool = [];

    /// <summary>
    /// The name of the item as stored in the Item.name property.
    /// </summary>
    public readonly string name;

    /// <summary>
    /// The display name of the item as stored in the Item.itemName property.
    /// </summary>
    public readonly string itemName;

    /// <summary>
    /// The unique identifier of the item as stored in the Item.itemId property.
    /// </summary>
    public readonly int itemId;

    /// <summary>
    /// The scan node header text of the item.
    /// </summary>
    public readonly string nodeText;

    /// <summary>
    /// The configuration name associated with this item.
    /// </summary>
    public readonly string configName;

    /// <summary>
    /// The Type of the GrabbableObject associated with this item, if any.
    /// </summary>
    public readonly Type? grabbableObjectType;

    /// <summary>
    /// The name of the GrabbableObject associated with this item.
    /// </summary>
    public readonly string grabbableObjectName;

    /// <summary>
    /// Gets the spawn prefab GameObject for this item. Returns null if the Item is not available or has no spawn prefab.
    /// </summary>
    public GameObject? SpawnPrefab {
        get
        {
            GameObject? spawnPrefab = Item?.spawnPrefab;
            if (spawnPrefab == null) return null;

            return spawnPrefab;
        }
    }

    /// <summary>
    /// Gets the GrabbableObject instance associated with this item. Returns null if the Item is not available.
    /// </summary>
    public GrabbableObject? GrabbableObject => Item?.GetGrabbableObject(); 

    /// <summary>
    /// Cached reference to the actual Item object.
    /// </summary>
    private Item? _Item;

    /// <summary>
    /// Gets the current Item object, either from cache or by searching all available items.
    /// </summary>
    public Item? Item => (_Item != null) ? _Item : (_Item = ItemUtils.AllItems.FirstOrDefault<Item?>(this.LooselyEquals));

    /// <summary>
    /// Private constructor that creates a new PersistentItemReference from an Item.
    /// Extracts and stores all key identifying properties from the source item.
    /// Automatically adds the new instance to the static pool.
    /// </summary>
    /// <param name="item">The Item to create a persistent reference for.</param>
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

    /// <summary>
    /// Gets or creates a PersistentItemReference for the specified Item.
    /// If no matching reference exists, creates a new one.
    /// </summary>
    /// <param name="item">The Item to get a persistent reference for. Can be null.</param>
    /// <returns>A PersistentItemReference for the item, or null if the item parameter is null.</returns>
    public static PersistentItemReference? GetPersistentItemReference(Item? item)
    {
        if (item == null) return null;

        return _pool.FirstOrDefault<PersistentItemReference?>(item.LooselyEquals) ?? new PersistentItemReference(item);
    }
}

/// <summary>
/// Extension methods for Item objects and PersistentItemReference comparison operations.
/// Provides convenient methods for creating persistent references and performing loose equality comparisons.
/// </summary>
public static class ItemStuffExtensions
{
    /// <summary>
    /// Gets or creates a PersistentItemReference for the Item.
    /// If no matching reference exists, creates a new one.
    /// </summary>
    /// <param name="item">The Item to get a persistent reference for.</param>
    /// <returns>A PersistentItemReference for this item, or null if this item is null.</returns>
    public static PersistentItemReference? GetPersistentReference(this Item? item)
    {
        return PersistentItemReference.GetPersistentItemReference(item);
    }

    /// <summary>
    /// Determines if two PersistentItemReference objects represent the same item.
    /// Uses loose equality comparison based on itemId, configName, and grabbableObjectType.
    /// Two references are considered equal if they have the same identifying properties.
    /// </summary>
    /// <param name="itemRef">The first PersistentItemReference to compare.</param>
    /// <param name="otherItemRef">The second PersistentItemReference to compare.</param>
    /// <returns>True if the references represent the same item, false otherwise.</returns>
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

    /// <summary>
    /// Determines if a PersistentItemReference represents the same item as an Item object.
    /// Uses loose equality comparison based on itemId, configName, and grabbableObjectType.
    /// </summary>
    /// <param name="itemRef">The PersistentItemReference to compare.</param>
    /// <param name="otherItem">The Item to compare against.</param>
    /// <returns>True if the reference and item represent the same item, false otherwise.</returns>
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

    /// <summary>
    /// Determines if an Item object represents the same item as a PersistentItemReference.
    /// This is a convenience method that delegates to the PersistentItemReference.LooselyEquals method.
    /// </summary>
    /// <param name="item">The Item to compare.</param>
    /// <param name="otherItemRef">The PersistentItemReference to compare against.</param>
    /// <returns>True if the item and reference represent the same item, false otherwise.</returns>
    public static bool LooselyEquals(this Item? item, PersistentItemReference? otherItemRef)
    {
        return otherItemRef.LooselyEquals(item);
    }
}