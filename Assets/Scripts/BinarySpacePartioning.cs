using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;      //Tells Random to use the Unity Engine random number generator.

/**
 * Algorithm taken from http://gamedevelopment.tutsplus.com/tutorials/how-to-use-bsp-trees-to-generate-game-maps--gamedev-12268
 */

public class BinarySpacePartioning : MonoBehaviour 
{
	/// <summary>
	/// The height of the map.
	/// </summary>
	public int MapHeight = 40;

	/// <summary>
	/// The width of the map.
	/// </summary>
	public int MapWidth = 40;

	/// <summary>
	/// The maximum size of a leaf.
	/// </summary>
	public int MaximumLeafSize = 20;



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

	public GameObject[] floorTiles;                                 //Array of floor prefabs.
	public GameObject[] wallTiles;                                  //Array of wall prefabs.
	public GameObject[] outerWallTiles;                             //Array of outer tile prefabs.
	
	public bool endEnabled = true;
	public GameObject endTile;

	private Transform boardHolder;                                  //A variable to store a reference to the transform of our Board object.
	private List <Vector3> gridPositions = new List <Vector3> ();   //A list of possible locations to place tiles.
	private List <Vector3> wallPositions = new List <Vector3> ();

	public static List<Leaf> Leafs = new List<Leaf> ();
	Leaf MasterRoot;


	/*
	 * Room Class
	 * Used to hold coordinates dimensions for each room.
	 */
	public class Room
	{
		public int x, y, width, height;

		public Room(int x, int y, int width, int height)
		{
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
		}
	}

	/*
	 * Leaf class
	 * Used to partition the large section
	 */
	public class Leaf
	{
		public int x, y, width, height;
		public Leaf leftChild, rightChild;
		public Room room;
		public List<Room> halls;

		/// <summary>
		/// The minimum size of a leaf.
		/// </summary>
		public static int  MinimumLeafSize = 6;

		public Leaf(int x, int y, int width, int height) 
		{
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
			leftChild = null;
			rightChild = null;
		}

		public bool Split()
		{
			// begin splitting the leaf into two children
			if (leftChild != null || rightChild != null)
				return false; // we're already split! Abort!
			
			// determine direction of split
			// if the width is >25% larger than height, we split vertically
			// if the height is >25% larger than the width, we split horizontally
			// otherwise we split randomly
			bool splitH = System.Convert.ToBoolean(Random.Range (0, 1));
			int max;

			if (width > height && width / height >= 1.25)
				splitH = false;
			else if (height > width && height / width >= 1.25)
				splitH = true;

			if (splitH) 
			{
				max = height - MinimumLeafSize;
			} 
			else 
			{
				max = width - MinimumLeafSize;
			}

			if (max <= MinimumLeafSize) 
			{
				//Debug.Log(max + " was less than " + MinimumLeafSize);
				return false; // the area is too small to split any more...
			} 
			else 
			{
				int split = Random.Range (MinimumLeafSize, max); // determine where we're going to split
			
				// create our left and right children based on the direction of the split
				if (splitH) 
				{
					leftChild = new Leaf (x, y, width, split);
					rightChild = new Leaf (x, y + split, width, height - split);
				} 
				else 
				{
					leftChild = new Leaf (x, y, split, height);
					rightChild = new Leaf (x + split, y, width - split, height);
				}
				return true; // split successful!
			}
		}

		public void CreateRooms()
		{
			// this function generates all the rooms and hallways for this Leaf and all of its children.
			if (leftChild != null || rightChild != null)
			{
				// this leaf has been split, so go into the children leafs
				if (leftChild != null)
				{
					leftChild.CreateRooms();
				}
				if (rightChild != null)
				{
					rightChild.CreateRooms();
				}

				// if there are both left and right children in this Leaf, create a hallway between them
				if (leftChild != null && rightChild != null)
				{
					CreateHall(leftChild.getRoom(), rightChild.getRoom());
				}
			}
			else
			{
				// place the room within the Leaf, but don't put it right 
				// against the side of the Leaf (that would merge rooms together)
				int roomWidth =  Random.Range (3, width - 2);
				int roomHeight = Random.Range (3, height - 2);

				// this Leaf is the ready to make a room
				// the room can be between 3 x 3 tiles to the size of the leaf - 2.
				int roomX =  Random.Range (1, width - roomWidth - 1);
				int roomY =  Random.Range (1, height - roomHeight - 1);

				room = new Room((x + roomX), (y + roomY), roomWidth, roomHeight);
			}
		}

