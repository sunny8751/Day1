using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class Slot : MonoBehaviour,  IDropHandler{

	public int slotNumber;
	private Inventory inv;

	void Start() {
		inv = GameObject.FindWithTag("Game Handler").GetComponent<Inventory>();
	}

	public void OnDrop(PointerEventData eventData) {
		ItemData droppedItem = eventData.pointerDrag.GetComponent<ItemData> ();
		if (inv.items [slotNumber].ID == -1) {
			//slot is empty
			//clear out item at original slot location
			inv.items [droppedItem.slot] = new Item ();
			//set the new slot to have the item
			inv.items [slotNumber] = droppedItem.item;
			//change item's slot number
			droppedItem.slot = slotNumber;
		} else if (droppedItem.slot != slotNumber){
			//there is already an item there
			//swap the items' locations
			//item already there
			Transform itemAlreadyThere = transform.GetChild(0);
			itemAlreadyThere.GetComponent<ItemData> ().slot = droppedItem.slot;
			itemAlreadyThere.SetParent (inv.slots [droppedItem.slot].transform);
			itemAlreadyThere.localPosition = Vector3.zero;

			//new item
			droppedItem.slot = slotNumber;
			droppedItem.transform.SetParent (transform);
			droppedItem.transform.localPosition = Vector3.zero;

			//update inventory
			inv.items [droppedItem.slot] = itemAlreadyThere.GetComponent<ItemData> ().item;
			inv.items [slotNumber] = droppedItem.item;
		}
	}
}
