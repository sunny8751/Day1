using UnityEngine;
using System.Collections;

public class InstantiatedObject {

	public GameObject prefab;
	public Vector3 pos;

	public InstantiatedObject (GameObject prefab, Vector3 pos) {
		this.prefab = prefab;
		this.pos = pos;
	}
}
