using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Skeleton {
	/**
	 * This class takes in a non-altered skeleton GameObject (an asset)
	 * and creates a mutation of it.
	 * */

	public class SkeletonMutation {
		public MeshedSkeleton skeleton;
		public GameObject gameObject;
		public List<MeshedSkeleton> crossBreeds;
		public float mutationChance;
		public float scaleMutationAmplitude, lengthMutationAmplitude, rotationMutationAmplitude;
		public static String CLONED_NODE_NAME = "MutationClone";
		public GameObject mutant;

		/**
		 * GameObject go: GameObject to be mutated. 
		 * float mutationChance: float between 0 and 1, representing the probability that a node gets mutated.
		 * float scaleMutationAmplitude: float representing by how much attributes of the node will be changed by the mutation.
		 * 		(e.g. 0.1 means that mutated attribute have a new value within 90% and 110% of the current one)
		 * float rotationMutationAmplitude: max amount in degrees for mutation
		 * */
		public SkeletonMutation(MeshedSkeleton skeleton, float mutationChance = 0.9f, float scaleMutationAmplitude = 0.8f,  float lengthMutationAmplitude = 0.8f, float rotationMutationAmplitude = 50f) {
			this.skeleton = skeleton;
			this.gameObject = skeleton.getOriginalSkeleton();
			this.mutationChance = Mathf.Min(mutationChance, 1f);
			this.scaleMutationAmplitude = scaleMutationAmplitude;
			this.lengthMutationAmplitude = lengthMutationAmplitude;
			this.rotationMutationAmplitude = rotationMutationAmplitude;
			this.crossBreeds = new List<MeshedSkeleton> ();

			this.mutant = UnityEngine.Object.Instantiate (this.gameObject);
			this.mutant.name = "Mutation" + mutant.GetInstanceID ().ToString ();
			this.skeletonMutation (this.mutant);
		}

		public SkeletonMutation(List<MeshedSkeleton> skeletons, float mutationChance = 0.2f, float scaleMutationAmplitude = 0.2f,  float lengthMutationAmplitude = 0.8f, float rotationMutationAmplitude = 30f): 
		this(skeletons [0], mutationChance, scaleMutationAmplitude, lengthMutationAmplitude, rotationMutationAmplitude) {
			this.crossBreeds.AddRange (skeletons.GetRange(1, skeletons.Count - 1));
		}

		public GameObject getMutatedObject() {
			return this.mutant;
		}
			
		private void skeletonMutation(GameObject gameObject) {
			if (this.crossBreeds.Count > 0) {
				GameObject node2 = this.crossBreeds [0].getOriginalSkeleton ();
				Debug.Log ("two parents");
				//this.dfsMutationTwoParents (gameObject, node2, 0);
			} else {
				Debug.Log ("one parent");
				this.dfsMutation (gameObject, 0, true);
			}
		}

		//TODO (cmocan): When I mutate a node, I need to un-mutate its children. So pass down
		// in the DFS the composite scale, transform and rotation so far and divide node transform
		// by that at each step.
		private void dfsMutation(GameObject node, int lvl, bool isOnlyChild) {
			List<Transform> nodeProperChildren = new List<Transform> ();
			List<Transform> nodeMutatedChildren = new List<Transform> ();
			foreach (Transform ch in node.transform) {
				if (ch.name.Contains(SkeletonMutation.CLONED_NODE_NAME)) {
					nodeMutatedChildren.Add(ch);
				} else {
					nodeProperChildren.Add (ch);
				}
			}

			List<Transform> allNodeChildren = new List<Transform> ();
			allNodeChildren.AddRange (nodeProperChildren);
			allNodeChildren.AddRange (nodeMutatedChildren);
				
			foreach (Transform ch in allNodeChildren) {
				GameObject child = ch.gameObject;
				dfsMutation (child, lvl + 1, (allNodeChildren.Count == 1));
			}

			this.mutateNodeAttributes (node, lvl, isOnlyChild, allNodeChildren);
		}

		private void mutateNodeAttributes(GameObject node, int lvl, bool isOnlyChild, List<Transform> nodeChildren) {
			this.mutateRotation (node);
			this.mutateScale (node);
			if (lvl > 0) {
				this.mutateEdgeLength (node);
			}
			if (lvl > 0 && isOnlyChild) {
				this.mutateAddRemoveNode (node);
			}
			if (nodeChildren.Count > 1) {
				this.mutateAddRemoveSubtrees (node, nodeChildren);
			}
		}
			
		private void mutateAddRemoveSubtrees(GameObject node, List<Transform> nodeChildren) {
			if (UnityEngine.Random.Range (0f, 1f) > mutationChance) {
				return;
			}

			// Chance of 1/2 to remove a subtree, 1/2 to add a subtree
			if (UnityEngine.Random.Range (0f, 1f) > 0.5f) {
				Debug.Log("remove branch " + node.name);
				int removePosition = Mathf.FloorToInt (UnityEngine.Random.Range (0f, nodeChildren.Count));
				Transform toRemove = nodeChildren [removePosition];
				nodeChildren.Remove (toRemove);
				this.dfsRemoveSubtree (toRemove.gameObject);
			} else {
				Debug.Log("Add branch " + node.name);

				nodeChildren.Sort ((x, y) => x.transform.localPosition.x.CompareTo (y.transform.localPosition.x));

				Debug.Log("Children count " + nodeChildren.Count);

				int addPosition = Mathf.FloorToInt (UnityEngine.Random.Range (0f, nodeChildren.Count - 1));
				GameObject left = nodeChildren [addPosition].gameObject;
				GameObject right = nodeChildren [addPosition + 1].gameObject;
				GameObject newSubtree = (GameObject) UnityEngine.Object.Instantiate (left, left.transform.parent, true);
				newSubtree.transform.localRotation = Quaternion.identity;
				newSubtree.transform.localPosition = (left.transform.localPosition + right.transform.localPosition) / 2f;
				newSubtree.transform.localEulerAngles = (left.transform.localEulerAngles + right.transform.localEulerAngles) / 2f;

				int index = 0;
				foreach (Transform child in nodeChildren) {
					if (index == addPosition) {
						child.localPosition += (child.localPosition - newSubtree.transform.localPosition);
					} else {
						child.localPosition += (child.localPosition - newSubtree.transform.localPosition) / (2 * Mathf.Abs(index - addPosition));
					}
					index++;
				}

				//left.transform.localPosition += (left.transform.localPosition - newSubtree.transform.localPosition);
				//right.transform.localPosition += (right.transform.localPosition - newSubtree.transform.localPosition);
			}
		}

		private void mutateAddRemoveNode(GameObject node) {
			if (UnityEngine.Random.Range (0f, 1f) > mutationChance) {
				return;
			}

			if (UnityEngine.Random.Range (0f, 1f) >= 0.5f) {
				Debug.Log ("New node");
				// Add new node with probability 1/2
				GameObject parent = node.transform.parent.gameObject;
				GameObject newNode = GameObject.CreatePrimitive (PrimitiveType.Sphere); 
				newNode.name = SkeletonMutation.CLONED_NODE_NAME;
				newNode.transform.parent = parent.transform;

				Vector3 originalNodeScale = node.transform.localScale;
				Vector3 originalNodePosition = node.transform.localPosition;
				Vector3 originalNodeRotation = node.transform.localEulerAngles;
				//node.transform.parent = newNode.transform;

				newNode.transform.localScale = (originalNodeScale + new Vector3 (1f, 1f, 1f)) / 2f;
				newNode.transform.localPosition = new Vector3 (0, originalNodePosition.y, 0);
				newNode.transform.localEulerAngles = originalNodeRotation;

				node.transform.parent = newNode.transform;
				node.transform.localScale = originalNodeScale;
				node.transform.localPosition = originalNodePosition;
				node.transform.localEulerAngles = new Vector3 (0, 0, 0.1f);//-originalNodeRotation;//(node.transform.localEulerAngles / 2f);//originalNodeRotation;
			} else {
				// Delete current node with probability 1/2
				Debug.Log("Delete " + node.name);
				foreach (Transform child in node.transform) {
					child.parent = node.transform.parent.transform;
					child.localPosition /= 2f;
				}
				node.transform.parent = null;
				UnityEngine.Object.Destroy (node);
			}
		}

		private void mutateEdgeLength(GameObject node) {
			if (UnityEngine.Random.Range (0f, 1f) > mutationChance)
				return;
			float lengthFactor = MathUtils.getGaussianInInterval (1f - this.lengthMutationAmplitude, 1f + this.lengthMutationAmplitude);	
			node.transform.localPosition *= lengthFactor;
		}

		private void mutateRotation(GameObject node) {
			float rotationX = node.transform.localEulerAngles.x;
			float rotationY = node.transform.localEulerAngles.y;
			float rotationZ = node.transform.localEulerAngles.z;

			if (UnityEngine.Random.Range (0f, 1f) < (this.mutationChance / 3f))
				rotationX += MathUtils.getGaussianInInterval (-this.rotationMutationAmplitude, this.rotationMutationAmplitude);

			/*
			if (Random.Range (0f, 1f) < (this.mutationChance / 3f))
				rotationY += MathUtils.getGaussianInInterval (-this.rotationMutationAmplitude, this.rotationMutationAmplitude);
			*/

			if (UnityEngine.Random.Range (0f, 1f) < (this.mutationChance / 3f))
				rotationZ += MathUtils.getGaussianInInterval (-this.rotationMutationAmplitude, this.rotationMutationAmplitude);
			
			node.transform.localEulerAngles = new Vector3 (rotationX, rotationY, rotationZ);
		}
			
		private void mutateScale(GameObject node) {
			if (UnityEngine.Random.Range (0f, 1f) > mutationChance)
				return;
			float scaleFactor = MathUtils.getGaussianInInterval (1f - this.scaleMutationAmplitude, 1f + this.scaleMutationAmplitude);
			node.transform.localScale *= scaleFactor;

			fixScaleSubtree (node, 0, scaleFactor);
		}

		private void fixScaleSubtree(GameObject node, int lvl, float scaleFactor) {
			foreach (Transform child in node.transform) {
				child.localScale /= scaleFactor;
			}
		}

		private void dfsRemoveSubtree(GameObject node) {
			List<Transform> children = new List<Transform> ();
			foreach (Transform child in node.transform) {
				children.Add(child);
			}

			foreach (Transform child in children) {
				dfsRemoveSubtree (child.gameObject);
			}
			node.transform.parent = null;
			UnityEngine.Object.Destroy (node);
		}


		/*

		private void mutateScaleTwo(GameObject node, GameObject node2) {
			if (UnityEngine.Random.Range (0f, 1f) > mutationChance)
				return;
			float scaleFactor = MathUtils.getGaussianInInterval (1f - this.scaleMutationAmplitude, 1f + this.scaleMutationAmplitude);
			if (UnityEngine.Random.Range (0f, 1f) >= 0.5) {
				node.transform.localScale = node2.transform.localScale;
			}
			node.transform.localScale *= scaleFactor;
		}

		private void mutateRotationTwo(GameObject node, GameObject node2) {
			float rotationX = (UnityEngine.Random.Range (0f, 1f) >= 0.5f) ? node.transform.localEulerAngles.x : node2.transform.localEulerAngles.x;
			float rotationY = (UnityEngine.Random.Range (0f, 1f) >= 0.5f) ? node.transform.localEulerAngles.y : node2.transform.localEulerAngles.y;
			float rotationZ = (UnityEngine.Random.Range (0f, 1f) >= 0.5f) ? node.transform.localEulerAngles.z : node2.transform.localEulerAngles.z;

			if (UnityEngine.Random.Range (0f, 1f) < (this.mutationChance / 3f))
				rotationX += MathUtils.getGaussianInInterval (-this.rotationMutationAmplitude, this.rotationMutationAmplitude);

			if (UnityEngine.Random.Range (0f, 1f) < (this.mutationChance / 3f))
				rotationZ += MathUtils.getGaussianInInterval (-this.rotationMutationAmplitude, this.rotationMutationAmplitude);

			node.transform.localEulerAngles = new Vector3 (rotationX, rotationY, rotationZ);
		}
		*/

		/*
		// This assumes node and node2 have the same base skeleton
		private void dfsMutationTwoParents(GameObject node, GameObject node2, int lvl) {
			List<Transform> nodeChildren = new List<Transform> ();
			List<Transform> nodeMutatedChildren = new List<Transform> ();
			foreach (Transform ch in node.transform) {
				if (ch.name.Contains(SkeletonMutation.CLONED_NODE_NAME)) {
					nodeMutatedChildren.Add(ch);
				} else {
					nodeChildren.Add (ch);
				}
			}

			List<Transform> node2Children = new List<Transform> ();
			List<Transform> node2MutatedChildren = new List<Transform> ();
			foreach (Transform ch in node2.transform) {
				if (ch.name.Contains(SkeletonMutation.CLONED_NODE_NAME)) {
					node2MutatedChildren.Add(ch);
				} else {
					node2Children.Add (ch);
				}
			}				

			int node2Index = 0;
			Transform[] node2Array = node2Children.ToArray ();

			foreach (Transform ch in nodeChildren) {
				GameObject child = ch.gameObject;
				GameObject child2;
				if (node2Index < node2Children.Count) {
					child2 = node2Array [node2Index].gameObject;
					dfsMutationTwoParents (child, child2, lvl + 1);
				} else {
					dfsMutation (child, lvl + 1, (nodeChildren.Count == 1));
				}
			}

			this.mutateTwoNodeAttributes (node, node2, lvl);
		}
		*/

		/*
		private void mutateEdgeLengthTwo(GameObject node, GameObject node2) {
			if (UnityEngine.Random.Range (0f, 1f) > mutationChance)
				return;
			float lengthFactor = MathUtils.getGaussianInInterval (1f - this.lengthMutationAmplitude, 1f + this.lengthMutationAmplitude);	

			if (UnityEngine.Random.Range (0f, 1f) >= 0.5f) {
				node.transform.localPosition = node2.transform.localPosition;
			} 

			node.transform.localPosition *= lengthFactor;
		}
		*/

		/*
		private void mutateTwoNodeAttributes(GameObject node, GameObject node2, int lvl) {
			this.mutateRotationTwo (node, node2);
			this.mutateScaleTwo (node, node2);
			if (lvl > 0) {
				this.mutateEdgeLengthTwo (node, node2);
				//this.mutateCloneNode (node);
			}
		}
		*/
	}
}