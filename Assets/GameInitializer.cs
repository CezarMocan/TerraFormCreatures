using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Skeleton;

public class GameInitializer : MonoBehaviour {

	public Button meshUp, mutate;

	public int mutationCount;
	List<MeshedSkeleton> mutants;

	MeshedSkeleton mainObject;

	private bool spheresVisible = false;
	private bool meshVisible = true;
	private static bool disableUpdate = true;

	Dictionary<int, MeshedSkeleton> meshIdToObject;

	// Use this for initialization
	void Start () {
		meshIdToObject = new Dictionary<int, MeshedSkeleton> ();
		mutants = new List<MeshedSkeleton> ();
		//skeleton = Resources.Load ("Skeleton2") as GameObject;
		//skeleton = Resources.Load ("SkeletonTree") as GameObject;
		//skeleton = Resources.Load ("Trident") as GameObject;
		//GameObject skeleton = Resources.Load ("ArmMotion") as GameObject;
		//skeleton = Resources.Load ("HumanoidMotion") as GameObject;
		GameObject skeleton = Resources.Load ("HumanoidArmBetter") as GameObject;
		//GameObject skeleton = Resources.Load ("Arm") as GameObject;

		GameObject container = new GameObject("SkeletonContainer");
		GameObject sphere1 = (GameObject) Instantiate (skeleton, new Vector3 (0, 0, 0), Quaternion.identity );

		this.mainObject = new MeshedSkeleton(container, new Vector3(0, 0, 0), sphere1, meshIdToObject, true);

		meshUp = GameObject.Find("MeshUp").GetComponent<Button>();
		meshUp.onClick.AddListener( () => {meshUpPress();} );

		mutate = GameObject.Find ("Mutate").GetComponent<Button> ();
		mutate.onClick.AddListener ( () => {mutatePress(); });

		spheresVisible = true;
		//meshVisible = false;

		//meshIdToObject.Add (this.mainObject.getMeshId (), this.mainObject);

	}

	void mutatePress() {
		Debug.Log ("mutate");
		this.mutationCount++;

		//Mutation objMutation = new Mutation (this.mainObject.getGameObject(), 0.8f, 0.9f, 0.9f, 20f);
		List<MeshedSkeleton> originals = new List<MeshedSkeleton>();
		foreach (MeshedSkeleton m in this.mutants) {
			if (m.isObjectSelected ())
				originals.Add (m);
		}

		if (this.mainObject.isObjectSelected ())
			originals.Add (this.mainObject);

		SkeletonMutation skeletonMutation = new SkeletonMutation (originals);
		GameObject localContainer = new GameObject("SkeletonContainer" + this.mutationCount.ToString());
		MeshedSkeleton currMutant = new MeshedSkeleton (localContainer, new Vector3 (0, 0, 15 * this.mutationCount), skeletonMutation, meshIdToObject, false);
		mutants.Add (currMutant);
	}

	void meshUpPress() {
		meshVisible = !meshVisible;
	}
		
	// Update is called once per frame
	void Update () {

		// Select / De-select objects
		if (Input.GetMouseButtonDown(0)) {
			//empty RaycastHit object which raycast puts the hit details into
			RaycastHit hit;
			//ray shooting out of the camera from where the mouse is
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit)) {
				//print out the name if the raycast hits something
				int instanceId = hit.transform.gameObject.GetInstanceID();
				Debug.Log(instanceId);
				MeshedSkeleton selectedSkeleton;
				if (meshIdToObject.TryGetValue (instanceId, out selectedSkeleton)) {
					selectedSkeleton.toggleSelected ();
					selectedSkeleton.updateMesh ();
				}
			}
		}

		// Show / hide spheres when pressing Z
		if (Input.GetKeyDown (KeyCode.Z)) {
			spheresVisible = !spheresVisible;
			this.mainObject.toggleSpheresVisibility (spheresVisible);
			foreach (MeshedSkeleton m in mutants) {
				m.toggleSpheresVisibility (spheresVisible);
			}
		}


		// Animate movement in meshes or not.
		if (!GameInitializer.disableUpdate) {
			if (meshVisible) {
				this.mainObject.updateMesh ();
				foreach (MeshedSkeleton m in this.mutants) {
					m.updateMesh ();
				}
			}
		}
				
			/*
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
			*/

	}
}
