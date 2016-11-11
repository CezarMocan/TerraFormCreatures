using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
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
	private Vector3 containerRotation;

	private static string[] FOLDERS = new string[]{"Arms", "Legs", "Bodies", "Heads"};

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
		//GameObject skeleton = Resources.Load ("HumanoidArmBetter") as GameObject;
		//GameObject skeleton = Resources.Load ("Arm") as GameObject;
		//GameObject skeleton = Resources.Load ("PigLeg") as GameObject;
		//GameObject skeleton = Resources.Load ("Generated/OriginalSkeleton-404478") as GameObject;
		//GameObject skeleton = Resources.Load ("BodyTest3") as GameObject;
		GameObject skeleton = Resources.Load ("Generated/Animal1") as GameObject;

		this.containerRotation = new Vector3 (-135, 0, 0);
		GameObject container = new GameObject("SkeletonContainer");
		//container.transform.localEulerAngles = (this.containerRotation);
		GameObject sphere1 = (GameObject) Instantiate (skeleton, new Vector3 (0, 0, 0), Quaternion.identity );

		this.mainObject = new MeshedSkeleton(container, new Vector3(0, 0, 0), sphere1, meshIdToObject, true);

		meshUp = GameObject.Find("MeshUp").GetComponent<Button>();
		meshUp.onClick.AddListener( () => {meshUpPress();} );

		mutate = GameObject.Find ("Mutate").GetComponent<Button> ();
		mutate.onClick.AddListener ( () => {mutatePress(); });


		mutate = GameObject.Find ("Save").GetComponent<Button> ();
		mutate.onClick.AddListener ( () => {savePress(); });

		spheresVisible = true;
		//meshVisible = false;

		//meshIdToObject.Add (this.mainObject.getMeshId (), this.mainObject);

	}

	List<MeshedSkeleton> getSelectedObjects() {
		List<MeshedSkeleton> originals = new List<MeshedSkeleton>();
		foreach (MeshedSkeleton m in this.mutants) {
			if (m.isObjectSelected ())
				originals.Add (m);
		}

		if (this.mainObject.isObjectSelected ())
			originals.Add (this.mainObject);

		return originals;
	}

	void createPrefabs() {
		List<MeshedSkeleton> selected = this.getSelectedObjects ();
		Dropdown input = GameObject.Find ("PrefabType").GetComponent<Dropdown> ();

		foreach (MeshedSkeleton m in selected) {
			//Debug.Log (input.value);
			string localPath = "Assets/Resources/Generated/" + GameInitializer.FOLDERS[input.value] + "/" + m.getOriginalSkeleton ().name + ".prefab";
			Object ePrefab = PrefabUtility.CreateEmptyPrefab (localPath);
			PrefabUtility.ReplacePrefab (m.getOriginalSkeleton (), ePrefab);
		}
	}

	void savePress() {
		Debug.Log ("save");
		this.createPrefabs ();
	}

	void mutatePress() {
		Debug.Log ("mutate");
		this.mutationCount++;

		//Mutation objMutation = new Mutation (this.mainObject.getGameObject(), 0.8f, 0.9f, 0.9f, 20f);
		List<MeshedSkeleton> originals = this.getSelectedObjects();

		SkeletonMutation skeletonMutation = new SkeletonMutation (originals, 0.2f);
		GameObject localContainer = new GameObject("SkeletonContainer" + this.mutationCount.ToString());
		//localContainer.transform.localEulerAngles = (this.containerRotation);
		MeshedSkeleton currMutant = new MeshedSkeleton (localContainer, new Vector3 (0, 0, 6 * this.mutationCount), skeletonMutation, meshIdToObject, false);
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

		if (Input.GetKey (KeyCode.RightArrow)) {
			Camera.main.transform.position += new Vector3(0f, 0f, 0.2f);
		}

		if (Input.GetKey (KeyCode.LeftArrow)) {
			Camera.main.transform.position -= new Vector3(0f, 0f, 0.2f);
		}

		if (Input.GetKey (KeyCode.UpArrow)) {
			Camera.main.transform.position += new Vector3(0.5f, 0f, 0f);
		}

		if (Input.GetKey (KeyCode.DownArrow)) {
			Camera.main.transform.position -= new Vector3(0.5f, 0f, 0f);
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
