using UnityEngine;
using System.Collections;

namespace SAP2D{

	[System.Serializable]
	public class Tile{
		public int x;                  //tile x cordinate in grid
		public int y;                  //tile y cordinate in grid
		public Vector3 WorldPosition;  //tile world position
		public bool isWalkable = true; //tile walk state
		public bool Lock;              //true - isWalkable value can't change
		
		public int F;           //summarized value of G and H
		public int G;           //cost of moving horizontally (10) and vertically (14)
		public int H;           //number of tiles between this tile and end tile
		public Tile ParentTile; //parent tile (used for G value calculation)
		
		//tile list state
		public listState State;
		
		public enum listState {Empty ,Open, Close}

		public Tile(int x, int y){
			this.x = x;
			this.y = y;
		}
		
		/*
	 * Empty - this tile is not on any lists
	 * Open -  this tile is on open list
	 * Close - this tile is on closed list
	 * 
	 **/
	}
}
