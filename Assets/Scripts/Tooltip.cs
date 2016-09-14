using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour {

	private Item item;
	private string data;
	GameObject tooltip;

	void Start() {
		tooltip = GameObject.Find ("Tooltip");
		tooltip.SetActive (false);
	}

	void Update() {
		//follow mouse position
		if (tooltip.activeSelf) {
			//added canvas group on tool tip to avoid mouse raycast
			tooltip.transform.position = Input.mousePosition;
		}
	}

	public void Activate(Item item) {
		this.item = item;
		tooltip.SetActive (true);
		ConstructDataString ();
	}

	public void Deactivate() {
		tooltip.SetActive (false);
	}

	public void ConstructDataString() {
		data = "<color=#0473f0><b>" + item.Title + "</b></color>\n\n" + item.Description + "";
		tooltip.transform.GetChild (0).GetComponent<Text> ().text = data;
	}
}
