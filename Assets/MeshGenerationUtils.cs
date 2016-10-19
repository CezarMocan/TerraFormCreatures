using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using MeshGeneratorNS;

namespace Skeleton {
	public static class MeshGenerationUtils
	{
		public static float sphereRadius = 0.5f;

		public static GameObject createGOFromMesh(String name, GameObject container, Mesh mesh) {
			GameObject selection = new GameObject(name);
			selection.transform.parent = container.transform;
			selection.transform.localPosition = new Vector3 (0, 0, 0);

			MeshFilter meshFilter = (MeshFilter)selection.AddComponent(typeof(MeshFilter));

			meshFilter.mesh = mesh;
			MeshGenerationUtils.mirrorMesh (meshFilter.mesh);
			meshFilter.mesh.RecalculateNormals ();
			meshFilter.mesh.Optimize ();

			Material material = new Material(Shader.Find("Diffuse"));
			MeshRenderer renderer = selection.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
			renderer.material = material;

			return selection;
		}

		public static Mesh createConvexHullMesh(List<Vector3> points) {
			Mesh m = new Mesh();
			m.name = "ConvexHullMesh";
			List<int> triangles = new List<int> ();
			var vertices = points.Select (x => new MIConvexHull.Vertex(x)).ToList();
			var result = MIConvexHull.ConvexHull.Create (vertices);
			m.vertices = result.Points.Select (x => x.ToVec ()).ToArray ();
			var xxx = result.Points.ToList ();

			foreach (var face in result.Faces) {
				triangles.Add(xxx.IndexOf(face.Vertices[0]));
				triangles.Add(xxx.IndexOf(face.Vertices[1]));
				triangles.Add(xxx.IndexOf(face.Vertices[2]));
			}

			m.triangles = triangles.ToArray ();
			return m;
		}

		/**
		 * Take an original mesh, and double its faces, with normals going both directions.
		 * Need to do this in order to get the light to work properly.
		 **/
		public static void mirrorMesh(Mesh m) {
			List<Vector3> vertices = new List<Vector3> (m.vertices);
			List<Vector3> normales = new List<Vector3> (m.normals);
			List<Vector2> uvs = new List<Vector2> (m.uv);
			List<int> triangles = new List<int> (m.triangles);

			int size = vertices.Count;

			List<Vector3> newVertices = new List<Vector3> ();
			for (int i = 0; i < 2; i++) 
				newVertices.AddRange (vertices);
			m.vertices = newVertices.ToArray();

			List<Vector3> newNormales = new List<Vector3> ();
			newNormales.AddRange (normales);
			foreach (Vector3 normal in normales)
				newNormales.Add (-normal);
			m.normals = newNormales.ToArray();

			List<Vector2> newUVs = new List<Vector2> ();
			for (int i = 0; i < 2; i++)
				newUVs.AddRange (uvs);
			m.uv = newUVs.ToArray();

			//TODO (cmocan): This is inefficient, refactor.
			List<int> newTriangles = new List<int> ();
			newTriangles.AddRange (triangles);
			int[] triArray = triangles.ToArray ();
			for (int i = 0; i < triangles.Count; i += 3) {
				newTriangles.Add (triArray[i] + size);
				newTriangles.Add (triArray[i + 2] + size);
				newTriangles.Add (triArray[i + 1] + size);
			}
			m.triangles = newTriangles.ToArray();			
		}

		public static Vector3[] generateQuadSection(GameObject container, GameObject sphere) {
			return MeshGenerationUtils.generateQuadSection (container, sphere, 0, 1);
		}