		public Room getRoom()
		{
			// iterate all the way through these leafs to find a room, if one exists.
			if (room != null)
				return room;
			else
			{
				Room lRoom = null;
				Room rRoom = null;
				if (leftChild != null)
				{
					lRoom = leftChild.getRoom();
				}
				if (rightChild != null)
				{
					rRoom = rightChild.getRoom();
				}
				if (lRoom == null && rRoom == null)
					return null;
				else if (rRoom == null)
					return lRoom;
				else if (lRoom == null)
					return rRoom;
				else if (Random.Range (0,1) > .5)
					return lRoom;
				else
					return rRoom;
			}
		}

		public void CreateHall(Room left, Room right)
		{
			// now we connect these two rooms together with hallways.
			// this looks pretty complicated, but it's just trying to figure out which point is where and then either draw a straight line, or a pair of lines to make a right-angle to connect them.
			// you could do some extra logic to make your halls more bendy, or do some more advanced things if you wanted.
			
			halls = new List<Room>();

			int x1 = Random.Range ((left.x + 1), (left.x + left.width - 2));
			int y1 = Random.Range ((left.y + 1), (left.y + left.height - 2));

			int x2 = Random.Range ((right.x + 1), (right.x + right.width - 2));
			int y2 = Random.Range ((right.y + 1), (right.y + right.height - 2));

			int width = x2 - x1;
			int height = y2 - y1;

			if (width < 0)
			{
				if (height < 0)
				{
					if (Random.Range (0,1) < 0.5)
					{
						halls.Add(new Room(x2, y1, Mathf.Abs(width), 1));
						halls.Add(new Room(x2, y2, 1, Mathf.Abs(height)));
					}
					else
					{
						halls.Add(new Room(x2, y2, Mathf.Abs(width), 1));
						halls.Add(new Room(x1, y2, 1, Mathf.Abs(height)));
					}
				}
				else if (height > 0)
				{
					if (Random.Range (0,1) < 0.5)
					{
						halls.Add(new Room(x2, y1, Mathf.Abs(width), 1));
						halls.Add(new Room(x2, y1, 1, Mathf.Abs(height)));
					}
					else
					{
						halls.Add(new Room(x2, y2, Mathf.Abs(width), 1));
						halls.Add(new Room(x1, y1, 1, Mathf.Abs(height)));
					}
				}
				else // if (h == 0)
				{
					halls.Add(new Room(x2, y2, Mathf.Abs(width), 1));
				}
			}
			else if (width > 0)
			{
				if (height < 0)
				{
					if (Random.Range (0,1) < 0.5)
					{
						halls.Add(new Room(x1, y2, Mathf.Abs(width), 1));
						halls.Add(new Room(x1, y2, 1, Mathf.Abs(height)));
					}
					else
					{
						halls.Add(new Room(x1, y1, Mathf.Abs(width), 1));
						halls.Add(new Room(x2, y2, 1, Mathf.Abs(height)));
					}
				}
				else if (height > 0)
				{
					if (Random.Range (0,1) < 0.5)
					{
						halls.Add(new Room(x1, y1, Mathf.Abs(width), 1));
						halls.Add(new Room(x2, y1, 1, Mathf.Abs(height)));
					}
					else
					{
						halls.Add(new Room(x1, y2, Mathf.Abs(width), 1));
						halls.Add(new Room(x1, y1, 1, Mathf.Abs(height)));
					}
				}
				else // if (h == 0)
				{
					halls.Add(new Room(x1, y1, Mathf.Abs(width), 1));
				}
			}
			else // if (w == 0)
			{
				if (height < 0)
				{
					halls.Add(new Room(x2, y2, 1, Mathf.Abs(height)));
				}
				else if (height > 0)
				{
					halls.Add(new Room(x1, y1, 1, Mathf.Abs(height)));
				}
			}
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

	//Start of Class functions
	List<Leaf> CreateLeaves(int level)
	{
		List<Leaf> leaves = new List<Leaf>();

		Leaf root = new Leaf(0, 0, MapWidth, MapHeight);

		//Trying this to make more small rooms
		//Leaf.MinimumLeafSize = (level + 6);

		MasterRoot = root;

		leaves.Add(root);

		bool didSplit = true;

		while(didSplit)
		{
			didSplit = false;

			for(int i = 0; i < leaves.Count; i++)
			{
				if (leaves[i].leftChild == null && leaves[i].rightChild == null) // if this Leaf is not already split...
				{
					// if this Leaf is too big, or 75% chance...
					if (leaves[i].width > MaximumLeafSize || leaves[i].height > MaximumLeafSize || Random.Range (0,1) > 0.25)
					{
						if (leaves[i].Split()) // split the Leaf!
						{
							// if we did split, push the child leafs to the Vector so we can loop into them next
							leaves.Add(leaves[i].leftChild);
							leaves.Add(leaves[i].rightChild);
							didSplit = true;
						}
					}
				}
			}
		}

		return leaves;
	}

	void InitializeMap(int level)
	{
		//Clear our lists.
		gridPositions.Clear ();
		wallPositions.Clear ();

		MasterRoot = null;
		boardHolder = new GameObject ("Board").transform;

		Leafs = CreateLeaves (level);

		MasterRoot.CreateRooms ();
	}

	void BuildGrid()
	{
		for (int leafCounter = 0; leafCounter < Leafs.Count; leafCounter++) 
		{
			///Add in the Room
			if(Leafs[leafCounter].room != null)
			{
				for(int roomXcounter = 0; roomXcounter < Leafs[leafCounter].room.width; roomXcounter++)
				{
					//Debug.Log("Room: " + Leafs[leafCounter].room.x + ", " + Leafs[leafCounter].room.y + ", width: " + Leafs[leafCounter].room.width + ", height: " + Leafs[leafCounter].room.height);

					for(int roomYcounter = 0; roomYcounter < Leafs[leafCounter].room.height; roomYcounter++)
					{
						gridPositions.Add(new Vector3( (Leafs[leafCounter].room.x + roomXcounter), (Leafs[leafCounter].room.y + roomYcounter), 0f ));
					}
				}
			}

			//Add in the Halls
			if(Leafs[leafCounter].halls != null)
			{
				for(int hallCounter = 0; hallCounter < Leafs[leafCounter].halls.Count; hallCounter++)
				{
					for(int hallXcounter = 0; hallXcounter < Leafs[leafCounter].halls[hallCounter].width; hallXcounter++)
					{
						//Debug.Log("Hall: " + Leafs[leafCounter].halls[hallCounter].x + ", " + Leafs[leafCounter].halls[hallCounter].y + ", width: " + Leafs[leafCounter].halls[hallCounter].width + ", height: " + Leafs[leafCounter].halls[hallCounter].height);
						for(int hallYcounter = 0; hallYcounter < Leafs[leafCounter].halls[hallCounter].height; hallYcounter++)
						{
							gridPositions.Add(new Vector3((Leafs[leafCounter].halls[hallCounter].x + hallXcounter), (Leafs[leafCounter].halls[hallCounter].y + hallYcounter), 0f));
						}
					}
				}
			}
		}
	}

	void BuildWalls()
	{
		//Setup walls
		for (int i = 0; i < gridPositions.Count; i++) 
		{
			Vector3 northOf = gridPositions[i]; northOf.y = northOf.y + 1;
			Vector3 southOf = gridPositions[i]; southOf.y = southOf.y - 1;
			Vector3 eastOf = gridPositions[i]; eastOf.x = eastOf.x + 1;
			Vector3 westOf = gridPositions[i]; westOf.x = westOf.x - 1;
			
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
	}

	void AssembleMap()
	{
		//Floor tiles posted
		for (int i = 0; i < gridPositions.Count; i++) 
		{
			//Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
			GameObject toInstantiate = floorTiles [Random.Range (0, floorTiles.Length)];
			
			//Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
			GameObject instance =
				Instantiate (toInstantiate, new Vector3 ((gridPositions[i].x*5), 0f, (gridPositions[i].y*5)), Quaternion.identity) as GameObject;
			
			//Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
			instance.transform.SetParent (boardHolder);
		}

		//Walls posted
		for (int i = 0; i < wallPositions.Count; i++) 
		{
			//Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
			GameObject toInstantiate = wallTiles [Random.Range (0, wallTiles.Length)];
			
			//Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
			GameObject instance =
				Instantiate (toInstantiate, new Vector3 ((wallPositions[i].x*5), 0f, (wallPositions[i].y*5)), Quaternion.identity) as GameObject;
			
			//Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
			instance.transform.SetParent (boardHolder);
		}
		
		//End Tile
		if (endEnabled) 
		{
			Vector3 randomPosition = RandomPosition ();
			
			//Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
			GameObject toInstantiate = endTile;
			
			//Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
			GameObject instance =
				Instantiate (toInstantiate, new Vector3 ((randomPosition.x*5), 0f, (randomPosition.y*5)), Quaternion.identity) as GameObject;
			
			//Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
			instance.transform.SetParent (boardHolder);
		}
	}

	public void SetupScene(int level)
	{
		MapWidth =  (level + 1) * 10;
		MapHeight = (level + 1) * 10;

		//Trying this to keep rooms small
		//MaximumLeafSize = 20 + level;

		InitializeMap (level);
		BuildGrid ();
		BuildWalls ();
		AssembleMap ();

		GameObject.Find ("ThirdPersonController").transform.position = new Vector3(gridPositions[0].x*5, 0f, gridPositions[0].y*5);
		GameObject.Find ("MultipurposeCameraRig").transform.position = new Vector3(gridPositions[0].x*5, 0f, gridPositions[0].y*5);
	}
}
