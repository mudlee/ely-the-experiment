using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SAP2D;

namespace SAP2D {
	
	[AddComponentMenu("Pathfinding 2D/SAP2D Manager")]
	public class SAP2DManager : MonoBehaviour {

		private static SAP2DManager Singleton;
		
		public static SAP2DManager singleton{
			get{
				if(Singleton == null){
					Singleton = FindObjectOfType<SAP2DManager>();
					
					if(Singleton == null){
						Singleton = new GameObject("SAP2D").AddComponent<SAP2DManager>();
					}
				}
				return Singleton;
			}
		}

		public GridGraph grid = new GridGraph ();

		public bool UsePhysics2D = true;                              //use 2D physics (calculate 2D colliders)
		public List<string> IgnoreCollisionTags = new List<string>(); //array of collider tags that will be ignored in the system colculations

		public UserObstaclesData ObsUserData = new UserObstaclesData();

		//find path from point A to point B
		public Vector2[] FindPath(Vector2 from, Vector2 to, PathfindingConfig2D config){
			
			Clear ();
		
			List<Tile> OpenList = new List<Tile> ();   //list of tiles to check
			List<Tile> ClosedList = new List<Tile> (); //list of tiles to ignore
			
			Tile CurrentTile = grid.GetTileFromWorldPosition (from); //current check tile, that to beginning of path searching equal start tile
			Tile toTile = grid.GetTileFromWorldPosition (to);        //end tile
			
			if (!toTile.isWalkable)
				return null;
			
			OpenList.Add(CurrentTile);
			CurrentTile.State = Tile.listState.Open;
			
			while (toTile.State != Tile.listState.Close) {
				
				if(OpenList.Count == 0){
					Debug.Log("Path not found!");
					return null;
				}
				
				OpenList.Remove (CurrentTile); //delete current from the open list
				
				ClosedList.Add (CurrentTile);  //add current tile to closed list
				CurrentTile.State = Tile.listState.Close;
				
				CalculateTilesAround (CurrentTile, toTile, OpenList, config); //calculate tile parameters around current tile
				
				int minF = int.MaxValue; //minimum F value
				
				//searching tile with minimum F value in open list 
				foreach (Tile t in OpenList) {
					
					if(t.F < minF){
						minF = t.F;
						//current tile equal tile with minimum F value
						CurrentTile = t;
					}
				}
			}
			return PathRecovery (from, to);
		}

		//calculate tile parameters around ceterTile
		void CalculateTilesAround(Tile centerTile, Tile toTile, List<Tile> OpenList, PathfindingConfig2D config){
			
			//loop that ckecks tiles around the central tile
			for (int y = centerTile.y - 1; y <= centerTile.y + 1; y++) {
				for(int x = centerTile.x - 1; x <= centerTile.x + 1; x++){
					
					//loop values should not be greater than grid size
					if(x >= 0 && x < grid.GridWidth && y >= 0 && y < grid.GridHeight){
						
						Tile current = grid.tile[x,y];

						if(!config.DiagonalMovement)
						if(x != centerTile.x && y != centerTile.y)
							continue;
						
						if(!current.isWalkable || !ConnerManager(centerTile, current, config))
							continue;
						
						//current checked tile should not to be in the closed list
						if(current.State != Tile.listState.Close){ 
							
							//if current tile is not in any list
							if(current.State == Tile.listState.Empty){
								
								current.ParentTile = centerTile; //set central tile as parent tile for current tile
								
								CalculateTileValues(current, centerTile, toTile);
								
								//add current tile to open list
								OpenList.Add(current);
								current.State = Tile.listState.Open;
							}
							
							//if tile is already in open list, we should check the shortest path across this tile
							//compare the already calculated G value and new G value
							if(current.State == Tile.listState.Open){
								
								//save the already calculated values ​​of G, H and ParentTile
								int oldG = current.G;
								int oldH = current.H;
								Tile oldParentTile = current.ParentTile;
								
								//calculate new values of G and H
								current.ParentTile = centerTile;
								CalculateTileValues(current, centerTile, toTile);
								
								//compare old values and new values
								//if the new value of G is greater than the old G value, then the path is not shorter
								if(current.G >= oldG){
									
									//return old values
									current.ParentTile = oldParentTile;
									current.G = oldG;
									current.H = oldH;
									current.F = oldH + oldG;
								}
							}
						}
					}
				}
			}
		}

