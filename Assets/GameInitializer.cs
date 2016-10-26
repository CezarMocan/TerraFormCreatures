using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Skeleton;

public class GameInitializer : MonoBehaviour {

	public Button meshUp, mutate;
	public int mutationCount;
	public GameObject skeleton, sphere1, container;
	public SphereCompleter sc;
	private Dictionary<int, SphereCompleter> sphereCompleters;
	private Dictionary<int, GameObject> containers;
	private bool spheresVisible;
	private bool meshVisible;
	private static bool disableUpdate = true;
	// Use this for initialization
	void Start () {
		Debug.Log ("start");
		//skeleton = Resources.Load ("Skeleton2") as GameObject;
		//skeleton = Resources.Load ("SkeletonTree") as GameObject;
		//skeleton = Resources.Load ("Trident") as GameObject;
		skeleton = Resources.Load ("ArmMotion") as GameObject;
		//skeleton = Resources.Load ("HumanoidMotion") as GameObject;

		container = new GameObject("SkeletonContainer");
		container.transform.position = new Vector3 (0, 0, 0);
		sphere1 = (GameObject) Instantiate (skeleton, new Vector3 (0, 0, 0), Quaternion.identity );
		sphere1.transform.parent = container.transform;
		sphere1.transform.localPosition = new Vector3 (0, 0, 0);
		sphere1.transform.localRotation = Quaternion.identity;
		sc = null;
		meshUp = GameObject.Find("MeshUp").GetComponent<Button>();
		meshUp.onClick.AddListener( () => {meshUpPress();} );

		mutate = GameObject.Find ("Mutate").GetComponent<Button> ();
		mutate.onClick.AddListener ( () => {mutatePress(); });

		spheresVisible = true;
		meshVisible = false;

		sphereCompleters = new Dictionary<int, SphereCompleter> ();
		containers = new Dictionary<int, GameObject> ();

	}

	void mutatePress() {
		Debug.Log ("mutate");
		this.mutationCount++;
		Mutation objMutation = new Mutation (sphere1, 0.5f, 0.7f, 0.7f, 30f);
		GameObject mutant = objMutation.getMutatedObject ();

		GameObject localContainer = new GameObject("SkeletonContainer" + this.mutationCount.ToString());
		localContainer.transform.position = new Vector3 (0, 0, 4 * this.mutationCount);
		mutant.transform.parent = localContainer.transform;
		mutant.transform.localPosition = new Vector3 (0, 0, 0);

		SphereCompleter sc = new SphereCompleter (localContainer, mutant);
		sc.generatePathAndMesh ();
		sphereCompleters.Add (mutationCount, sc);
		containers.Add (mutationCount, localContainer);
	}

	void meshUpPress() {
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
		if (!GameInitializer.disableUpdate) {
			removeMesh (this.sc, this.container);
			if (meshVisible) {
				sc = new SphereCompleter (container, sphere1);
				sc.generatePathAndMesh ();
			}

			if (Input.GetKeyDown (KeyCode.Z)) {
				spheresVisible = !spheresVisible;
				List<Renderer> renderers = new List<Renderer> (sphere1.GetComponentsInChildren<Renderer> ());
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

			if (Input.GetKey (KeyCode.UpArrow)) {
				this.container.transform.localPosition += (new Vector3 (0, .5f, 0));
			}

			if (Input.GetKey (KeyCode.DownArrow)) {
				this.container.transform.localPosition += (new Vector3 (0, -.5f, 0));
			}

			if (Input.GetKey (KeyCode.W)) {
				this.container.transform.localScale += (new Vector3 (.01f, .01f, .01f));
			}				

			if (Input.GetKey (KeyCode.S)) {
				this.container.transform.localScale += (new Vector3 (-.01f, -.01f, -.01f));
			}				

		}
	}
}
