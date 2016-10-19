using UnityEngine;
using System.Collections;
using MeshGeneratorNS;

public class CubeGenerator : MonoBehaviour {

	// Use this for initialization
	void Start () {
		CubeGeneratorStatic.generateCube (gameObject);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
