using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
		}

		public SkeletonMutation(List<MeshedSkeleton> skeletons, float mutationChance = 0.1f, float scaleMutationAmplitude = 0.8f,  float lengthMutationAmplitude = 0.8f, float rotationMutationAmplitude = 30f): 
		this(skeletons [0], mutationChance, scaleMutationAmplitude, lengthMutationAmplitude, rotationMutationAmplitude) {
			this.crossBreeds.AddRange (skeletons.GetRange(1, skeletons.Count - 1));
		}

		public GameObject getMutatedObject() {
			GameObject mutant = Object.Instantiate (this.gameObject);
			mutant.name = "Mutation" + mutant.GetInstanceID ().ToString ();
			this.skeletonMutation (mutant);

			return mutant;
		}
			
		private void skeletonMutation(GameObject gameObject) {
			this.dfsMutation (gameObject, 0);
		}

		//TODO (cmocan): When I mutate a node, I need to un-mutate its children. So pass down
		// in the DFS the composite scale, transform and rotation so far and divide node transform
		// by that at each step.
		private void dfsMutation(GameObject node, int lvl) {
			List<Transform> originalChildren = new List<Transform> ();
			foreach (Transform ch in node.transform) {
				originalChildren.Add (ch);
			}
			foreach (Transform ch in originalChildren) {
				GameObject child = ch.gameObject;
				dfsMutation (child, lvl + 1);
			}

			this.mutateNodeAttributes (node, lvl);
		}

		private void mutateNodeAttributes(GameObject node, int lvl) {
			this.mutateRotation (node);
			this.mutateScale (node);
			if (lvl > 0) {
				this.mutateEdgeLength (node);
				this.mutateCloneNode (node);
			}
		}

		//TODO (cmocan): Need to be smarter about this, so it doesn't fuck up the shape.
		private void mutateCloneNode(GameObject node) {
			if (Random.Range (0f, 1f) > mutationChance) {
				Debug.Log ("not clone");
				return;
			}

			Debug.Log("clone");
			
			GameObject parent = node.transform.parent.gameObject;
			GameObject newNode = GameObject.CreatePrimitive (PrimitiveType.Sphere); 
			newNode.transform.parent = parent.transform;
			newNode.transform.localScale = node.transform.localScale;
			newNode.transform.localPosition = node.transform.localPosition;
			newNode.transform.localEulerAngles = node.transform.localEulerAngles;

			float posX = MathUtils.getGaussianInInterval (-node.transform.localPosition.y / 2f, node.transform.localPosition.y / 2f);	
			float posY = MathUtils.getGaussianInInterval (0, 2 * node.transform.localPosition.y);	

			newNode.transform.localPosition = new Vector3(
				posX,
				posY,  
				node.transform.localPosition.z);


			float scaleFactor = MathUtils.getGaussianInInterval (1f - this.scaleMutationAmplitude, 1f + this.scaleMutationAmplitude);	
			newNode.transform.localScale = node.transform.localScale * scaleFactor;
		}

		private void mutateEdgeLength(GameObject node) {
			if (Random.Range (0f, 1f) > mutationChance)
				return;
			float lengthFactor = MathUtils.getGaussianInInterval (1f - this.lengthMutationAmplitude, 1f + this.lengthMutationAmplitude);	
			node.transform.localPosition *= lengthFactor;
		}

		private void mutateRotation(GameObject node) {
			float rotationX = node.transform.localEulerAngles.x;
			float rotationY = node.transform.localEulerAngles.y;
			float rotationZ = node.transform.localEulerAngles.z;

			if (Random.Range (0f, 1f) < (this.mutationChance / 3f))
				rotationX += MathUtils.getGaussianInInterval (-this.rotationMutationAmplitude, this.rotationMutationAmplitude);

			/*
			if (Random.Range (0f, 1f) < (this.mutationChance / 3f))
				rotationY += MathUtils.getGaussianInInterval (-this.rotationMutationAmplitude, this.rotationMutationAmplitude);
			*/

			if (Random.Range (0f, 1f) < (this.mutationChance / 3f))
				rotationZ += MathUtils.getGaussianInInterval (-this.rotationMutationAmplitude, this.rotationMutationAmplitude);
			
			node.transform.localEulerAngles = new Vector3 (rotationX, rotationY, rotationZ);
		}

		private void mutateScale(GameObject node) {
			if (Random.Range (0f, 1f) > mutationChance)
				return;
			float scaleFactor = MathUtils.getGaussianInInterval (1f - this.scaleMutationAmplitude, 1f + this.scaleMutationAmplitude);
			node.transform.localScale *= scaleFactor;
		}
	}
}