using UnityEngine;
using System.Collections;

public class PerlinNoise {

	long seed;

	public PerlinNoise (long seed) {
		this.seed = seed;
	}

	private int random (int x, int range) {
		return (int) (((x+seed)^5) % range);
	}

	public int getNoise(int x, int range) {
		int chunkSize = 16; //distance between sampled points

		int chunkIndex = x / chunkSize;
		float prog = (x % chunkSize) / (chunkSize * 1f);

		//get the same points
		int left_random = random(chunkIndex, range);
		int right_random = random(chunkIndex + 1, range);

		float noise = (1 - prog) * left_random + prog * right_random;

		return Mathf.RoundToInt(noise);
	}
}
