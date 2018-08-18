using UnityEngine;
using UnityEditor;
using System.Collections;
using SAP2D;

namespace SAP2D{

	[CustomEditor(typeof(PathfindingConfig2D))]
	public class PathfindingConfigEditor : Editor {

		private PathfindingConfig2D config;

		void OnEnable(){
			config = (PathfindingConfig2D)target;
		}

		public override void OnInspectorGUI(){
			config.DiagonalMovement = EditorGUILayout.Toggle (new GUIContent ("Diagonal Movement"), config.DiagonalMovement);
			EditorGUI.BeginDisabledGroup (!config.DiagonalMovement);
			config.IgnoreCorners = EditorGUILayout.Toggle (new GUIContent ("Ignore Corners"), config.IgnoreCorners);
			EditorGUI.EndDisabledGroup();
		}
	}
}