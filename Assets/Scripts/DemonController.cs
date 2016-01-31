using UnityEngine;
using System.Collections;

using UnityStandardAssets.Characters.ThirdPerson;

public class DemonController : MonoBehaviour {

    [System.Serializable]
    public class GeneralSettings
    {
        public float Health = 1.0f;
        public float PursuitDistance = 6.0f;
    }

    [System.Serializable]
    public class RangedAttackSettings
    {
        public bool CanRangeAttack = false;
        public float Range = 10.0f;
        public float HitTimer = 3.0f;
        public float RangeAngleInDegrees = 15.0f;
        public GameObject Projectile;
    }

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
        Pursuit = 4,
        MeleeAttack = 5,
        RangedAttack = 6,
    }

    public Transform Player;
    public float Health = 2;

    [SerializeField]
    private GeneralSettings _generalSettings;

    [SerializeField]
    private MeleeSettings _meleeSettings;

    [SerializeField]
    private RangedAttackSettings _rangedSettings;


    private Animator _animator;
    private State _currentState = State.PatrolA;
    private float _attackTimer = 0;
    private float _defaultMoveSpeed = 1.0f;

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

            case State.RangedAttack:
                UpdateRangedVsPlayer();
                break;

            case State.Pursuit:
                UpdatePursuit();
                break;

            default:
                break;
        }
    }

    // ------------------------------------------------------------------------------------
    protected void UpdatePursuit()
    {
        if (DidSwitchState())
            return;


        // Find direction to player and turn towards him
        var vToPlayer = Player.transform.position - transform.position;
        var dirToPLayer = vToPlayer.normalized;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dirToPLayer), 8.0f);


        var characterController = GetComponent<ThirdPersonCharacter>();
        if (characterController)
        {
            characterController.Move(dirToPLayer, false, false);
        }
        else
        {
            var controller = GetComponent<CharacterController>();
            if (controller)
            {
                controller.SimpleMove(_defaultMoveSpeed * dirToPLayer);
            }
        }
    }

    protected void UpdatePathfinding()
    { 
        if (DidSwitchState())
            return;


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

    protected void UpdateRangedVsPlayer()
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

        if (Vector3.Dot(transform.forward, dirToPLayer) > Mathf.Cos(Mathf.Deg2Rad * 0.5f * _rangedSettings.RangeAngleInDegrees))
        {
            // Update attack timer loop.
            if (_attackTimer <= 0)
            {
                _attackTimer = _rangedSettings.HitTimer;
                PerformRangedAttack();
            }
        }
    }

    protected void PerformRangedAttack()
    {
        Debug.Log(name + " Ranged attack!");

        if (_animator)
        {
            _animator.SetTrigger("RangedAttack");
        }

        if (_rangedSettings.Projectile)
        {
            // Find direction to player
            var dirToPlayer = Player.transform.position - transform.position;
            dirToPlayer.Normalize();

            var spawnPosition = transform.position + transform.forward + Vector3.up;
            var spawnDirection = dirToPlayer;

            // Spawn a hit effect
            GameObject.Instantiate(_rangedSettings.Projectile, spawnPosition, Quaternion.LookRotation(dirToPlayer));
        }
    }

    // -----------------------------------------------------------------------------------------------------

    protected bool ShouldSwitchToMelee()
    {
        if (_meleeSettings.CanMelee == false)
            return false;
     
        if (_currentState == State.MeleeAttack)
            return false;

        if (Vector3.Distance(Player.position, transform.position) <= _meleeSettings.MeleeRange)
        {
            _currentState = State.MeleeAttack;
            return true;
        }

        return false;
    }

    protected bool ShouldSwitchToRanged()
    {   
        if (_rangedSettings.CanRangeAttack == false)
            return false;

        if (_currentState == State.RangedAttack)
            return false;

        if (Vector3.Distance(Player.position, transform.position) <= _rangedSettings.Range)
        {
            _currentState = State.RangedAttack;
            return true;
        }

        return false;
    }

    protected bool ShouldSwitchToPursuit()
    {
        if (_currentState == State.Pursuit)
            return false;

        if (Vector3.Distance(Player.position, transform.position) > _generalSettings.PursuitDistance)
        {
            _currentState = State.Pursuit;
            return true;
        }

        return false;
    }

    protected bool DidSwitchState()
    {
        if (ShouldSwitchToMelee() == false)
        {
            if (ShouldSwitchToRanged() == false)
            {
                if (ShouldSwitchToPursuit() == false)
                {
                    return false;
                }
            }
        }

        return true;
    }

    // -------------------------------------------------- DEBUG DRAWING --------------------------------------------------------
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (_meleeSettings.CanMelee)
        {
            // Draw helpers: Show angle lines in Scene view to see how big melee state radius is.
            Gizmos.DrawWireSphere(transform.position, _meleeSettings.MeleeRange);

            // Draw helpers: Show angle lines in Scene view to see how big melee angle range is.
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawLine(Vector3.zero, Quaternion.AngleAxis(-0.5f * _meleeSettings.MeleeAngleInDegrees, Vector3.up) * (_meleeSettings.MeleeRange * Vector3.forward));
            Gizmos.DrawLine(Vector3.zero, Quaternion.AngleAxis(0.5f * _meleeSettings.MeleeAngleInDegrees, Vector3.up) * (_meleeSettings.MeleeRange * Vector3.forward));
        }
    }
}
