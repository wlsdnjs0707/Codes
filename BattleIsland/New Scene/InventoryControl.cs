using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryControl : MonoBehaviour
{
    public static InventoryControl instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        gameUIControll = FindObjectOfType<GameUIControll>();
        inventory = new List<Item>();
    }

    [Header("Item")]
    private List<GameObject> nearItemList = new List<GameObject>(); // �ֺ� ������ ����Ʈ
    public LayerMask itemLayer; // ������ üũ�� ���̾�
    public List<GameObject> focusedItems; // ���� �ݶ��̴��� ���˵� ������ ����Ʈ

    [Header("Model")]
    [SerializeField] private GameObject bagModel;
    [SerializeField] private GameObject armorModel;
    [SerializeField] private GameObject helmetModel;

    [Header("UI")]
    [SerializeField] private GameUIControll gameUIControll;

    public struct Item
    {
        public string itemName;
        public int id;
        public int amount;

        public Item(string name, int id, int amount)
        {
            this.itemName = name;
            this.id = id;
            this.amount = amount;
            // �ʿ��� ���� ������ ���Ŀ� �߰�
        }
    }

    // [������ �����۵��� �����س��� ����Ʈ]
    public List<Item> inventory;

    // [�Ѿ� ����]
    public int ammo;

    private void Update()
    {
        if (focusedItems.Count > 0 && Input.GetKeyDown(KeyCode.F)) // TEST
        {
            if (focusedItems[focusedItems.Count - 1].CompareTag("Weapon"))
            {
                if (focusedItems[focusedItems.Count - 1].GetComponent<EquipCheck>().isEquip)
                {
                    return;
                }
            }

            focusedItems[focusedItems.Count - 1].GetComponent<ItemControl>().PickUpItem();
        }
    }

    public void GetItem(string name, int id, int amount)
    {
        Item currentItem = new Item(name, id, amount);

        if (id == 103) // �Ƹ�
        {
            if (!armorModel.activeSelf) // �Ƹ� �� Ȱ��ȭ
            {
                GetComponent<CombatControl>().isArmor = true;
                GetComponent<CombatControl>().playerHealth += 60;
                gameUIControll.hpbar.maxValue = 360.0f;
                armorModel.SetActive(true);
            }
        }
        else if (id == 104) // ����
        {
            if (!bagModel.activeSelf) // ���� �� Ȱ��ȭ
            {
                bagModel.SetActive(true);
            }
        }
        else if (id == 107) // ����ź
        {
            for (int i = 0; i < amount - 1; i++)
            {
                inventory.Add(currentItem);
            }
        }
        else if (id == 108) // �Ѿ˻���
        {
            ammo += amount;
            UIManager.instance.UpdateAmmoText(0); // Test

            if (InventoryControl.instance.CheckInventory(108))
            {
                return; // �̹� �ڽ� ������ �߰� ����
            }
        }
        else if (id == 111) // ���
        {
            if (!helmetModel.activeSelf) // ��� �� Ȱ��ȭ
            {
                helmetModel.SetActive(true);
            }
        }
        else if (id == 116) // ������
        {
            GetComponent<CombatControl>().EquipGun(GunType.Rifle1);
        }
        else if (id == 117) // ��������
        {
            GetComponent<CombatControl>().EquipGun(GunType.Sniper1);
        }

        inventory.Add(currentItem);
    }

    public void RemoveItem(int id)
    {
        foreach(Item item in inventory)
        {
            if (item.id == id)
            {
                inventory.Remove(item);
                return;
            }
        }
    }
    
    public void ShowInventory() // TEST
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            Debug.Log($"������ �̸� : {inventory[i].itemName}, ������ ID : {inventory[i].id}");
        }
    }

    public bool CheckInventory(int id)
    {
        foreach (Item item in inventory)
        {
            if (item.id == id)
            {
                return true;
            }
        }

        // �������� �ʴ´�
        return false;
    }

    private void GetNearItemList()
    {
        nearItemList.Clear();

        Collider[] colliders = Physics.OverlapSphere(transform.position, 3.0f, itemLayer); // �÷��̾� �ֺ��� item���̾� ������Ʈ�� ����

        foreach (Collider c in colliders)
        {
            nearItemList.Add(c.gameObject); // �ֺ� �����۵� ����Ʈ�� �߰�
        }
    }
}
