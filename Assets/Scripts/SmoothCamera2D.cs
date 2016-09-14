using UnityEngine;
using System.Collections;

public class SmoothCamera2D : MonoBehaviour {

	public float dampTime = 0.15f;
	private Vector3 velocity = Vector3.zero;
	public Transform target;
	private Camera mainCamera;

	void Start() {
		mainCamera = GetComponent<Camera> ();
	}

	void Update () 
	{
		if (target)
		{
			Vector3 point = mainCamera.WorldToViewportPoint(target.position);
			Vector3 delta = target.position - mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
			Vector3 destination = transform.position + delta;
			if (destination.y <= 0) {
				destination.y = 0;
			}
			transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
		}

	}
}