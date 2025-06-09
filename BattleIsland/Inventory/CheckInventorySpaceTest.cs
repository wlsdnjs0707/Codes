using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckInventorySpaceTest : MonoBehaviour
{
    struct item
    {
        public int x; // ���� ���� (�� ����)
        public int y; // ���� ���� (�� ����)
    }

    private int[,] inventory;
    private GameObject[,] tiles;

    public int x; // �� ����
    public int y; // �� ����

    public GameObject tilePrefab;
    public GameObject rowPrefab;
    public GameObject board;

    private item currentItem;

    private int place_x = -1; // �������� ���� �� �ִ� ��ġ x
    private int place_y = -1; // �������� ���� �� �ִ� ��ġ y

    //private bool haveToTurn = false; // ȸ���ؾ� �ϴ°�

    private void Awake()
    {
        inventory = new int[y, x];
        tiles = new GameObject[y, x];

        currentItem = new item();
    }

    private void Start()
    {
        inventory[0, 0] = 1;
        inventory[0, 1] = 1;
        inventory[0, 2] = 1;
        inventory[0, 3] = 1;
        inventory[1, 3] = 1;
        inventory[2, 3] = 1;

        currentItem.x = 2;
        currentItem.y = 5;


        SetTile();
        CheckBoard();
        PlaceItem();
    }

    private void SetTile()
    {
        for (int i = 0; i < y; i++)
        {
            GameObject row = Instantiate(rowPrefab);
            row.transform.SetParent(board.transform);

            for (int j = 0; j < x; j++)
            {
                GameObject tile = Instantiate(tilePrefab);
                tile.transform.SetParent(row.transform);

                tiles[i, j] = tile;

                if (inventory[i, j] == 1)
                {
                    tile.GetComponent<Image>().color = Color.red;
                }
            }
        }
    }

    private void CheckBoard()
    {
        for (int i = 0; i < y; i++)
        {
            for (int j = 0; j < x; j++)
            {
                if (i + currentItem.y - 1 < y && j + currentItem.x - 1 < x)
                {
                    if (inventory[i, j] == 0) // �ش� ��ġ�� ��������� üũ ����
                    {
                        if (CheckSpace(i, j))
                        {
                            place_y = i;
                            place_x = j;
                            return;
                        }
                    }
                }
            }
        }

        int temp = currentItem.x;
        currentItem.x = currentItem.y;
        currentItem.y = temp;

        for (int i = 0; i < y; i++)
        {
            for (int j = 0; j < x; j++)
            {
                if (i + currentItem.y - 1 < y && j + currentItem.x - 1 < x)
                {
                    if (inventory[i, j] == 0) // �ش� ��ġ�� ��������� üũ ����
                    {
                        if (CheckSpace(i, j))
                        {
                            place_y = i;
                            place_x = j;
                            //haveToTurn = true;
                            return;
                        }
                    }
                }
            }
        }
    }

    private void PlaceItem()
    {
        if (place_x == -1 || place_y == -1)
        {
            Debug.Log("�������� ����");
        }
        else
        {
            Debug.Log($"{place_y}, {place_x}�� ���´�");
            PlaceColor();
        }
    }

    private bool CheckSpace(int i, int j)
    {
        for (int k = 0; k < currentItem.y; k++)
        {
            for (int l = 0; l < currentItem.x; l++)
            {
                if (inventory[i + k, j + l] != 0)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void PlaceColor()
    {
        for (int i = 0; i < currentItem.y; i++)
        {
            for (int j = 0; j < currentItem.x; j++)
            {
                tiles[place_y + i, place_x + j].GetComponent<Image>().color = Color.blue;
            }
        }
    }
}
