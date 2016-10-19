using UnityEngine;
using System.Collections;

public class Pendulum : MonoBehaviour {

	private bool isMoving;
	// Use this for initialization
	void Start () {
		isMoving = true;
		//gameObject.transform.localEulerAngles = new Vector3 (0, 0, Mathf.Sin (1f) * 45);
	}
	
	// Update is called once per frame
	void Update () {
		if (isMoving)
			gameObject.transform.localEulerAngles = new Vector3 (0, 0, Mathf.Sin (Time.time) * 20);

		if (Input.GetKeyDown (KeyCode.X)) {
			isMoving = !isMoving;
		}
	}
}
