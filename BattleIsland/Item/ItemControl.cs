using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemControl : MonoBehaviour
{
    [Header("Item Info")]
    public string itemName;
    public int id;
    public int amount;

    [Header("Status")]
    public bool canGet = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GetComponent<EquipCheck>() != null && GetComponent<EquipCheck>().isEquip)
            {
                return;
            }

            canGet = true;
            InventoryControl.instance.focusedItems.Add(gameObject);
            UIManager.instance.ShowGetItemUI();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canGet = false;
            InventoryControl.instance.focusedItems.Remove(gameObject);

            if (InventoryControl.instance.focusedItems.Count == 0)
            {
                UIManager.instance.CloseGetItemUI();
            }
        }
    }

    public void PickUpItem()
    {
        if (!canGet)
        {
            return;
        }

        InventoryControl.instance.focusedItems.RemoveAt(InventoryControl.instance.focusedItems.Count - 1);

        if (InventoryControl.instance.focusedItems.Count > 0)
        {
            UIManager.instance.ShowGetItemUI();
        }
        else
        {
            UIManager.instance.CloseGetItemUI();
        }

        InventoryControl.instance.GetItem(itemName, id, amount);

        Destroy(gameObject);
    }
}
