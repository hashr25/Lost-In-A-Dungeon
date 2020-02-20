using UnityEngine;
using System.Collections;


public class NextLevel : MonoBehaviour, IObserver
{
	Completed.GameManager gameManager;

	void OnTriggerEnter (Collider col)
	{
		if ( col.gameObject.name == "ThirdPersonController")  
		{
			gameManager.NextLevel();
		}
	}

	void Start()
	{
		gameManager = GameObject.Find ("GameManager").GetComponent<Completed.GameManager>();
	}

	void Awake()
	{
		GameObject.Find ("GameManager").GetComponent<Completed.GameManager> ().Attach (this);
	}

	public void onNotify(Completed.GameManager.GameEvent e)
	{
		if (e.type == Completed.GameManager.EventType.DATA_EVENT) 
		{
			//Debug.Log (e.data);
		}
	}
}
