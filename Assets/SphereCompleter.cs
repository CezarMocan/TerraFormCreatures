using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using MeshGeneratorNS;
using Skeleton;

namespace Skeleton {
	public class SphereCompleter {
		public GameObject container, parentSphere, sphere2;
		public List<GameObject> meshes;
		public GameObject completeMesh;
		private Color color;

		private Dictionary<int, Vector3[]> sphereCrosses;

		public SphereCompleter(GameObject container, GameObject parentSphere, Color color) {
			this.container = container;
			this.parentSphere = parentSphere;
			this.sphereCrosses = new Dictionary<int, Vector3[]> ();
			//TODO: Create just one mesh out of this.
			this.meshes = new List<GameObject> ();
			this.color = color;
		}

		public void removeMeshes() {
			List<GameObject> children = new List<GameObject> ();
			foreach (Transform child in this.container.transform)
				if (child.name.Contains (MeshGenerationUtils.FULL_MESH_NAME) || child.name.Contains (MeshGenerationUtils.PARTIAL_MESH_NAME))
					children.Add (child.gameObject);

			foreach (GameObject child in children) {
				UnityEngine.Object.Destroy (child);
			}
		}
			
		public GameObject generatePathAndMesh() {
			dfsChains (parentSphere);
			dfsSplits (parentSphere);
			unifyMeshes ();
			return completeMesh;
			//parentSphere.SetActive (false);
		}

		public void unifyMeshes() {
			CombineInstance[] combine = new CombineInstance[meshes.Count];
			int index = 0;
			Matrix4x4 parentTransform = this.container.transform.worldToLocalMatrix;
			foreach (GameObject meshGO in meshes) {
				MeshFilter mf = meshGO.GetComponent<MeshFilter> ();
				combine [index].mesh = mf.sharedMesh;
				combine [index].transform = parentTransform * mf.transform.localToWorldMatrix;
				meshGO.SetActive (false);
				index++;
			}


			GameObject newMesh = new GameObject (MeshGenerationUtils.FULL_MESH_NAME);
			newMesh.transform.parent = this.container.transform;
			newMesh.transform.localPosition = new Vector3 (0, 0, 0) - this.container.transform.position;
			MeshFilter filter = newMesh.AddComponent< MeshFilter > ();
			filter.mesh.Clear ();
			filter.mesh.CombineMeshes (combine);
			filter.mesh.RecalculateBounds ();

			Material material = new Material (Shader.Find ("Diffuse"));
			material.color = this.color;
			MeshRenderer meshRenderer = newMesh.AddComponent<MeshRenderer> ();
			meshRenderer.material = material;
			//meshRenderer.

			MeshCollider mc = newMesh.AddComponent<MeshCollider> ();
			mc.sharedMesh = filter.mesh;

			newMesh.gameObject.SetActive (true);

			/*
			for (index = 0; index < meshes.Count; index++) {
				UnityEngine.Object.Destroy (meshes [index]);
			}
			*/

			this.completeMesh = newMesh;
		}

		private List<Transform> getProperChildren(GameObject component) {
			List<Transform> properChildren = new List<Transform> ();
			foreach (Transform child in component.transform) {
				if (!child.name.Contains ("__"))
					properChildren.Add (child);
			}

			return properChildren;
		}

		public void dfsSplits(GameObject component) {
			List<Transform> properChildren = this.getProperChildren (component);
			foreach (Transform child in properChildren) {
				dfsSplits (child.gameObject);
			}
			if (properChildren.Count > 1) {
				List<Vector3> points = new List<Vector3>();
				foreach (Transform child in properChildren) {
					points.AddRange (this.sphereCrosses [child.gameObject.GetInstanceID ()]);
				}
				points.AddRange (this.sphereCrosses[component.GetInstanceID ()]);
				// Mesh mesh = MeshGenerationUtils.createMesh (points);

				Mesh m = MeshGenerationUtils.createConvexHullMesh (points);
				GameObject selection = MeshGenerationUtils.createGOFromMesh(MeshGenerationUtils.PARTIAL_MESH_NAME, this.container, m);
				meshes.Add (selection);
			} 
		}
			
