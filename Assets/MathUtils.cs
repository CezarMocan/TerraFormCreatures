using UnityEngine;
using System.Collections;

namespace Skeleton {
	public class MathUtils {
		/**
		 * Courtesy of http://www.alanzucconi.com/2015/09/16/how-to-sample-from-a-gaussian-distribution/
		 * 
		 * */
		public static float nextGaussian() {
			float v1, v2, s;
			do {
				v1 = 2.0f * Random.Range(0f,1f) - 1.0f;
				v2 = 2.0f * Random.Range(0f,1f) - 1.0f;
				s = v1 * v1 + v2 * v2;
			} while (s >= 1.0f || s == 0f);

			s = Mathf.Sqrt((-2.0f * Mathf.Log(s)) / s);
			//Debug.Log (((v1 * s) + Mathf.PI) / (2f * Mathf.PI));
			return ((v1 * s) + Mathf.PI) / (2f * Mathf.PI);
		}

		public static float getGaussianInInterval(float left, float right) {
			return MathUtils.nextGaussian () * (right - left) + left;
		}
	}
}