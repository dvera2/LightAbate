using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class EnemyProjectile : MonoBehaviour {

    [SerializeField]
    private GameObject _explosionPrefab;

    [SerializeField]
    private float _speed = 5.0f;

    private Rigidbody _rigidbody;

    public float LightDamage = 3.0f;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void SetDirection( Vector3 direction )
    {
        _rigidbody.velocity = _speed * direction;
    }

    protected void OnTriggerEnter( Collider collider )
    {
        SpawnExplosion();

        if(collider.CompareTag("Player"))
        {
            GameEvents.TriggerPlayerDamaged( LightDamage );
        }

        Destroy(gameObject);
    }

    protected void SpawnExplosion()
    {
        if (_explosionPrefab == null)
            return;

        GameObject.Instantiate(_explosionPrefab, _rigidbody.position, Quaternion.identity);
    }
}
