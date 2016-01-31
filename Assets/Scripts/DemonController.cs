using UnityEngine;
using System.Collections;

public class DemonController : MonoBehaviour {
    

    [System.Serializable]
    public class MeleeSettings
    {
        public bool CanMelee = false;
        public float MeleeRange = 3.0f;
        public float MeleeHitDamage = 3.0f;
        public float HitTimer = 3.0f;
        public float MeleeAngleInDegrees = 15.0f;

        public GameObject HitEffect;
    }

    public enum State
    {
        None = 0,
        Dead = 1,
        PatrolA = 2,
        PatrolB = 3,
        Stalking = 4,
        MeleeAttack,
    }

    public Transform Player;
    public float Health = 2;

    [SerializeField]
    private MeleeSettings _meleeSettings;

    private Animator _animator;
    private State _currentState = State.PatrolA;
    private float _attackTimer = 0;

    void Awake()
    {
        if(_animator == null)
            _animator = GetComponent<Animator>();
    }

	// Use this for initialization
	void Start () {
	
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj)
        {
            Player = playerObj.transform;
        }
	}
	
	// Update is called once per frame
	void Update () {
	
        CheckAIRoutine();
       

        _attackTimer -= Time.deltaTime;
	}

    protected void CheckAIRoutine( )
    {
        
        switch (_currentState)
        {
            case State.PatrolA:
                UpdatePathfinding();
                break;

            case State.MeleeAttack:
                UpdateMeleeVsPlayer();
                break;

            default:
                break;
        }
    }

    protected void UpdatePathfinding()
    {
        if (_meleeSettings.CanMelee)
        {
            if (Vector3.Distance(Player.position, transform.position) <= _meleeSettings.MeleeRange)
            {
                _currentState = State.MeleeAttack;
                return;
            }
        }
    }
        
    protected void UpdateMeleeVsPlayer()
    {
        if (Player == null)
        {
            _currentState = State.None;
            return;
        }

        if (Vector3.Distance(Player.position, transform.position) > _meleeSettings.MeleeRange)
        {
            _currentState = State.PatrolA;
            return;
        }

        // Find direction to player and turn towards him
        var vToPlayer = Player.transform.position - transform.position;
        var dirToPLayer = vToPlayer.normalized;


        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dirToPLayer), 5.0f);

        if (Vector3.Dot(transform.forward, dirToPLayer) > Mathf.Cos(Mathf.Deg2Rad * 0.5f * _meleeSettings.MeleeAngleInDegrees))
        {
            // Update attack timer loop.
            if (_attackTimer <= 0)
            {
                _attackTimer = _meleeSettings.HitTimer;
                PerformMeleeAttack();
            }
        }
    }

    protected void PerformMeleeAttack( )
    {
        Debug.Log(name + " Melee attack!");

        if (_animator)
        {
            _animator.SetTrigger("MeleeAttack");
        }

        if (_meleeSettings.HitEffect)
        {
            // Find direction to player
            var dirToPlayer = Player.transform.position - transform.position;
            dirToPlayer.Normalize();

            // Spawn a hit effect
            var instance = (GameObject)GameObject.Instantiate(_meleeSettings.HitEffect, Player.transform.position + Vector3.up, Quaternion.LookRotation(-dirToPlayer));
            GameObject.Destroy(instance.gameObject, 2.0f);
        }

        GameEvents.TriggerPlayerDamaged(_meleeSettings.MeleeHitDamage);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (_meleeSettings.CanMelee)
        {
            Gizmos.DrawWireSphere(transform.position, _meleeSettings.MeleeRange);
        }
    }
}
