using UnityEngine;
using System.Collections;

namespace SAP2D{

	[CreateAssetMenu(fileName = "New Config", menuName = "SAP2D/Pathfinding Config 2D", order = 1)]
	public class PathfindingConfig2D : ScriptableObject {
		public bool IgnoreCorners;
		public bool DiagonalMovement = true;
	}
}
