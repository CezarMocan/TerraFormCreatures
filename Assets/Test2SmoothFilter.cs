using UnityEngine;
using System.Collections;
using Skeleton;

public class Test2SmoothFilter : MonoBehaviour {

	private Mesh sourceMesh;
	private Mesh workingMesh;

	void Start () {
		MeshFilter meshfilter = gameObject.GetComponentInChildren<MeshFilter>();

		// Clone the cloth mesh to work on
		sourceMesh = new Mesh();
		// Get the sourceMesh from the originalSkinnedMesh
		sourceMesh = meshfilter.mesh;
		// Clone the sourceMesh 
		workingMesh = MeshGenerationUtils.cloneMesh(sourceMesh);
		// Reference workingMesh to see deformations
		meshfilter.mesh = workingMesh;


		// Apply Laplacian Smoothing Filter to Mesh
		int iterations = 1;
		for(int i=0; i<iterations; i++)
			//workingMesh.vertices = SmoothFilter.laplacianFilter(workingMesh.vertices, workingMesh.triangles);
			workingMesh.vertices = SmoothFilter.hcFilter(sourceMesh.vertices, workingMesh.vertices, workingMesh.triangles, 0.0f, 0.5f);
	}

	void Update () {
	
	}
}
