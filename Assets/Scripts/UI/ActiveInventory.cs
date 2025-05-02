using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActiveInventory : Singleton<ActiveInventory>
{
    private int activeSlotIndexNum = 0;

    private PlayerControls playerControls;

    protected override void Awake()
    {
        base.Awake();

        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Inventory.Keyboard.performed += ToggleActiveSlot;
        playerControls.Inventory.NextWeapon.performed += ToggleSelectNextWeapon;
        playerControls.Inventory.PreviousWeapon.performed += ToggleSelectPreviousWeapon;
        playerControls.Enable();
    }

    private void ToggleSelectPreviousWeapon(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() == 1)
        {
            activeSlotIndexNum = (activeSlotIndexNum - 1 + transform.childCount) % transform.childCount;
            ToggleActiveHighlight(activeSlotIndexNum);
        }
    }

    private void ToggleSelectNextWeapon(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() == 1)
        {
            activeSlotIndexNum = (activeSlotIndexNum + 1) % transform.childCount;
            ToggleActiveHighlight(activeSlotIndexNum);
        }
    }

    // private void OnDisable()
    // {
    //     playerControls.Disable();
    //     playerControls.Combat.Disable();
    //     playerControls.Turn.Disable();
    //     playerControls.Inventory.Disable();
    //     playerControls.Movement.Disable();
    //     playerControls.Inventory.Keyboard.performed -= ToggleActiveSlot;
    // }

    public void EquipStartingWeapon()
    {
        ToggleActiveHighlight(0);
    }

    private void ToggleActiveSlot(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        ToggleActiveHighlight((int)context.ReadValue<float>() - 1);
    }

    private void ToggleActiveHighlight(int indexNum)
    {
        activeSlotIndexNum = indexNum;

        foreach (Transform inventorySlot in this.transform)
        {
            inventorySlot.GetChild(0).gameObject.SetActive(false);
        }

        this.transform.GetChild(indexNum).GetChild(0).gameObject.SetActive(true);

        ChangeActiveWeapon();
    }
    private void ChangeActiveWeapon()
    {

        if (ActiveWeapon.Instance.CurrentActiveWeapon != null)
        {
            Destroy(ActiveWeapon.Instance.CurrentActiveWeapon.gameObject);
        }

        Transform childTransform = transform.GetChild(activeSlotIndexNum);
        InventorySlot inventorySlot = childTransform.GetComponentInChildren<InventorySlot>();
        WeaponInfo weaponInfo = inventorySlot.GetWeaponInfo();
        GameObject weaponToSpawn = weaponInfo.weaponPrefab;

        if (weaponInfo == null)
        {
            ActiveWeapon.Instance.WeaponNull();
            return;
        }

        GameObject newWeapon = Instantiate(weaponToSpawn, ActiveWeapon.Instance.transform);

        //ActiveWeapon.Instance.transform.rotation = Quaternion.Euler(0, 0, 0);
        // newWeapon.transform.parent = ActiveWeapon.Instance.transform;

        ActiveWeapon.Instance.NewWeapon(newWeapon.GetComponent<MonoBehaviour>());

    }
}
