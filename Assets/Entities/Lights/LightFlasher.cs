using System.Collections;
using UnityEngine;

public class LightFlasher : MonoBehaviour {
    const float FLASH_LENGTH = 3.5f;
    Light _light;
    float _intensityMax;
    Coroutine _coroutine;

	void Start ()
    {
        _light = GetComponent<Light>();
        _intensityMax = _light.intensity;

        InvokeRepeating("StartFlashing", 0, FLASH_LENGTH);
    }
	
    void StartFlashing()
    {
        if(_coroutine!=null)
        {
            StopCoroutine(_coroutine);    
        }

        _coroutine = StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        float waitTime = FLASH_LENGTH / 2;
        while (_light.intensity < _intensityMax)
        {
            _light.intensity += Time.deltaTime / waitTime;
            yield return null;
        }
        while (_light.intensity > 0)
        {
            _light.intensity -= Time.deltaTime / waitTime;
            yield return null;
        }
        yield return null;
    }
}