		//calculate tile parameters
		void CalculateTileValues(Tile tile, Tile centerTile, Tile toTile){  //14 10 14  if current tile coordinate equal the central tile coordinates
			int G = 0;                                                      //10 ** 10  equal the central tile coordinates (**), 
			if (tile.x == centerTile.x || tile.y == centerTile.y)           //14 10 14  then G of current tile = 10
				G = 10;                                                                   
			else                                                          //if both coordinates of the current tile are not equal to central tile cordinates,
				G = 14;                                                   //then G of current tile = 14
			G += tile.ParentTile.G; //summarize parent tile G value and setted G 
			//calculate distance between current and end tiles, ignoring vertical movement (calculations)
			int H = (Mathf.Abs(tile.x - toTile.x) + Mathf.Abs(tile.y - toTile.y))*10;  // (|x1 - x2| + |y1 - y2|)*10  (x1, y1) - current tile cordinates
			int F = G + H;                                                             //                             (x2, y2) - end tile cordinates
			
			tile.G = G;
			tile.H = H;
			tile.F = F;
		}

		//path recovering
		//the path is recovered beginning end tile, move from the parent tile to parent tile, to the starting tile
		Vector2[] PathRecovery(Vector2 from, Vector2 to){
			
			Tile fromTile = grid.GetTileFromWorldPosition (from);
			Tile current = grid.GetTileFromWorldPosition (to);
			
			List<Vector2> path = new List<Vector2> ();
			
			while (current != fromTile) {
				
				path.Add(current.WorldPosition);
				
				current = current.ParentTile;
			}
			
			path.Reverse ();
			
			return path.ToArray ();
		}

		//returns false if angular movement or angle cut is impossible
		bool ConnerManager(Tile centerTile, Tile current, PathfindingConfig2D config){
			
			bool canWalk = true;
			
			//if rule the catting conners is off, then forbid cutting corner                    C - centerTile * - current X - unwalkable tile
			if (!config.IgnoreCorners) {                                                     
				
				if (centerTile.x + 1 == current.x && centerTile.y + 1 == current.y) { //0 X *
																				      //0 C 0
					if (!grid.tile [current.x-1,current.y].isWalkable) {              //O O O
						canWalk = false;
					}
				}
				
				if (centerTile.x - 1 == current.x && centerTile.y - 1 == current.y) { //0 X C
					                                                                  //0 * 0
					if (!grid.tile [current.x,current.y+1].isWalkable) {              //O O O
						canWalk = false;
					}
				}
				
				if (centerTile.x - 1 == current.x && centerTile.y + 1 == current.y) { //* X 0
																					  //0 C 0
					if (!grid.tile [current.x+1,current.y].isWalkable) {              //O O O
						canWalk = false;
					}
				}
				
				if (centerTile.x + 1 == current.x && centerTile.y - 1 == current.y) { //C X 0
					                                                                  //0 * 0
					if (!grid.tile [current.x,current.y+1].isWalkable) {              //O O O
						canWalk = false;
					}
				}
				
				if (centerTile.x + 1 == current.x && centerTile.y - 1 == current.y) { //0 0 0
					                                                                  //0 C 0
					if (!grid.tile [current.x-1,current.y].isWalkable) {              //O X *
						canWalk = false;
					}
				}
				
				if (centerTile.x - 1 == current.x && centerTile.y + 1 == current.y) { //0 0 0
					                                                                  //0 * 0
					if (!grid.tile [current.x,current.y-1].isWalkable) {              //0 X C
						canWalk = false;
					}
				}
				
				if (centerTile.x - 1 == current.x && centerTile.y - 1 == current.y) { //0 0 0
					                                                                  //0 C 0
					if (!grid.tile [current.x+1,current.y].isWalkable) {              //* X 0
						canWalk = false;
					}
				}
				
				if (centerTile.x + 1 == current.x && centerTile.y + 1 == current.y) { //0 0 0
					                                                                  //0 * 0
					if (!grid.tile [current.x,current.y-1].isWalkable) {              //C X 0
						canWalk = false;
					}
				}
				
			} 
			//if rule the catting conners is on, then check possibility of cutting conner
			else {
				
				if (centerTile.x + 1 == current.x && centerTile.y + 1 == current.y) { //0 X *  - in this case, the angle cut is impossible
					                                                                  //0 C X
					if (!grid.tile [current.x-1,current.y].isWalkable) {              //0 0 0 
						if (!grid.tile [current.x,current.y-1].isWalkable)
							canWalk = false;
					}
				}
				
				if (centerTile.x - 1 == current.x && centerTile.y + 1 == current.y) { //* X 0  - in this case, the angle cut is impossible
					                                                                  //X C 0
					if (!grid.tile [current.x+1,current.y].isWalkable) {              //0 0 0 
						if (!grid.tile [current.x,current.y-1].isWalkable)
							canWalk = false;
					}
				}
				
				if (centerTile.x - 1 == current.x && centerTile.y - 1 == current.y) { //0 0 0  - in this case, the angle cut is impossible
					                                                                  //X C 0
					if (!grid.tile [current.x+1,current.y].isWalkable) {              //* X 0 
						if (!grid.tile [current.x,current.y+1].isWalkable)
							canWalk = false;
					}
				}
				
				if (centerTile.x + 1 == current.x && centerTile.y - 1 == current.y) { //0 0 0  - in this case, the angle cut is impossible
					                                                                  //0 C X
					if (!grid.tile [current.x-1,current.y].isWalkable) {              //0 X *
						if (!grid.tile [current.x,current.y+1].isWalkable)
							canWalk = false;
					}
				}
			}
			
			return canWalk;
		}

