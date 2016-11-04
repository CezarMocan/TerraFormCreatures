using UnityEngine;
using System.Collections;

public class PendulumXY : MonoBehaviour {
	private bool isMoving;
	// Use this for initialization
	void Start () {
		isMoving = true;
		//gameObject.transform.localEulerAngles = new Vector3 (0, 0, Mathf.Sin (1f) * 45);
	}

	// Update is called once per frame
	void Update () {
		if (isMoving)
			gameObject.transform.localEulerAngles = new Vector3 (Mathf.Sin (Time.time) * 40 + 40, Mathf.Sin (Time.time) * 10 + 10, 0);

		if (Input.GetKeyDown (KeyCode.X)) {
			isMoving = !isMoving;
		}
	}
}