		public void dfsChains(GameObject component) {
			List<Transform> properChildren = this.getProperChildren (component);
			bool previousSpheresExist = (properChildren.Count != component.transform.childCount);

			// Generate crossing for parent sphere
			Vector3[] parentCorners; 
			// Cross a section through the middle of the sphere if it is a normal node.
			// If it's a leaf, cross a tiny section through its top side, so that the total mesh ends 
			// with a corner.
			if (properChildren.Count == 0)
				parentCorners = MeshGenerationUtils.generateQuadSection (this.container, component, MeshGenerationUtils.sphereRadius, .5f);
			else if (component.Equals(this.parentSphere))
				parentCorners = MeshGenerationUtils.generateQuadSection (this.container, component, -MeshGenerationUtils.sphereRadius, .5f);
			else
				parentCorners = MeshGenerationUtils.generateQuadSection (this.container, component);				
			this.sphereCrosses.Add (component.GetInstanceID (), parentCorners);

			foreach (Transform child in properChildren) {
				dfsChains (child.gameObject);
			}

			if (properChildren.Count == 1) {
				// Populate linear path between parent sphere and child sphere with spheres
				if (!previousSpheresExist)
					MeshGenerationUtils.generateParentChildSpheres (component, properChildren[0].gameObject);

				// Get all child spheres, before adding other things to the hierarchy of the Transform.
				List<Transform> sphereChildren = getSphereChildren (component);

				// Place a quad in each of the spheres on the way from parent to child
				// In order to get the 4 points that the sphere contributes to creating the mesh.
				List<Vector3[]> meshPoints = generateQuadSections (component, sphereChildren);
				//Vector3[] parentPoints = 

				List<Vector3> points = new List<Vector3> ();
				foreach (Vector3[] meshPoint in meshPoints) {
					points.AddRange (meshPoint);
				}
				Mesh m = MeshGenerationUtils.createConvexHullMesh (points);
				GameObject selection = MeshGenerationUtils.createGOFromMesh(MeshGenerationUtils.PARTIAL_MESH_NAME, this.container, m);
				meshes.Add (selection);

				// Create a mesh out of the corner points obtained in generateQuadSections 
				//GameObject mesh = generateMesh (parentSphere, meshPoints);
				//meshes.Add (mesh);
			} 
		}

		private List<Transform> getSphereChildren(GameObject component) {
			List<Transform> sphereChildren = new List<Transform> ();
			foreach (Transform child in component.transform) {
				sphereChildren.Add (child);
			}
			return sphereChildren;
		}

		private GameObject generateMesh(GameObject parentSphere, List<Vector3[]> allChildrenCorners) {
			List<CubeGeneratorStatic> meshGenerators = new List<CubeGeneratorStatic> ();
			int index;

			//TODO (cmocan): Do foreach instead of for.
			for (index = 0; index < allChildrenCorners.Count - 1; index++) {
				GameObject currMesh = new GameObject ("newMesh" + index.ToString ());
				currMesh.transform.parent = this.container.transform;
				currMesh.transform.localPosition = new Vector3 (0, 0, 0);
				CubeGeneratorStatic meshGenerator = new CubeGeneratorStatic (currMesh);
				meshGenerator.appendMesh (
					allChildrenCorners [index] [2],
					allChildrenCorners [index] [3],
					allChildrenCorners [index] [1],
					allChildrenCorners [index] [0],
					allChildrenCorners [index + 1] [0],
					allChildrenCorners [index + 1] [2],
					allChildrenCorners [index + 1] [3],
					allChildrenCorners [index + 1] [1]
				);
				meshGenerator.finalizeMesh ();
				meshGenerators.Add (meshGenerator);
			}

			CombineInstance[] combine = new CombineInstance[meshGenerators.Count];
			Matrix4x4 parentTransform = this.container.transform.worldToLocalMatrix;
			index = 0;
			foreach (CubeGeneratorStatic meshGenerator in meshGenerators) {
				combine [index].mesh = meshGenerator.getMeshFilter ().sharedMesh;
				combine [index].transform = parentTransform * meshGenerator.getMeshFilter ().transform.localToWorldMatrix;
				meshGenerator.getMeshFilter ().gameObject.SetActive (false);
				index++;
			}


			GameObject newMesh = new GameObject (MeshGenerationUtils.FULL_MESH_NAME);
			newMesh.transform.parent = this.container.transform;
			newMesh.transform.localPosition = new Vector3 (0, 0, 0);
			MeshFilter filter = newMesh.AddComponent< MeshFilter > ();
			filter.mesh.Clear ();
			filter.mesh.CombineMeshes (combine);
			newMesh.gameObject.SetActive (true);

			Material material = new Material (Shader.Find ("Diffuse"));
			MeshRenderer meshRenderer = newMesh.AddComponent<MeshRenderer> ();
			meshRenderer.material = material;

			for (index = 0; index < meshGenerators.Count; index++) {
				UnityEngine.Object.Destroy (meshGenerators [index].gameObject);
			}

			return newMesh;
		}

		private List<Vector3[]> generateQuadSections(GameObject parentSphere, List<Transform> sphereChildren) {
			List<Vector3[]> allChildrenCorners = new List<Vector3[]>();

			//TODO (cmocan): Defensive coding. Check dictionary first, and generate if it's not there.
			Vector3[] parentCorners = this.sphereCrosses[parentSphere.gameObject.GetInstanceID()];
			allChildrenCorners.Add (parentCorners);

			foreach (Transform child in sphereChildren) {
				Vector3[] currChildCorners;
				if (this.sphereCrosses.ContainsKey(child.gameObject.GetInstanceID())) {
					currChildCorners = this.sphereCrosses [child.gameObject.GetInstanceID ()]; 
				} else {
					currChildCorners = MeshGenerationUtils.generateQuadSection (this.container, child.gameObject);
				}
				allChildrenCorners.Add (currChildCorners);
			}				

			return allChildrenCorners;
		}
			
	}
}