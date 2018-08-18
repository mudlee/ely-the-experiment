using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour 
{
    [SerializeField] Light _flashLight;

    const float FLASHLIGHT_DISTANCE = 7f;
    const float FLASHLIGHT_MAX_INTENSITY = 3f;
    const float FLASHLIGHT_LIFETIME = 10f;
    const float FLASHLIGHT_ROTATION_SMOOTHNESS = 10f;
    const float SPEED = 2f;
    const float ENEMY_AWARENESS_RADIUS = 2f;

    Vector2 TEMP_velocity = new Vector2();
    Quaternion TEMP_flashLightQuaternion;

    Rigidbody2D _rigidbody;
    CircleCollider2D _flashLightCollider;
    bool _flashLightOn;
    int _flashLightFadeOffUpdates;

    void Start()
    {
        TEMP_flashLightQuaternion = Quaternion.LookRotation(new Vector2(Random.Range(0.5f, 1), Random.Range(0.5f, 1)));

        _rigidbody = GetComponent<Rigidbody2D>();
        _flashLightCollider = GetComponent<CircleCollider2D>();
        _flashLight.intensity = FLASHLIGHT_MAX_INTENSITY;
    }

	void Update () 
    {
        HandleMove();
        HandleFlashLightSwitch();
        HandleFlashLightLit();
	}

    void HandleFlashLightLit()
    {
        if(!_flashLightOn)
        {
            return;
        }

        float intensityDecrease = _flashLight.intensity / (float)_flashLightFadeOffUpdates;
        _flashLight.intensity -= intensityDecrease;

        // this controls the radius when we decrease the light's intensity
        float intensityFactor = Mathf.Clamp((_flashLight.intensity / FLASHLIGHT_MAX_INTENSITY) * 1.5f, 0, 1);
        _flashLightCollider.radius = FLASHLIGHT_DISTANCE * intensityFactor;
        _flashLightFadeOffUpdates--;

        if(_flashLightFadeOffUpdates <= 0)
        {
            SwitchFlashLight(false);
            return;
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        var zombie = collision.gameObject.GetComponent<ZombieController>();
        if(zombie == null)
        {
            return;
        }

        // if the flashlight is on, checks whether the Zombie is inside the cone
        if (_flashLightOn)
        {
            Vector3 direction = collision.gameObject.transform.position - transform.position;
            float angle = Vector3.Angle(direction, _flashLight.transform.forward);

            if (angle < _flashLight.spotAngle / 2)
            {
                zombie.Lit(transform.position);
            }
        }
        else // checks if the zombie is close enough when the flashlight is off
        {
            float distance = Vector3.Distance(collision.gameObject.transform.position, transform.position);
            if (distance <= ENEMY_AWARENESS_RADIUS)
            {
                zombie.Lit(transform.position);
            }
        }
    }

    void HandleMove()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        TEMP_velocity.Set(horizontal * SPEED, vertical * SPEED);
        _rigidbody.velocity = TEMP_velocity;

        if(TEMP_velocity != Vector2.zero)
        {
            TEMP_flashLightQuaternion.SetLookRotation(TEMP_velocity.normalized);
        }

        _flashLight.transform.rotation = Quaternion.Slerp(_flashLight.transform.rotation, TEMP_flashLightQuaternion, Time.deltaTime*FLASHLIGHT_ROTATION_SMOOTHNESS);
    }

    void HandleFlashLightSwitch()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            SwitchFlashLight(!_flashLightOn);
        }
    }

    void SwitchFlashLight(bool on)
    {
        _flashLightOn = on;
        _flashLight.enabled = on;

        if(on)
        {
            _flashLightFadeOffUpdates = Mathf.RoundToInt((float)FLASHLIGHT_LIFETIME / Time.deltaTime);
            _flashLightCollider.radius = FLASHLIGHT_DISTANCE;
        }
        else
        {
            _flashLight.intensity = FLASHLIGHT_MAX_INTENSITY;
        }
    }

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.back, FLASHLIGHT_DISTANCE);
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.back, ENEMY_AWARENESS_RADIUS);
#endif
    }
}
