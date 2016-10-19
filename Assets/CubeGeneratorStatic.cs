using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Skeleton;

namespace MeshGeneratorNS {
	public class CubeGeneratorStatic {

		public GameObject gameObject;
		private MeshFilter filter;
		private Mesh mesh;
		private Vector3 p0, p1, p2, p3, p4, p5, p6, p7, p8;

		private List<Vector3> vertices;
		private List<Vector3> normales;
		private List<Vector2> uvs;
		private List<int> triangles;

		private int noMeshes;
		private static int verticesPerMesh = 24;

		public CubeGeneratorStatic(GameObject gameObject) {
			this.gameObject = gameObject;
			this.filter = gameObject.AddComponent< MeshFilter >();
			this.mesh = filter.mesh;
			this.mesh.Clear();

			this.vertices = new List<Vector3> ();
			this.normales = new List<Vector3> ();
			this.uvs = new List<Vector2> ();
			this.triangles = new List<int>(); 

			this.noMeshes = 0;
		}

		public MeshFilter getMeshFilter() {
			return this.filter;
		}

		public Mesh getMesh() {
			return this.mesh;
		}

		public void finalizeMesh() {
			// Call after appending all the components to the mesh, such that the mesh is actually created.
			// Create the mesh on both the inside and the outside—opposite normals
			// Such that I don't have to actually compute them, since I have no idea how.
			mesh.vertices = this.vertices.ToArray();
			mesh.normals = this.normales.ToArray();
			mesh.uv = this.uvs.ToArray();
			mesh.triangles = this.triangles.ToArray();
			MeshGenerationUtils.mirrorMesh(mesh);

			//mesh.RecalculateNormals ();
			mesh.RecalculateBounds();
			mesh.Optimize();

			Material material = new Material(Shader.Find("Diffuse"));
			MeshRenderer meshRenderer = this.gameObject.AddComponent<MeshRenderer> ();
			meshRenderer.material = material;
		}
						
		public void appendMesh(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Vector3 p5, Vector3 p6, Vector3 p7) {
			this.p0 = p0; this.p1 = p1; this.p2 = p2; this.p3 = p3;
			this.p4 = p4; this.p5 = p5; this.p6 = p6; this.p7 = p7;
			this.appendVertices ();
			//this.appendNormales ();
			this.appendUvs ();
			this.appendTriangles ();
			this.noMeshes++;
		}
			
		private void appendVertices() {
			List<Vector3> currVertices = new List<Vector3> {
				// Bottom
				p0, p1, p2, p3,
				// Left
				p7, p4, p0, p3,
				// Front
				p4, p5, p1, p0,
				// Back
				p6, p7, p3, p2,
				// Right
				p5, p6, p2, p1,
				// Top
				p7, p6, p5, p4
			};
			this.vertices.AddRange (currVertices);
		}

		private void appendNormales() {
			Vector3 up 	= Vector3.up;
			Vector3 down 	= Vector3.down;
			Vector3 front 	= Vector3.forward;
			Vector3 back 	= Vector3.back;
			Vector3 left 	= Vector3.left;
			Vector3 right 	= Vector3.right;

			List<Vector3> currNormales = new List<Vector3> {
				// Bottom
				//down, down, down, down,
				up, up, up, up,
				// Left
				left, left, left, left,
				// Front
				front, front, front, front,
				// Back
				back, back, back, back,
				// Right
				right, right, right, right,
				// Top
				down, down, down, down
			};

			this.normales.AddRange (currNormales);
		}

		private void appendUvs() {
			Vector2 _00 = new Vector2( 0f, 0f );
			Vector2 _10 = new Vector2( 1f, 0f );
			Vector2 _01 = new Vector2( 0f, 1f );
			Vector2 _11 = new Vector2( 1f, 1f );

			List<Vector2> currUvs = new List<Vector2> {
				// Bottom
				_11, _01, _00, _10,
				// Left
				_11, _01, _00, _10,
				// Front
				_11, _01, _00, _10,
				// Back
				_11, _01, _00, _10,
				// Right
				_11, _01, _00, _10,
				// Top
				_11, _01, _00, _10,
			};

			this.uvs.AddRange (currUvs);
		}

