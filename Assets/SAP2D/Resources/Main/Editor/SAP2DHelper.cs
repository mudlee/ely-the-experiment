using UnityEngine;
using UnityEditor;

namespace SAP2D{
	public class SAP2DHelper : MonoBehaviour {

//		[MenuItem("Tools/SAP2D/About")]
//		static void OpenAboutWindow(){
//
//		}
			
		[MenuItem("Tools/SAP2D/Documentation")]
		static void OpenDoumentation(){
			Application.OpenURL ("https://docs.google.com/document/d/1XF7-nzzJY5H-bvi3iirro9fWSLZsS-2cFpQD2clNk6c");
		}

		[MenuItem("Tools/SAP2D/Forum")]
		static void OpenForumPage(){
			Application.OpenURL ("https://forum.unity.com/threads/released-sap2d-system-03-2b-a-pathfinding-for-2d-games.525707");
		}
	}
}
