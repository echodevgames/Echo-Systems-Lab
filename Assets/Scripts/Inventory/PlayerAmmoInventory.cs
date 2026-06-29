//-----PlayerAmmoInventory.cs START-----

using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAmmoInventory : MonoBehaviour
{
    public event Action OnAmmoChanged;

    private readonly Dictionary<string, int> reserveAmmo = new Dictionary<string, int>();

    public int GetReserveAmmo(AmmoData ammoData)
    {
        if (ammoData == null || string.IsNullOrWhiteSpace(ammoData.ammoId))
            return 0;

        return reserveAmmo.TryGetValue(ammoData.ammoId, out int amount) ? amount : 0;
    }

    public void AddAmmo(AmmoData ammoData, int amount)
    {
        if (ammoData == null || string.IsNullOrWhiteSpace(ammoData.ammoId))
            return;

        if (amount <= 0)
            return;

        if (!reserveAmmo.ContainsKey(ammoData.ammoId))
            reserveAmmo.Add(ammoData.ammoId, 0);

        reserveAmmo[ammoData.ammoId] += amount;

        Debug.Log($"Picked up {amount} {ammoData.displayName}. Reserve: {reserveAmmo[ammoData.ammoId]}");

        OnAmmoChanged?.Invoke();
    }

    public int RemoveAmmo(AmmoData ammoData, int amount)
    {
        if (ammoData == null || string.IsNullOrWhiteSpace(ammoData.ammoId))
            return 0;

        if (amount <= 0)
            return 0;

        int currentReserve = GetReserveAmmo(ammoData);
        int amountToRemove = Mathf.Min(currentReserve, amount);

        if (amountToRemove <= 0)
            return 0;

        reserveAmmo[ammoData.ammoId] = currentReserve - amountToRemove;

        OnAmmoChanged?.Invoke();

        return amountToRemove;
    }

    public bool HasAmmo(AmmoData ammoData)
    {
        return GetReserveAmmo(ammoData) > 0;
    }
}

//-----PlayerAmmoInventory.cs END-----