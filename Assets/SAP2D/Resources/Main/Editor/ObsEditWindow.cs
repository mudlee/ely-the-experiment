using UnityEngine;
using UnityEditor;
using System.Collections;

namespace SAP2D{

	public class ObsEditWindow : EditorWindow {


		public static void Init(){
			ObsEditWindow window = (ObsEditWindow)EditorWindow.GetWindow(typeof(ObsEditWindow));
			window.titleContent = new GUIContent ("Obs Edit Tools");
			window.minSize = new Vector2 (100, 38);
			window.maxSize = window.minSize;
			window.Show ();
		}

		void OnEnable(){
			autoRepaintOnSceneChange = true;
			SAP2DManager.singleton.grid.isObsEditingEnable = true;
		}

		void OnDisable(){
			SAP2DManager.singleton.grid.isObsEditingEnable = false;
		}

		void OnGUI(){
			float brushIconID = Mathf.Clamp (SAP2DManager.singleton.grid.toolIndex, 0, 1);
			GUIContent brushTool = new GUIContent("", Resources.Load ("Main/Editor/Graphics/brush_icon"+brushIconID) as Texture2D);
			float rubberIconID = Mathf.Clamp (SAP2DManager.singleton.grid.toolIndex, 0.1f, 1);
			GUIContent rubberTool = new GUIContent("", Resources.Load ("Main/Editor/Graphics/rubber_icon"+rubberIconID) as Texture2D);

			SAP2DManager.singleton.grid.toolIndex = GUILayout.Toolbar (SAP2DManager.singleton.grid.toolIndex, new GUIContent[2]{brushTool, rubberTool});
		}
	}
}
