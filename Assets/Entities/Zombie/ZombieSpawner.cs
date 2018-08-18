using System.Collections;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] GameObject _zombiePrefab;

	void Start () {
        StartCoroutine(Spawn());
	}

    IEnumerator Spawn()
    {
        GameObject zombie = Instantiate(_zombiePrefab) as GameObject;
        zombie.transform.position = transform.position;
        yield return null;
    }
	
    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.back, 1f);
#endif      
    }
}