		//calculate 2D colliders
		public void CalculateColliders(){
			
			for (int y = 0; y < grid.GridHeight; y++) {
				for(int x = 0; x < grid.GridWidth; x++){
					
					Collider2D[] colls = Physics2D.OverlapCircleAll (grid.tile[x,y].WorldPosition, grid.TileDiameter / 4);
					
					if(grid.tile[x,y].Lock)
						continue;
					
					if(UsePhysics2D){
						
						if (colls.Length > 0) {
							
							grid.tile[x,y].isWalkable = CanWalkable(colls);
							
						} else {
							
							grid.tile[x,y].isWalkable = true;
						}
					}else{
						
						grid.tile[x,y].isWalkable = true;
					}
				}
			}
		}

		//checking current tile for its walk parameter
		bool CanWalkable(Collider2D[] colls){	
			//check all found colliders that collide the current tile
			//if one of the found colliders has the parameter IsTrigger = false, then this tile is unwalkable
			bool walkable = true;
			
			foreach (Collider2D coll in colls) {
				//if the current collider has a tag that is in list of ignored tags, then
				//checking current collider is not necessary
				if(IgnoreCollisionTags.Contains(coll.tag))
					continue;
				
				if(coll.isTrigger == false){
					walkable = false;
				}
			}
			return walkable;
		}

		void Clear(){
			for (int y = 0; y < grid.GridHeight; y++) {
				for (int x = 0; x < grid.GridWidth; x++) {
					
					grid.tile[x,y].State = Tile.listState.Empty;
					grid.tile[x,y].F = 0;
					grid.tile[x,y].G = 0;
					grid.tile[x,y].H = 0;
					grid.tile[x,y].ParentTile = null;
				}
			}
		}

		public void GetTilesData(){
			foreach (Tile t in ObsUserData.t) {
				grid.tile[t.x,t.y].isWalkable = false;
				grid.tile[t.x,t.y].Lock = true;
			}
		}

		public void WriteTileData(Tile tile){
			if (ObsUserData.t.Contains (tile)) 
				return;
			ObsUserData.t.Add (tile);
		}

		public void RemoveTileData(){
			ObsUserData.t.Clear ();
		}

		public void DeleteTileData(int x, int y){
			for (int i=0; i<ObsUserData.t.Count; i++) {
				if(ObsUserData.t[i].x == x && ObsUserData.t[i].y == y){
					ObsUserData.t.Remove(ObsUserData.t[i]);
					break;
				}
			}
		}
	}

	[System.Serializable]
	public class UserObstaclesData{
		public List<Tile> t = new List<Tile>();
	}
}