		private void appendTriangles() {
			int[] currTriangles = new int[] {
				// Bottom
				3, 1, 0,
				3, 2, 1,			

				// Left
				3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1,
				3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,

				// Front
				3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
				3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,

				// Back
				3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
				3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,

				// Right
				3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
				3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,

				// Top
				3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
				3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5,
			};
			// Make the triangles in the current appended mesh reference the vertices of the current mesh
			for (int i = 0; i < 24; i++) {
				currTriangles [i] += this.noMeshes * CubeGeneratorStatic.verticesPerMesh;
			}

			this.triangles.AddRange (currTriangles);
		}

		public static void generateCubeWithCorners(GameObject gameObject, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Vector3 p5, Vector3 p6, Vector3 p7) {
			MeshFilter filter = gameObject.AddComponent< MeshFilter >();
			Mesh mesh = filter.mesh;
			mesh.Clear();

			#region Vertices
			Vector3[] vertices = new Vector3[]
			{
				// Bottom
				p0, p1, p2, p3,

				// Left
				p7, p4, p0, p3,

				// Front
				p4, p5, p1, p0,

				// Back
				p6, p7, p3, p2,

				// Right
				p5, p6, p2, p1,

				// Top
				p7, p6, p5, p4
			};
			#endregion

			#region Normales
			Vector3 up 	= Vector3.down;
			Vector3 down 	= Vector3.up;
			Vector3 front 	= Vector3.forward;
			Vector3 back 	= Vector3.back;
			Vector3 left 	= Vector3.right;
			Vector3 right 	= Vector3.right;

			Vector3[] normales = new Vector3[]
			{
				// Bottom
				down, down, down, down,

				// Left
				left, left, left, left,

				// Front
				front, front, front, front,

				// Back
				back, back, back, back,

				// Right
				right, right, right, right,

				// Top
				up, up, up, up
			};
			#endregion	

			#region UVs
			Vector2 _00 = new Vector2( 0f, 1f );
			Vector2 _10 = new Vector2( 1f, 0f );
			Vector2 _01 = new Vector2( 0f, 1f );
			Vector2 _11 = new Vector2( 1f, 1f );

			Vector2[] uvs = new Vector2[]
			{
				// Bottom
				_11, _01, _00, _10,

				// Left
				_11, _01, _00, _10,

				// Front
				_11, _01, _00, _10,

				// Back
				_11, _01, _00, _10,

				// Right
				_11, _01, _00, _10,

				// Top
				_11, _01, _00, _10,
			};
			#endregion

			#region Triangles
			int[] triangles = new int[]
			{
				// Bottom
				3, 1, 0,
				3, 2, 1,			

				// Left
				3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1,
				3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,

				// Front
				3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
				3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,

				// Back
				3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
				3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,

				// Right
				3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
				3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,

				// Top
				3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
				3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5,

			};
			#endregion

			mesh.vertices = vertices;
			mesh.normals = normales;
			mesh.uv = uvs;
			mesh.triangles = triangles;

			mesh.RecalculateBounds();
			mesh.Optimize();

			Material material = new Material(Shader.Find("Diffuse"));
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer> ();
			meshRenderer.material = material;
		}

		public static void generateCube(GameObject gameObject) {
			// Courtesy of: http://wiki.unity3d.com/index.php/ProceduralPrimitives#C.23_-_Box
			// You can change that line to provide another MeshFilter
			float length = 1f;
			float width = 1f;
			float height = 1f;

			Vector3 p0 = new Vector3( -length * .5f,	-width * .5f, height * .5f );
			Vector3 p1 = new Vector3( length * .5f, 	-width * .5f, height * .5f );
			Vector3 p2 = new Vector3( length * .5f, 	-width * .5f, -height * .5f );
			Vector3 p3 = new Vector3( -length * .5f,	-width * .5f, -height * .5f );	

			Vector3 p4 = new Vector3( -length * .5f,	width * .5f,  height * .5f );
			Vector3 p5 = new Vector3( length * .5f, 	width * .5f,  height * .5f );
			Vector3 p6 = new Vector3( length * .5f, 	width * .5f,  -height * .5f );
			Vector3 p7 = new Vector3( -length * .5f,	width * .5f,  -height * .5f );

			generateCubeWithCorners(gameObject, p0, p1, p2, p3, p4, p5, p6, p7);
		}
	}
}