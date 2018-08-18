using UnityEngine;
using System.Collections;
using SAP2D;

namespace SAP2D{

	[ExecuteInEditMode]
	[RequireComponent(typeof(Collider2D))]
	[AddComponentMenu("Pathfinding 2D/Dynamic Obstacle")]
	public class DynamicObstacle : MonoBehaviour {

		public Collider2D Coll2D;

		private SAP2DManager manager;
		private Bounds CurrentBounds;
		private Bounds LastBounds;
		private bool isMoving;
		private bool isTrigger;

		void OnEnable(){
			manager = SAP2DManager.singleton;
			Coll2D = GetComponent<Collider2D> ();
			CheckObstacleTiles(CurrentBounds);
		}

		void OnDisable(){
			CheckObstacleTiles(CurrentBounds);
		}
		
		void Update(){
			if (Coll2D != null) {
				
				if (transform.hasChanged) {       
					isMoving = true;              
					transform.hasChanged = false;
				} else {
					isMoving = false;
				}
				
				if(isMoving){
					CurrentBounds = Coll2D.bounds;
					
					CheckObstacleTiles(CurrentBounds);
					CheckObstacleTiles(LastBounds);
					
					LastBounds = CurrentBounds;
				}
				
				
				if(isTrigger != Coll2D.isTrigger){
					CheckObstacleTiles(CurrentBounds);
				}
				
				isTrigger = Coll2D.isTrigger;
			}
		}
		
		//check for intersections with the object collider of all tiles located inside the object
		public void CheckObstacleTiles(Bounds bounds){
			if (Coll2D != null) {
				
				Tile minTile = manager.grid.GetTileFromWorldPosition (bounds.min);
				Tile maxTile = manager.grid.GetTileFromWorldPosition (bounds.max);
				
				int xMin = minTile.x - 1;
				int xMax = maxTile.x + 1;
				int yMin = minTile.y - 1;
				int yMax = maxTile.y + 1;
				
				if (xMin < 0) 
					xMin = 0;
				if (yMin < 0) 
					yMin = 0;
				if (xMax >= manager.grid.GridWidth)
					xMax = manager.grid.GridWidth - 1;
				if (yMax >= manager.grid.GridHeight)
					yMax = manager.grid.GridHeight - 1;
				
				for (int y = yMin; y <= yMax; y++) {
					for (int x = xMin; x <= xMax; x++) {
						
						Collider2D[] colls = Physics2D.OverlapCircleAll (manager.grid.tile[x,y].WorldPosition, manager.grid.TileDiameter / 4);
						
						if(manager.grid.tile[x,y].Lock)
							continue;
						
						if(manager.UsePhysics2D){
							
							if (colls.Length > 0) {
								
								manager.grid.tile[x,y].isWalkable = CanWalkable(colls);
								
							} else {
								
								manager.grid.tile[x,y].isWalkable = true;
							}
						}else{
							
							manager.grid.tile[x,y].isWalkable = true;
						}
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
				if(manager.IgnoreCollisionTags.Contains(coll.tag))
					continue;
				
				if(coll.isTrigger == false){
					walkable = false;
				}
			}
			return walkable;
		}
		
		void OnDestroy(){
			CheckObstacleTiles(LastBounds);
		}
	}
}
