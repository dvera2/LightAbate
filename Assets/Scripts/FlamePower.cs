using UnityEngine;
using System.Collections;

public class FlamePower : MonoBehaviour {

    [SerializeField]
    private Transform _muzzle;

    [SerializeField]
    private ParticleSystem _particleSystem;

    [SerializeField]
    private float _flameConeInDegrees = 45.0f;

    [SerializeField]
    private float _flameDistance = 3.0f;

    [SerializeField]
    private float _damagePerSecond = 1.0f;

    private bool _isOn = false;
    private EnemyManager _enemyManager;

    void Start()
    {
        _enemyManager = GameObject.FindObjectOfType<EnemyManager>();

        if (_particleSystem)
        {
            var emission = _particleSystem.emission;
            emission.enabled = false;
        }
    }

    private void Update()
    {
        bool wasOn = _isOn;

        _isOn = Input.GetKey(KeyCode.G);
     
        if (_isOn != wasOn)
        {
            ToggleFire(_isOn);
        } 
    }

    void FixedUpdate()
    {
        if (_isOn)
        {
            // Do a cone check
            if (_enemyManager)
            {
                var demonsToDamage = _enemyManager.FindDemonsInCone(transform.position, transform.forward, _flameConeInDegrees, _flameDistance);
                if (demonsToDamage != null)
                {
                    for (int i = 0; i < demonsToDamage.Count; i++)
                        demonsToDamage [i].DoDamage(_damagePerSecond * Time.fixedDeltaTime);
                }
            }
        }
    }

    private void ToggleFire(bool isOn)
    {
        Debug.Log("Toggling fire: " + isOn);
        if (_particleSystem)
        {
            var emission = _particleSystem.emission;

            if (!isOn)
                emission.enabled = false;
            else
                emission.enabled = true;
        }
    }
}
