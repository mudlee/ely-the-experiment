using UnityEngine;
using SAP2D;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SAP2DAgent))]
public class ZombieController : MonoBehaviour
{
    SAP2DAgent _sAP2DAgent;

    public void Lit(Vector3 LitFrom)
    {
        // setting the last lit position for the A* pathfinder
        _sAP2DAgent.Target = LitFrom;
    }

    void Start()
    {
        _sAP2DAgent = GetComponent<SAP2DAgent>();
    }
}
