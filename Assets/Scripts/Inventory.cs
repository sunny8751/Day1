using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Inventory : MonoBehaviour {

	GameObject inventoryPanel, slotPanel;
	public GameObject inventorySlot, inventoryItem;
	ItemDatabase database;
	public List <Item> items = new List <Item> ();
	public List <GameObject> slots = new List<GameObject>();
	public int slotAmount = 20;

	void Start() {
		database = GetComponent<ItemDatabase> ();
		inventoryPanel = GameObject.FindWithTag ("Inventory Panel");
		slotPanel = inventoryPanel.transform.FindChild ("SlotPanel").gameObject;
		//add slotAmount slots to inventory
		for (int i = 0; i < slotAmount; i++) {
			//add an empty item
			items.Add (new Item());
			//create the slots into the slotPanel
			slots.Add (Instantiate(inventorySlot));
			slots [i].transform.SetParent (slotPanel.transform);
			slots [i].GetComponent<Slot> ().slotNumber = i;
		}
	}

	public void AddItem(int id) {
		Item itemToAdd = database.FetchItemByID (id);
		//stackable
		if (itemToAdd.Stackable && getIndexOfItem(itemToAdd) != -1) {
			ItemData data = slots [getIndexOfItem (itemToAdd)].transform.GetChild (0).GetComponent<ItemData> ();
			data.amount++;
			data.transform.GetChild (0).GetComponent<Text> ().text = data.amount.ToString();
			return;
		}

		//find an empty slot to add the item to
		for (int i = 0; i < items.Count; i++) {
			if (items [i].ID == -1) {
				//found an empty slot
				items[i] = itemToAdd;
				//create the item
				GameObject itemObj = Instantiate (inventoryItem);
				itemObj.transform.SetParent (slots [i].transform);
				itemObj.transform.localPosition = Vector2.zero;
				itemObj.transform.localScale = Vector3.one;
				itemObj.transform.GetChild (0).localScale = Vector3.one;
				itemObj.name = itemToAdd.Title;
				//change the item icon
				itemObj.GetComponent<Image>().sprite = itemToAdd.Sprite;
				//change itemData
				ItemData data = itemObj.GetComponent<ItemData>();
				data.item = itemToAdd;
				data.slot = i;
				data.amount = 1;
				break;
			}
		}
	}

	int getIndexOfItem(Item item) {
		for (int i = 0; i < items.Count; i++) {
			if (items [i].ID == item.ID) {
				return i;
			}
		}
		return -1;
	}
}
