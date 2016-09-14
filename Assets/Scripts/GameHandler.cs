using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameHandler : MonoBehaviour {

	//dictionary of objects that we don't want to completely redo when we move on to another chunk
	//for example, trees
	static Dictionary <int, ArrayList> objects = new Dictionary <int, ArrayList> ();
	public static float cameraExtent;

	float time = 12;
	Transform lightSource;
	float lightTotalDistance;
	bool sun = true;

	void Start() {
		//leftmost edge of camera viewport in world coordinates. aka half the width
		cameraExtent = Camera.main.orthographicSize * Screen.width / Screen.height - 4.5f + 0.1f;
		//the sun or moon
		lightSource = GameObject.FindWithTag ("Sun").transform;
		//how far it has to travel vertically in each day/night cycle
		lightTotalDistance = 3.67f + 5f + lightSource.GetComponent<SpriteRenderer>().sprite.bounds.size.y;
		//change the lighting of the world
		StartCoroutine (changeDayNight());
	}

	void Update () {
		//day and night cycles
		float timeScale = 20;
		time += Time.deltaTime / timeScale;
		time = time % 24;
		if (time <= 6) {
			//moon going down
			lightSource.localPosition -= Vector3.up*lightTotalDistance * Time.deltaTime / timeScale / 6;
		} else if (time <= 12) {
			//sun going ups
			if (!sun) {
				lightSource.GetComponent<SpriteRenderer> ().enabled = false;
				lightSource = GameObject.FindWithTag ("Sun").transform;
				lightSource.GetComponent<SpriteRenderer> ().enabled = true;
				lightSource.localPosition = new Vector3 (6.57f, -lightTotalDistance+3.67f, 10);
				sun = true;
			}
			lightSource.localPosition += Vector3.up*lightTotalDistance * Time.deltaTime / timeScale / 6;
		} else if (time <= 18) {
			//sun going down
			lightSource.localPosition -= Vector3.up*lightTotalDistance * Time.deltaTime / timeScale / 6;
		} else if (time <= 24) {
			//moon going up
			if (sun) {
				lightSource.GetComponent<SpriteRenderer> ().enabled = false;
				lightSource = GameObject.FindWithTag ("Moon").transform;
				lightSource.GetComponent<SpriteRenderer> ().enabled = true;
				lightSource.localPosition = new Vector3 (6.57f, -lightTotalDistance+3.67f, 10);
				sun = false;
			}
			lightSource.localPosition += Vector3.up*lightTotalDistance * Time.deltaTime / timeScale / 6;
		}
	}

	public static void addObject (int chunkNumber, GameObject prefab, Vector3 pos) {
		InstantiatedObject obj = new InstantiatedObject (prefab, pos);
		if (objects.ContainsKey (chunkNumber)) {
			objects [chunkNumber].Add (obj);
		} else {
			objects.Add (chunkNumber, new ArrayList () { obj });
		}
	}

	public static void removeObject(int chunkNumber, Vector3 pos) {
		ArrayList chunkObjects = objects [chunkNumber];
		foreach (InstantiatedObject obj in chunkObjects) {
			if (Vector3.Distance(obj.pos, pos) < 0.01f) {
				objects [chunkNumber].Remove (obj);
				print ("Removed");
				return;
			}
		}
	}

	public static ArrayList getObjects (int chunkNumber) {
		return objects [chunkNumber];
	}

	IEnumerator changeDayNight()
	{
		while(true) 
		{
			foreach (SpriteRenderer r in GameObject.FindObjectsOfType<SpriteRenderer>()) {
				float v;
				if (time >= 12) {
					v = 1 - 2 * (time / 24 - 0.5f);
				} else {
					v = time / 12;
				}
				if (r.tag != "Sun" && r.tag != "Moon" && Vector2.Distance(Camera.main.transform.position, r.transform.position) <= cameraExtent * 3.5f) {
					r.color = new Color (v, v, v, 1);
				}
				v = Mathf.Max (v, 0.3f);
				Camera.main.backgroundColor = new Color (v*204f/255, v*243f/255, v*247f/255, 1);
			}
			yield return new WaitForSeconds(1f);
		}
	}
}
