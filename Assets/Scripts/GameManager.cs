using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Completed
{
	using System.Collections.Generic;       //Allows us to use Lists. 
	
	public class GameManager : MonoBehaviour
	{
		public enum EventType
		{
			DATA_EVENT
		}

		public class GameEvent
		{
			public EventType type;
			public int data;
		}

		public static GameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.
		public int level = 1;                                  //Current level number, expressed in game as "Day 1".
		private Text levelText; 


		public GameObject player;


		private int MapMode = 1;
		private bool mapMaxed = false;

		private BlockMapGeneration columnedRoomGeneration;                 
		private DrunkardWalk drunkardWalk;
		private BinarySpacePartioning binarySpacePartioning;

		//Observer fields
		List<IObserver> observers;

		public void Attach(IObserver observer)
		{
			observers.Add(observer);
		}

		public void Remove(IObserver observer)
		{
			observers.Remove (observer);
		}

		//Awake is always called before any Start functions
		void Awake()
		{
			observers = new List<IObserver> ();

			levelText = GameObject.Find ("LevelText").GetComponent<Text>();
			levelText.text = "Level: " + level;

			//Check if instance already exists
			if (instance == null)
				
				//if not, set instance to this
				instance = this;
			
			//If instance already exists and it's not this:
			else if (instance != this)
				
				//Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
				Destroy(gameObject);    
			
			//Sets this to not be destroyed when reloading scene
			DontDestroyOnLoad(gameObject);
			
			//Get a component reference to the attached BoardManager script
			columnedRoomGeneration = GetComponent<BlockMapGeneration>();
			drunkardWalk = GetComponent<DrunkardWalk> ();
			binarySpacePartioning = GetComponent<BinarySpacePartioning> ();

			
			//Call the InitGame function to initialize the first level 
			InitGame();
		}
		
		//Initializes the game for each level.
		void InitGame()
		{
			//Call the SetupScene function of the BoardManager script, pass it current level number.
			if (MapMode == 0) 
			{
				columnedRoomGeneration.SetupScene (level);
			} 
			else if (MapMode == 1) 
			{
				drunkardWalk.SetupScene (level);
			} 
			else if (MapMode == 2) 
			{
				binarySpacePartioning.SetupScene(level);
			}

			//Binary Space Partioning handles player reset
			if (MapMode != 2) {
				ResetPlayer ();
			}
		}

		void NewPlayer()
		{
			//Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
			GameObject toInstantiate = player;
			
			//Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
			Instantiate (toInstantiate, new Vector3 (0f, 0f, 0f), Quaternion.identity);
		}

		//Sets Character
		void ResetPlayer()
		{
			if (GameObject.Find("Player(Clone)") == null) 
			{
				NewPlayer ();
			} 
			else 
			{
				GameObject.Find("MultipurposeCameraRig").transform.position = new Vector3(0f,0f,0f);
				GameObject.Find("MultipurposeCameraRig").transform.Rotate(0f, 0f, 0f);

				GameObject.Find ("ThirdPersonController").transform.position = new Vector3(0f,0f,0f);
				GameObject.Find ("ThirdPersonController").transform.Rotate(0f, 0f, 0f);
			}

		}
		
		void CleanBoard()
		{
			GameObject boardHolder = GameObject.Find("Board");

			Destroy (boardHolder);
		}

		public void NextLevel ()
		{
			level++;
			levelText.text = "Level: " + level;

			CleanBoard ();

			InitGame ();
		}

		public void ColumnedRoomGeneration()
		{
			MapMode = 0;
			CleanBoard ();
			columnedRoomGeneration.SetupScene (level);
			ResetPlayer ();
		}

		public void DrunkardsWalk()
		{
			MapMode = 1;
			CleanBoard();
			drunkardWalk.SetupScene (level);
			ResetPlayer ();
		}

		public void BinarySpacePartioning()
		{
			MapMode = 2;
			CleanBoard ();
			binarySpacePartioning.SetupScene (level);
		}

		//Update is called every frame.
		void Update()
		{
			if (Input.GetKeyDown (KeyCode.M)) 
			{
				Camera mapCamera = GameObject.Find ("MapCamera").GetComponent<Camera>();
				mapCamera.enabled = !mapCamera.enabled;
			}

			if (Input.GetKey (KeyCode.Equals)) 
			{
				Camera mapCamera = GameObject.Find ("MapCamera").GetComponent<Camera> ();
				mapCamera.fieldOfView++;
			}

			if(Input.GetKey (KeyCode.Minus) )
			{
				Camera mapCamera = GameObject.Find ("MapCamera").GetComponent<Camera>();
				if (mapCamera.fieldOfView > 1) 
				{
					mapCamera.fieldOfView--;
				}
			}

			if (Input.GetKeyDown (KeyCode.F)) 
			{
				Camera mapCamera = GameObject.Find ("MapCamera").GetComponent<Camera> ();

				if(!mapMaxed)
				{
					mapMaxed = true;
					mapCamera.rect = new Rect(0f, 0f, 1f, 1f);
				}
				else
				{
					mapMaxed = false;
					mapCamera.rect = new Rect(0.8f, 0.7f, 0.2f, 0.3f);
				}
			}


		}

		void Notify(GameEvent e)
		{
			if (observers != null) 
			{
				for (int i = 0; i < observers.Count; i++) 
				{
					observers [i].onNotify (e);
				}
			}
		}
	}
}