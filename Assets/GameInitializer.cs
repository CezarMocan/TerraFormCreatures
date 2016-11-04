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

	private bool spheresVisible;
	private bool meshVisible;
	private static bool disableUpdate = false;

	// Use this for initialization
	void Start () {
		Debug.Log ("start");
		//skeleton = Resources.Load ("Skeleton2") as GameObject;
		//skeleton = Resources.Load ("SkeletonTree") as GameObject;
		//skeleton = Resources.Load ("Trident") as GameObject;
		GameObject skeleton = Resources.Load ("ArmMotion") as GameObject;
		//skeleton = Resources.Load ("HumanoidMotion") as GameObject;

		GameObject container = new GameObject("SkeletonContainer");
		GameObject sphere1 = (GameObject) Instantiate (skeleton, new Vector3 (0, 0, 0), Quaternion.identity );

		this.mainObject = new MeshedSkeleton(container, sphere1, new Vector3(0, 0, 0));

		meshUp = GameObject.Find("MeshUp").GetComponent<Button>();
		meshUp.onClick.AddListener( () => {meshUpPress();} );

		mutate = GameObject.Find ("Mutate").GetComponent<Button> ();
		mutate.onClick.AddListener ( () => {mutatePress(); });

		spheresVisible = true;
		meshVisible = false;

		mutants = new List<MeshedSkeleton> ();

	}

	void mutatePress() {
		Debug.Log ("mutate");
		this.mutationCount++;

		Mutation objMutation = new Mutation (this.mainObject.getGameObject(), 0.8f, 0.9f, 0.9f, 20f);
		GameObject localContainer = new GameObject("SkeletonContainer" + this.mutationCount.ToString());
		MeshedSkeleton currMutant = new MeshedSkeleton (localContainer, objMutation.getMutatedObject (), new Vector3 (0, 0, 4 * this.mutationCount));
		mutants.Add (currMutant);
	}

	void meshUpPress() {
		meshVisible = !meshVisible;
	}
		
	// Update is called once per frame
	void Update () {
		if (!GameInitializer.disableUpdate) {
			foreach (MeshedSkeleton m in this.mutants) {
				m.getSphereCompleter ().removeMeshes ();
			}
			this.mainObject.getSphereCompleter ().removeMeshes ();

			if (meshVisible) {
				this.mainObject.recomputeSphereCompleter ();
				foreach (MeshedSkeleton m in this.mutants) {
					m.recomputeSphereCompleter ();
				}
			}

			if (Input.GetKeyDown (KeyCode.Z)) {
				spheresVisible = !spheresVisible;
				this.mainObject.toggleSpheresVisibility (spheresVisible);
				foreach (MeshedSkeleton m in mutants) {
					m.toggleSpheresVisibility (spheresVisible);
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
}
