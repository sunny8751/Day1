using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ItemData : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler {

	public Item item;
	public int amount = 0;
	public int slot;

	private Inventory inv;
	private Tooltip tooltip;

	//when beginning drag, start from slot, not snapping from mouse position
	//difference between pos of icon and pos of mouse
	private Vector2 offset;

	void Start() {
		inv = GameObject.FindWithTag ("Game Handler").GetComponent<Inventory> ();
		tooltip = inv.GetComponent<Tooltip> ();
	}

	public void OnPointerDown(PointerEventData eventData){
		if (item != null) {
			offset = eventData.position - new Vector2(this.transform.position.x, this.transform.position.y);
		} 
	}

	public void OnBeginDrag(PointerEventData eventData) {
		//make sure we even have an item
		if (item != null) {
			//bump it to the slot panel
			transform.SetParent (transform.parent.parent);
			transform.position = eventData.position - offset;
			//block raycast for the item
			GetComponent<CanvasGroup>().blocksRaycasts = false;
		}
	}

	public void OnDrag(PointerEventData eventData) {
		if (item != null) {
			transform.position = eventData.position - offset;
		}
	}

	public void OnEndDrag(PointerEventData eventData) {
		transform.SetParent (inv.slots[slot].transform);
		transform.localPosition = Vector3.zero;
		//block raycast for the item
		GetComponent<CanvasGroup>().blocksRaycasts = true;
	}

	public void OnPointerEnter(PointerEventData eventData) {
		tooltip.Activate (item);
	}

	public void OnPointerExit(PointerEventData eventData) {
		tooltip.Deactivate ();
	}
}
