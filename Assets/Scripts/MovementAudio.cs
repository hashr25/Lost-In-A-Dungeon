using UnityEngine;
using System.Collections;

public class MovementAudio : MonoBehaviour 
{
	public AudioClip[] footsteps;
	/*
	 * 0 = Left Run
	 * 1 = Right Run
	 * 2 = Left Sneak
	 * 3 = Right Sneak
	 */

	public AudioClip jump;
	public AudioClip land;

	void PlayFootstep()
	{
		GetComponent<AudioSource>().clip = footsteps [Random.Range(0,footsteps.Length)];
		GetComponent<AudioSource>().Play ();
	}

	void RunLeft()
	{
		GetComponent<AudioSource>().clip = footsteps [0];
		GetComponent<AudioSource>().Play ();
	}

	void RunRight()
	{
		GetComponent<AudioSource>().clip = footsteps [1];
		GetComponent<AudioSource>().Play ();
	}

	void SneakLeft()
	{
		GetComponent<AudioSource>().clip = footsteps [2];
		GetComponent<AudioSource>().Play ();
	}

	void SneakRight()
	{
		GetComponent<AudioSource>().clip = footsteps [3];
		GetComponent<AudioSource>().Play ();
	}
	
	void Jump()
	{
		GetComponent<AudioSource>().clip = jump;
		GetComponent<AudioSource>().Play ();
	}
	
	void Land()
	{
		GetComponent<AudioSource>().clip = land;
		GetComponent<AudioSource>().Play ();
	}
}
