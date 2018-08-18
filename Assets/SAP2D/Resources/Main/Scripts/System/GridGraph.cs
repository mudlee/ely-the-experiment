using UnityEngine;
using System.Collections;
using SAP2D;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SAP2D
{
	[System.Serializable]
	public class GridGraph
	{

		public bool showGraph = true;     //show grid
		public float TileDiameter = 1;    //tile diameter (width and height)
		public int GridWidth = 10;        //grid width (number of tiles in x)
		public int GridHeight = 10;       //grid height (number tiles in y)h

		public Vector3 GridPosition;      //grid position in world space
		public Vector2 GridSize;          //grid size
		public Vector3 Center;            //point position in the center of grid
		public Vector3 LeftUpPoint;       //point position in the left up conner of grid
		public Vector3 RightUpPoint;      //point position in the right up conner of grid
		public Vector3 LeftButtomPoint;   //point position in the left buttom conner of grid
		public Vector3 RightButtomPoint;  //point position in the right buttom conner of grid

		public Color32 gridColor = new Color32 (255, 255, 255, 150);
		public Color32 obstacleColor = new Color32 (255, 0, 0, 50);
		public Color32 selectingColor = new Color32 (255, 255, 0, 100);

		public enum GridPivot
		{
			ButtomLeft,
			UpLeft,
			ButtomRight,
			UpRight,
			Center
		}
		public GridPivot gridPivot;       //grid rotation point
		public bool pivotWasChanged;      //grid rotation point change state
		private float TileRadius;   

		//manual obtacles editing values
		public bool isObsEditingEnable;
		public int toolIndex;
		public int brushSize;
		private Tile[,] matrix;

		public Tile[,] tile {
			get {
				if (matrix == null) {
					CreateGrid ();
					SAP2DManager.singleton.CalculateColliders ();
					SAP2DManager.singleton.GetTilesData ();
				} else {
					if (matrix.Length != GridWidth * GridHeight) {
						CreateGrid ();
						SAP2DManager.singleton.RemoveTileData ();
					}
				}
				return matrix;
			}
		}

		public void DrawGrid ()
		{
			#if UNITY_EDITOR

			CalculateGridRectangle ();

			if (showGraph) {
				//array of points forming a grid frameg
				Vector3[] gridFrame = new Vector3[5] {LeftButtomPoint, LeftUpPoint, RightUpPoint, RightButtomPoint, LeftButtomPoint};
				Handles.DrawAAPolyLine (3, gridFrame);
				
				if (Tools.current == Tool.Move) {
					GridPosition = Handles.PositionHandle (GridPosition, Quaternion.identity);
				}

				for(int y=0;y<GridHeight;y++){
					for(int x=0; x<GridWidth;x++){

						Vector3 WorldPos = LeftButtomPoint + Vector3.right * (tile[x,y].x * TileDiameter + TileRadius) + Vector3.up * (tile[x,y].y * TileDiameter + TileRadius);
						matrix[x,y].WorldPosition = WorldPos;

						Handles.color = gridColor;
						
						//drawing vertical lines between tiles
						if (y == GridHeight - 1) {
							Vector3 verticlal1 = tile[x,0].WorldPosition + Vector3.right * TileRadius - Vector3.up * TileRadius;
							Vector3 verticlal2 = tile[x,GridHeight-1].WorldPosition + Vector3.right * TileRadius + Vector3.up * TileRadius;
							
							Handles.DrawLine (verticlal1, verticlal2);
						}
						
						//drawing horizontal lines between tiles
						if (x == GridWidth - 1) {
							Vector3 horizontal1 = tile[0,y].WorldPosition - Vector3.right * TileRadius + Vector3.up * TileRadius;
							Vector3 horizontal2 = tile[GridWidth-1,y].WorldPosition + Vector3.right * TileRadius + Vector3.up * TileRadius;
							
							Handles.DrawLine (horizontal1, horizontal2);
						}
						
						//drawing unwakable tiles
						if(!tile[x,y].isWalkable){
							Handles.color = obstacleColor;
							
							Vector3 L_up = tile[x,y].WorldPosition - Vector3.right * TileRadius + Vector3.up * TileRadius;
							Vector3 L_down = tile[x,y].WorldPosition - Vector3.right * TileRadius - Vector3.up * TileRadius;
							Vector3 R_down = tile[x,y].WorldPosition + Vector3.right * TileRadius - Vector3.up * TileRadius;
							Vector3 R_up = tile[x,y].WorldPosition + Vector3.right * TileRadius + Vector3.up * TileRadius;
							
							Handles.DrawAAConvexPolygon(new Vector3[4] {L_down, L_up, R_up, R_down});
						}
					}
				}
			}
			#endif
		}

		public Tile GetTileFromWorldPosition (Vector2 WorldPosition)
		{
			
			WorldPosition = new Vector2 (WorldPosition.x + -LeftButtomPoint.x, WorldPosition.y + -LeftButtomPoint.y);
			
			float percentX = WorldPosition.x / GridSize.x;
			float percentY = WorldPosition.y / GridSize.y;
			percentX = Mathf.Clamp01 (percentX);
			percentY = Mathf.Clamp01 (percentY);
			
			int x = Mathf.RoundToInt ((GridWidth - 1) * percentX);
			int y = Mathf.RoundToInt ((GridHeight - 1) * percentY);
			return tile [x, y];
		}

		void CreateGrid ()
		{

			if (GridWidth < 0)
				GridWidth = 1;
			if (GridHeight < 0)
				GridHeight = 1;

			matrix = new Tile[GridWidth, GridHeight];
			CalculateGridRectangle ();

			for (int y=0; y<GridHeight; y++) {
				for (int x=0; x<GridWidth; x++) {
					matrix [x, y] = new Tile (x, y);

					Vector3 WorldPos = LeftButtomPoint + Vector3.right * (tile [x, y].x * TileDiameter + TileRadius) + Vector3.up * (tile [x, y].y * TileDiameter + TileRadius);
					matrix [x, y].WorldPosition = WorldPos;
				}
			}
		}

		//calculate positions of grid points
		void CalculateGridRectangle ()
		{
			
			CalculateGridSize ();
			PivotCalculation ();
			
			//calculate positions of grid conners relative to the center
			LeftUpPoint = Center - Vector3.right * GridSize.x / 2 + Vector3.up * GridSize.y / 2;
			LeftButtomPoint = Center - Vector3.right * GridSize.x / 2 - Vector3.up * GridSize.y / 2;
			RightUpPoint = Center + Vector3.right * GridSize.x / 2 + Vector3.up * GridSize.y / 2;
			RightButtomPoint = Center + Vector3.right * GridSize.x / 2 - Vector3.up * GridSize.y / 2;
		}
		
		void CalculateGridSize ()
		{
			GridSize.x = GridWidth * TileDiameter;
			GridSize.y = GridHeight * TileDiameter;
			
			TileRadius = TileDiameter / 2;
		}

		//caclculate position of grid center relative to the grid pivot
		void PivotCalculation ()
		{
			
			if (gridPivot == GridPivot.Center) {
				
				if (pivotWasChanged)
					GridPosition = Center;
				
				Center = GridPosition;
			}
			
			//calculate grid center position relative to the left buttom conner
			if (gridPivot == GridPivot.ButtomLeft) {
				
				if (pivotWasChanged) {
					GridPosition = LeftButtomPoint;
				}
				
				Center = GridPosition + Vector3.right * GridSize.x / 2 + Vector3.up * GridSize.y / 2;
			}
			
			//calculate grid center position relative to the left up conner
			if (gridPivot == GridPivot.UpLeft) {
				
				if (pivotWasChanged)
					GridPosition = LeftUpPoint;
				
				Center = GridPosition + Vector3.right * GridSize.x / 2 - Vector3.up * GridSize.y / 2;
			}
			
			//calculate grid center position relative to the right buttom conner
			if (gridPivot == GridPivot.ButtomRight) {
				
				if (pivotWasChanged)
					GridPosition = RightButtomPoint;
				
				Center = GridPosition - Vector3.right * GridSize.x / 2 + Vector3.up * GridSize.y / 2;
			}
			
			//calculate grid center position relative to the right up conner
			if (gridPivot == GridPivot.UpRight) {
				
				if (pivotWasChanged)
					GridPosition = RightUpPoint;
				
				Center = GridPosition - Vector3.right * GridSize.x / 2 - Vector3.up * GridSize.y / 2;
			}

			pivotWasChanged = false;
		}

		#if UNITY_EDITOR
		public void GridDrawUpdate(SceneView view){
			if (isObsEditingEnable && showGraph) {
				
				HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive)); //disable unity editor mouse selection
				
				Vector2 screenMousePosition = Event.current.mousePosition;
				screenMousePosition.y = SceneView.currentDrawingSceneView.camera.pixelHeight - screenMousePosition.y;
				
				Vector2 worldMousePosition = SceneView.currentDrawingSceneView.camera.ScreenToWorldPoint (screenMousePosition);
				
				if(worldMousePosition.x < LeftButtomPoint.x || worldMousePosition.y < LeftButtomPoint.y || 
				   worldMousePosition.x > RightUpPoint.x || worldMousePosition.y > RightUpPoint.y)
					return;
				
				Tile mouseTile = GetTileFromWorldPosition(worldMousePosition);
				
				Handles.color = selectingColor;

				int minY = mouseTile.y - (brushSize - 1);
				int maxY = mouseTile.y + (brushSize - 1);
				int minX = mouseTile.x - (brushSize - 1);
				int maxX = mouseTile.x + (brushSize - 1);
				
				if (minX < 0) 
					minX = 0;
				if (minY < 0) 
					minY = 0;
				if (maxX >= GridWidth)
					maxX = GridWidth - 1;
				if (maxY >= GridHeight)
					maxY = GridHeight - 1;
				
				
				for(int y = minY; y <= maxY; y++){
					for(int x = minX; x <= maxX; x++){
						
						Vector3 L_up = tile[x,y].WorldPosition - Vector3.right * TileRadius + Vector3.up * TileRadius;
						Vector3 L_down = tile[x,y].WorldPosition - Vector3.right * TileRadius - Vector3.up * TileRadius;
						Vector3 R_down = tile[x,y].WorldPosition + Vector3.right * TileRadius - Vector3.up * TileRadius;
						Vector3 R_up = tile[x,y].WorldPosition + Vector3.right * TileRadius + Vector3.up * TileRadius;
						
						Handles.DrawAAConvexPolygon(new Vector3[4] {L_down, L_up, R_up, R_down});
						
						if (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown) {
							if (Event.current.button == 0) {
								if(toolIndex == 0){
									tile[x,y].isWalkable = false;
									tile[x,y].Lock = true;
									SAP2DManager.singleton.WriteTileData(tile[x,y]);
								}
								if(toolIndex == 1){
									tile[x,y].isWalkable = true;
									tile[x,y].Lock = false;
									SAP2DManager.singleton.DeleteTileData(x,y);
								}
							}
						}
					}
				}
				SceneView.currentDrawingSceneView.Repaint ();
			}
		}
		#endif
	}
}

