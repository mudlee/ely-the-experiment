using UnityEngine;
using UnityEditor;
using System.Collections;
using SAP2D;

namespace SAP2D{

	[CustomEditor(typeof(SAP2DManager))]
	public class SAP2DManagerEditor : Editor {

		private SAP2DManager manager;

		private int TabIndex; 
		private bool showGridParameters;
		private bool showColorSettings;
		private bool showObstacleEditingSettigs;

		void OnEnable(){
			manager = SAP2DManager.singleton;
			LoadPrefs ();
			SceneView.onSceneGUIDelegate += manager.grid.GridDrawUpdate;
		}

		void OnDisable(){
			SceneView.onSceneGUIDelegate -= manager.grid.GridDrawUpdate;
		}

		public override void OnInspectorGUI(){

			DrawTabsPanel ();

			serializedObject.Update ();
			if (TabIndex == 0) {
				DrawGridArea ();
			}
			if (TabIndex == 1) {
				DrawGeneralArea();
			}
			serializedObject.ApplyModifiedProperties ();

			EditorGUILayout.Space ();
			if (GUILayout.Button ("Calculate Colliders", GUILayout.Height(25))) {
				manager.CalculateColliders();
				SceneView.RepaintAll();
			}
		}

		void OnSceneGUI(){
			manager.grid.DrawGrid ();
		}

		void DrawTabsPanel(){
			string[] tabsName = new string[2] {"Grapics", "General"};
			TabIndex = GUILayout.Toolbar (TabIndex, tabsName);
			EditorPrefs.SetInt ("TabIndex", TabIndex);
		}

		void DrawGridArea(){
			bool showGraph = EditorGUILayout.ToggleLeft (new GUIContent ("Show Grid"), manager.grid.showGraph);
			
			if (showGraph != manager.grid.showGraph) {
				manager.grid.showGraph = showGraph;
				SceneView.RepaintAll();
			}

			EditorGUI.indentLevel = 1;
			showGridParameters = EditorGUILayout.Foldout (showGridParameters, "Grid Parameters");
			EditorPrefs.SetBool ("showGridParameters", showGridParameters);

			if (showGridParameters) {
				
				float newDiameter = EditorGUILayout.FloatField (new GUIContent("Tile Diameter", "Tile diameter (width and height value)."), manager.grid.TileDiameter);
				
				if (newDiameter != manager.grid.TileDiameter) {
					manager.grid.TileDiameter = newDiameter;
					SceneView.RepaintAll();
				}
				
				int newWidth = EditorGUILayout.IntField (new GUIContent ("Grid Width", "Grid width (number of tiles in x)."), manager.grid.GridWidth);
				
				if (newWidth != manager.grid.GridWidth) {
					if(newWidth < 0) newWidth =1;
					manager.grid.GridWidth = newWidth;
					SceneView.RepaintAll();
				}
				
				int newHeight = EditorGUILayout.IntField (new GUIContent("Grid Height", "Grid height (number tiles in y)."), manager.grid.GridHeight);
				
				if (newHeight != manager.grid.GridHeight) {
					if(newHeight < 0) newHeight =1;
					manager.grid.GridHeight = newHeight;
					SceneView.RepaintAll();
				}
				
				GridGraph.GridPivot newValue = (GridGraph.GridPivot)EditorGUILayout.EnumPopup ("Pivot", manager.grid.gridPivot);
				Vector3 NEWgridPos = EditorGUILayout.Vector3Field(new GUIContent("Grid Position"), manager.grid.GridPosition);
				
				if (manager.grid.GridPosition != NEWgridPos) {
					manager.grid.GridPosition = NEWgridPos;
					SceneView.RepaintAll();
				}
				
				if (newValue != manager.grid.gridPivot) {
					
					manager.grid.pivotWasChanged = true;
					manager.grid.gridPivot = newValue;
					SceneView.RepaintAll();
				}
			}

			showColorSettings = EditorGUILayout.Foldout (showColorSettings, "Colors");
			EditorPrefs.SetBool ("showColorSettings", showColorSettings);

			if (showColorSettings) {
				Color gridColor = EditorGUILayout.ColorField("Grid", manager.grid.gridColor);
				if(gridColor != manager.grid.gridColor){
					manager.grid.gridColor = gridColor;
					SceneView.RepaintAll();
				}

				Color obstacleColor = EditorGUILayout.ColorField("Obstacles", manager.grid.obstacleColor);
				if(obstacleColor != manager.grid.obstacleColor){
					manager.grid.obstacleColor = obstacleColor;
					SceneView.RepaintAll();
				}

				Color selectingColor = EditorGUILayout.ColorField("Selecting", manager.grid.selectingColor);
				if(selectingColor != manager.grid.selectingColor){
					manager.grid.selectingColor = selectingColor;
					SceneView.RepaintAll();
				}
			}
			EditorGUI.indentLevel = 0;
		}

		void DrawGeneralArea(){
			bool usePhysics = EditorGUILayout.ToggleLeft (new GUIContent("Use Physics 2D", "Calculate 2D colliders."), manager.UsePhysics2D);
			EditorPrefs.SetBool ("showObstacleEditingSettigs", showObstacleEditingSettigs);
			
			if (usePhysics != manager.UsePhysics2D) {
				manager.UsePhysics2D = usePhysics;
				manager.CalculateColliders();
				SceneView.RepaintAll();
			}

			EditorGUI.indentLevel = 1;
			if (usePhysics) {
				SerializedProperty collisionTags = serializedObject.FindProperty ("IgnoreCollisionTags");
				EditorGUILayout.PropertyField (collisionTags ,true);
			}

			showObstacleEditingSettigs = EditorGUILayout.Foldout (showObstacleEditingSettigs, "Manual Obstacle Editing");
			EditorPrefs.SetBool ("showObstacleCalculation", showObstacleEditingSettigs);

			if (showObstacleEditingSettigs) {
				float y = EditorGUILayout.BeginVertical().y;
				SAP2DManager.singleton.grid.brushSize = EditorGUILayout.IntSlider("Brush Size",SAP2DManager.singleton.grid.brushSize, 1, 10);
				if(GUI.Button(new Rect(30,y+16,110,15),"Open Tools Panel", EditorStyles.miniButton)){
					ObsEditWindow.Init();
				}
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				EditorGUILayout.EndVertical();
			}
			EditorGUI.indentLevel = 0;
		}

		void LoadPrefs(){
			TabIndex = EditorPrefs.GetInt ("TabIndex");
			showGridParameters = EditorPrefs.GetBool ("showGridParameters");
			showColorSettings = EditorPrefs.GetBool ("showColorSettings");
			showObstacleEditingSettigs = EditorPrefs.GetBool ("showObstacleEditingSettigs");
		}
	}
}