		public static Vector3[] generateQuadSection(GameObject container, GameObject sphere, float yPosition, float scale) {
			GameObject perpendicularQuad = GameObject.CreatePrimitive (PrimitiveType.Quad);
			perpendicularQuad.name = "__Quad";

			// Get corners of the quad, so eventually I can build a mesh using those points.
			Vector3[] initialCorners = new Vector3[4] {
				new Vector3 (-.5f, -.5f, 0f), 
				new Vector3 (-.5f, .5f, 0f), 
				new Vector3 (.5f, -.5f, 0f), 
				new Vector3 (.5f, .5f, 0f) 
			};

			GameObject[] points = new GameObject[4] {
				new GameObject ("__EmptyGO"),
				new GameObject ("__EmptyGO"),
				new GameObject ("__EmptyGO"),
				new GameObject ("__EmptyGO")
			};

			for (int i = 0; i <= 3; i++) {
				points [i].transform.parent = perpendicularQuad.transform;
				points [i].transform.localPosition = initialCorners [i];
			}

			perpendicularQuad.transform.parent = sphere.transform;
			perpendicularQuad.transform.localPosition = new Vector3(0, yPosition, 0);

			perpendicularQuad.transform.parent = sphere.transform.parent.gameObject.transform;
			perpendicularQuad.transform.localRotation = Quaternion.identity;
			perpendicularQuad.transform.Rotate (new Vector3 (90f - sphere.transform.localEulerAngles.z / 2f, 90f, 0f));

			perpendicularQuad.transform.parent = sphere.transform;
			perpendicularQuad.transform.localScale = new Vector3(scale, scale, scale);

			Vector3[] currCorners = new Vector3[4];
			for (int i = 0; i < 4; i++) {
				points [i].transform.parent = container.transform;
				currCorners [i] = points [i].transform.position;
				UnityEngine.Object.Destroy (points [i]);
			}

			UnityEngine.Object.Destroy (perpendicularQuad);

			return currCorners;
		}
			
		public static GameObject[] generateParentChildSpheres(GameObject parentSphere, GameObject childSphere) {
			float parentRadius = sphereRadius * 1; //parentSphere.transform.localScale.x;
			float childRadius = sphereRadius * childSphere.transform.localScale.x;
			float scaleDif = (1 - childSphere.transform.localScale.x);
			float distance = Vector3.Distance (new Vector3 (0f, 0f, 0f), childSphere.transform.localPosition); // - parentRadius - childRadius;
			float radiusSmall = Mathf.Min (parentRadius, childRadius);

			float realK = distance / (2 * radiusSmall + Math.Abs(scaleDif) * sphereRadius) + 1f;
			float directionSgn = 1;
			if (childSphere.transform.localPosition.y < 0)
				directionSgn = -1;
			int k = Mathf.FloorToInt(realK) + 1;
			GameObject[] completionSpheres = new GameObject[k + 1];
			Vector3 currPos = new Vector3 (0f, 0f, 0f);
			for (int i = k - 1; i >= 1; i--) {
				int sizeFactor = i;
				float currSphereScale = 1.0f * sizeFactor / k * scaleDif + childSphere.transform.localScale.x;
				float prevSphereScale = currSphereScale + scaleDif / k;
				currPos += new Vector3 (0, directionSgn * sphereRadius * (currSphereScale + prevSphereScale), 0);
				completionSpheres[i] = GameObject.CreatePrimitive (PrimitiveType.Sphere);
				completionSpheres [i].name = "__Sphere";
				completionSpheres[i].transform.parent = parentSphere.transform;
				completionSpheres[i].transform.localPosition = new Vector3 (currPos.x, currPos.y, currPos.z);
				completionSpheres[i].transform.localScale = new Vector3 (currSphereScale, currSphereScale, currSphereScale);
				completionSpheres[i].transform.localRotation = Quaternion.identity;
			}

			// This should be negative (meaning that the last sphere goes further up the Y axis than half a radius inside childSphere)
			float extraDistance = (childSphere.transform.localPosition.y - childRadius * 0.5f) - (completionSpheres [1].transform.localPosition.y + completionSpheres[1].transform.localScale.x * sphereRadius);
			// The Y position of each sphere is going to be adjusted by + alpha * sphere_radius
			float alpha = extraDistance / ((k - 1) * (2 * radiusSmall + Math.Abs(scaleDif) * sphereRadius)); 
			float totalOffset = 0;
			for (int i = k - 1; i >= 1; i--) {
				float currRadius = sphereRadius * completionSpheres [i].transform.localScale.x;
				totalOffset += 2 * alpha * currRadius;
				completionSpheres [i].transform.localPosition += new Vector3 (0, directionSgn * totalOffset, 0);
			}

			// Make child go at the end of the parentSphere's children list, such that completion spheres + child sphere are sorted by Y.
			childSphere.transform.parent = null;
			childSphere.transform.parent = parentSphere.transform;

			return completionSpheres;
		}

	}
}

