using UnityEngine;
using System;
using System.Collections.Generic;       //Allows us to use Lists.
using Random = UnityEngine.Random;      //Tells Random to use the Unity Engine random number generator.

namespace Completed
	
{
	public class DrunkardWalk : MonoBehaviour
	{
		// Using Serializable allows us to embed a class with sub properties in the inspector.
		[Serializable]
		public class Count
		{
			public int minimum;             //Minimum value for our Count class.
			public int maximum;             //Maximum value for our Count class.
			
			
			//Assignment constructor.
			public Count (int min, int max)
			{
				minimum = min;
				maximum = max;
			}
		}
		
		
		public Count pathLenth = new Count (50, 300);					//Lower and upper limit for the length of the path the algorithm takes
		public GameObject[] floorTiles;                                 //Array of floor prefabs.
		public GameObject[] wallTiles;                                  //Array of wall prefabs.
		public GameObject[] outerWallTiles;                             //Array of outer tile prefabs.

		public bool endEnabled = true;
		public GameObject endTile;

		public bool MirrorsEnabled = false;
		public Count numOfMirrors = new Count (1, 2);					//Lower and upper limit for the number of mirrors in the 
		public GameObject[] mirrorTiles;

		private Transform boardHolder;                                  //A variable to store a reference to the transform of our Board object.
		private List <Vector3> gridPositions = new List <Vector3> ();   //A list of possible locations to place tiles.
		private List <Vector3> wallPositions = new List <Vector3> ();
		
		
		//Clears our list gridPositions and prepares it to generate a new board.
		void InitialiseList ()
		{
			//Clear our lists.
			gridPositions.Clear ();
			wallPositions.Clear ();
			
			int length = Random.Range (pathLenth.minimum, pathLenth.maximum);
			int currentX = 0;
			int currentY = 0;


			gridPositions.Add(new Vector3(currentX, 0f, currentY));

			int randomDirection;

			//Expansion Algorithm
			for (int i = 0; i < length; i++) 
			{
				randomDirection = Random.Range (0, 4);

				if(randomDirection == 0)		//North
				{
					currentY = currentY + 5;
				}
				if(randomDirection == 1)	//East
				{
					currentX = currentX + 5;
				}
				if(randomDirection == 2)	//South
				{
					currentY = currentY - 5;
				}
				if(randomDirection == 3)	//West
				{
					currentX = currentX - 5;
				}

				if(gridPositions.Contains(new Vector3(currentX, currentY, 0f)))
				{
					i--;
				}
				else
				{
					gridPositions.Add(new Vector3(currentX, currentY, 0f));
				}

			}
		}
		
		
		//Sets up the outer walls and floor (background) of the game board.
		void BoardSetup ()
		{
			//CleanBoard ();
			//Instantiate Board and set boardHolder to its transform.
			boardHolder = new GameObject ("Board").transform;

			//Setup floor tiles
			for (int i = 0; i < gridPositions.Count; i++) 
			{
				//Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
				GameObject toInstantiate = floorTiles [Random.Range (0, floorTiles.Length)];

				//Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
				GameObject instance =
					Instantiate (toInstantiate, new Vector3 (gridPositions[i].x, 0f, gridPositions[i].y), Quaternion.identity) as GameObject;
				
				//Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
				instance.transform.SetParent (boardHolder);
			}

			//Setup walls
			for (int i = 0; i < gridPositions.Count; i++) 
			{
				Vector3 northOf = gridPositions[i]; northOf.y = northOf.y + 5;
				Vector3 southOf = gridPositions[i]; southOf.y = southOf.y - 5;
				Vector3 eastOf = gridPositions[i]; eastOf.x = eastOf.x + 5;
				Vector3 westOf = gridPositions[i]; westOf.x = westOf.x - 5;

				if(!gridPositions.Contains(northOf) && !wallPositions.Contains(northOf) )
				{
					wallPositions.Add(northOf);
				}
				if(!gridPositions.Contains(southOf) && !wallPositions.Contains(southOf) )
				{
					wallPositions.Add(southOf);
				}
				if(!gridPositions.Contains(eastOf) && !wallPositions.Contains(eastOf) )
				{
					wallPositions.Add(eastOf);
				}
				if(!gridPositions.Contains(westOf) && !wallPositions.Contains(westOf) )
				{
					wallPositions.Add(westOf);
				}
			}

			//Walls posted
			for (int i = 0; i < wallPositions.Count; i++) 
			{
				//Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
				GameObject toInstantiate = wallTiles [Random.Range (0, wallTiles.Length)];
				
				//Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
				GameObject instance =
					Instantiate (toInstantiate, new Vector3 (wallPositions[i].x, 0f, wallPositions[i].y), Quaternion.identity) as GameObject;
				
				//Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
				instance.transform.SetParent (boardHolder);
			}

			//Mirrors
			if (MirrorsEnabled) 
			{
				int totalMirrors = Random.Range (numOfMirrors.minimum, numOfMirrors.maximum);
				for (int i = 0; i < totalMirrors; i++) {
					Vector3 randomPosition = RandomPosition ();

					//Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
					GameObject toInstantiate = mirrorTiles [Random.Range (0, mirrorTiles.Length)];
					
					//Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
					GameObject instance =
						Instantiate (toInstantiate, new Vector3 (randomPosition.x, 0f, randomPosition.y), Quaternion.identity) as GameObject;
					
					//Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
					instance.transform.SetParent (boardHolder);
					instance.transform.Rotate (new Vector3 (0f, Random.Range (0, 360), 0f));
				}
			}

			//End game
			if (endEnabled) 
			{
				Vector3 randomPosition = RandomPosition ();
				
				//Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
				GameObject toInstantiate = endTile;
				
				//Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
				GameObject instance =
					Instantiate (toInstantiate, new Vector3 (randomPosition.x, 0f, randomPosition.y), Quaternion.identity) as GameObject;
				
				//Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
				instance.transform.SetParent (boardHolder);
			}
		}
		
		
		//RandomPosition returns a random position from our list gridPositions.
		Vector3 RandomPosition ()
		{
			//Declare an integer randomIndex, set it's value to a random number between 0 and the count of items in our List gridPositions.
			int randomIndex = Random.Range (0, gridPositions.Count);
			
			//Declare a variable of type Vector3 called randomPosition, set it's value to the entry at randomIndex from our List gridPositions.
			Vector3 randomPosition = gridPositions[randomIndex];
			
			//Remove the entry at randomIndex from the list so that it can't be re-used.
			gridPositions.RemoveAt (randomIndex);
			
			//Return the randomly selected Vector3 position.
			return randomPosition;
		}
		
		
		//LayoutObjectAtRandom accepts an array of game objects to choose from along with a minimum and maximum range for the number of objects to create.
		void LayoutObjectAtRandom (GameObject[] tileArray, int minimum, int maximum)
		{
			//Choose a random number of objects to instantiate within the minimum and maximum limits
			int objectCount = Random.Range (minimum, maximum+1);
			
			//Instantiate objects until the randomly chosen limit objectCount is reached
			for(int i = 0; i < objectCount; i++)
			{
				//Choose a position for randomPosition by getting a random position from our list of available Vector3s stored in gridPosition
				Vector3 randomPosition = RandomPosition();
				
				//Choose a random tile from tileArray and assign it to tileChoice
				GameObject tileChoice = tileArray[Random.Range (0, tileArray.Length)];
				
				//Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
				Instantiate(tileChoice, randomPosition, Quaternion.identity);
			}
		}
		
		
		//SetupScene initializes our level and calls the previous functions to lay out the game board
		public void SetupScene (int level)
		{
			pathLenth.minimum = (level + 4) * (level + 4);
			pathLenth.maximum = (level + 4) * (level + 4) * (level + 4);

			//Reset our list of gridpositions.
			InitialiseList ();

			//Creates the outer walls and floor.
			BoardSetup ();
		}
	}
}