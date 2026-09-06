using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class WeaponUIManager : SerializedMonoBehaviour
{
    [System.NonSerialized, Sirenix.Serialization.OdinSerialize]
    public Dictionary<string, GameObject> weapons;
    private GameObject _activeWeapon;
    //public PlayerWeaponsManager playerWeaponsManager;

    private void Start()
    {
        //OnWeaponSwitched(playerWeaponsManager.startingWeapons[0].weaponName);
    }

    public void OnWeaponSwitched(string weaponName)
    {
        //Debug.LogWarning($"switch to {weaponName}");
        if (_activeWeapon != null) _activeWeapon.SetActive(false);
        
        _activeWeapon = weapons[weaponName];
        if (_activeWeapon != null)
        {
            _activeWeapon.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"Weapon named {weaponName} not found");
        }
    }
}
