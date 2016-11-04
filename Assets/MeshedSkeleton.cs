using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Skeleton {
	public class MeshedSkeleton {
		private SphereCompleter sc;
		private GameObject container, gameObject, meshGameObject, originalSkeleton;
		private Dictionary<int, MeshedSkeleton> meshIdToObject;
		private bool isSelected;

		public static Color SELECTED_COLOR = Color.red;
		public static Color UNSELECTED_COLOR = Color.white;

		public MeshedSkeleton(GameObject container, GameObject gameObject, Vector3 positionOnScreen, Dictionary<int, MeshedSkeleton> meshIdToObject, bool isSelected = false) {
			this.container = container;
			this.gameObject = gameObject;
			this.meshIdToObject = meshIdToObject;
			this.isSelected = isSelected;

			this.originalSkeleton = Object.Instantiate (gameObject);
			this.originalSkeleton.SetActive (false);
			this.originalSkeleton.transform.parent = this.container.transform;
			this.originalSkeleton.name = "OriginalSkeleton" + this.gameObject.GetInstanceID ().ToString ();

			this.container.transform.position = positionOnScreen;
			this.gameObject.transform.parent = this.container.transform;
			this.gameObject.transform.localPosition = new Vector3 (0, 0, 0);
			this.gameObject.transform.localRotation = Quaternion.identity;

			this.sc = null;
			this.updateMesh ();
		}

		public void toggleSpheresVisibility(bool spheresVisible) {
			//TODO (cmocan): Maybe I can just disable the GameObject? Nah...
			List<Renderer> renderers = new List<Renderer> (this.gameObject.GetComponentsInChildren<Renderer> ());
			foreach (Renderer r in renderers) {
				r.enabled = spheresVisible;
			}
		}

		public SphereCompleter getSphereCompleter() {
			return this.sc;
		}

		private Color getMeshColor() {
			return this.isSelected ? MeshedSkeleton.SELECTED_COLOR : MeshedSkeleton.UNSELECTED_COLOR;
		}

		public void updateMesh() {
			if (this.sc != null) {
				meshIdToObject.Remove (this.getMeshId ());
				this.sc.removeMeshes ();
			}

			Color color = this.getMeshColor ();
			this.sc = new SphereCompleter (this.container, this.gameObject, color);
			this.meshGameObject = sc.generatePathAndMesh ();
			meshIdToObject.Add (this.getMeshId (), this);
		}

		public void toggleSelected() {
			Debug.Log("toggle selected");
			this.isSelected = !this.isSelected;
		}

		public bool isObjectSelected() {
			return this.isSelected;
		}
			
		public GameObject getMeshGameObject() {
			return this.meshGameObject;
		}

		public int getMeshId() {
			return this.meshGameObject.GetInstanceID ();
		}

		public GameObject getContainer() {
			return this.container;
		}

		public GameObject getGameObject() {
			return this.gameObject;
		}

		public GameObject getOriginalSkeleton() {
			return this.originalSkeleton;
		}

	}
}