using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Skeleton;

public class GameInitializer : MonoBehaviour {

	public Button button;
	public GameObject skeleton, sphere1, container;
	public SphereCompleter sc;
	private bool spheresVisible;
	private bool meshVisible;
	// Use this for initialization
	void Start () {
		Debug.Log ("start");
		//skeleton = Resources.Load ("Skeleton2") as GameObject;
		//skeleton = Resources.Load ("SkeletonTree") as GameObject;
		skeleton = Resources.Load ("Trident") as GameObject;

		container = new GameObject("SkeletonContainer");
		container.transform.position = new Vector3 (0, 0, 0);
		sphere1 = (GameObject) Instantiate (skeleton, new Vector3 (0, 0, 0), Quaternion.identity );
		sphere1.transform.parent = container.transform;
		sphere1.transform.localPosition = new Vector3 (0, 0, 0);
		sphere1.transform.localRotation = Quaternion.identity;
		sc = null;
		button = GameObject.Find("Button").GetComponent<Button>();
		//button.GetComponentsInChildren<Text> (). = "Mesh up!";
		button.onClick.AddListener( () => {buttonPress();} );

		spheresVisible = true;
		meshVisible = false;

	}

	void buttonPress() {
		meshVisible = !meshVisible;
		// Comment this when re-activating the Update function.
		sc = new SphereCompleter (container, sphere1);
		sc.generatePathAndMesh ();
	}

	private void removeMesh(SphereCompleter sc, GameObject container) {
		if (sc != null) {
			List<GameObject> children = new List<GameObject> ();
			foreach (Transform child in container.transform)
				if (child.name.Contains ("parentMesh") || child.name.Contains ("MeshCH"))
					children.Add (child.gameObject);

			foreach (GameObject child in children) {
				UnityEngine.Object.Destroy (child);
			}
		}
	}

	// Update is called once per frame
	void Update () {
		/*
		removeMesh (this.sc, this.container);
		if (meshVisible) {
			sc = new SphereCompleter (container, sphere1);
			sc.generatePathAndMesh ();
		}

		if (Input.GetKeyDown(KeyCode.Z)) {
			spheresVisible = !spheresVisible;
			List<Renderer> renderers = new List<Renderer>(sphere1.GetComponentsInChildren<Renderer>());
			foreach (Renderer r in renderers) {
				r.enabled = spheresVisible;
			}
		}

		if (Input.GetKey (KeyCode.RightArrow)) {
			this.container.transform.Rotate (new Vector3 (0, 2, 0));
		}

		if (Input.GetKey (KeyCode.LeftArrow)) {
			this.container.transform.Rotate (new Vector3 (0, -2, 0));
		}
		*/
	}
}
