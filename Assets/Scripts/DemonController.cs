using UnityEngine;
using System.Collections;

using UnityStandardAssets.Characters.ThirdPerson;

public class DemonController : MonoBehaviour {

    [System.Serializable]
    public class GeneralSettings
    {
        public float Health = 1.0f;
        public float PursuitDistance = 6.0f;
        public GameObject DeathEffect;
    }

    [System.Serializable]
    public class RangedAttackSettings
    {
        public bool CanRangeAttack = false;
        public float Range = 10.0f;
        public float HitTimer = 3.0f;
        public float RangeAngleInDegrees = 15.0f;
        public EnemyProjectile Projectile;
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
    private float _health = 2;

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


    // Possible controllers
    private AICharacterControl _aiController;
    private ThirdPersonCharacter _thirdPersonController;
    private CharacterController _characterController;

    private EnemyManager _enemyManager;

    void Awake()
    {
        if(_animator == null)
            _animator = GetComponent<Animator>();
        
        _aiController = GetComponent<AICharacterControl>();
        _thirdPersonController = GetComponent<ThirdPersonCharacter>();
        _characterController = GetComponent<CharacterController>();
    }

    void OnDestroy()
    {
        if (_enemyManager)
            _enemyManager.RemoveDemon(this);
    }

	// Use this for initialization
	void Start () {
	
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj)
        {
            Player = playerObj.transform;
        }

        _enemyManager = GameObject.FindObjectOfType<EnemyManager>();
        if (_enemyManager)
        {
            _enemyManager.AddDemon(this);
        }

        _health = _generalSettings.Health;
	}
	
	// Update is called once per frame
	void Update () {
	
        CheckAIRoutine();
       

        _attackTimer -= Time.deltaTime;
	}

    public void DoDamage(float damage)
    {
        if (_currentState == State.Dead)
            return;

        _health -= damage;

        // Died? Spawn death anim
        if (_health <= 0)
        {
            _currentState = State.Dead;

            if (Player)
            {
                Player.GetComponent<CharacterLightController>().AddLight(_generalSettings.Health);
            }

            if (_enemyManager)
                _enemyManager.RemoveDemon(this);

            PerformDeathAnimation();
            SpawnDeathEffect();

            Destroy(gameObject, 1.0f);
        } else
        {
            PerformHitAnimation();
        }

    }

    // ---------------------------------------------------------------------------------------

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

        // If we have an AI controller and Nav mesh, use that
        if (_aiController)
        {
            _aiController.target = Player;
            _aiController.agent.SetDestination(Player.position);
            _aiController.agent.Resume();

        } else
        {
            // Otherwise, if we have a third person character controller, use that to move.
            if (_thirdPersonController)
            {
                _thirdPersonController.Move(dirToPLayer, false, false);
            } 
            else
            {
                // Lastly, try and fall back to a generic character controller.
                if (_characterController)
                {
                    _characterController.SimpleMove(_defaultMoveSpeed * dirToPLayer);
                }
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

        // Stop the player in melee
        if(_aiController)
        {
            _aiController.agent.Stop();
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
       
        if (Vector3.Distance(Player.position, transform.position) > _rangedSettings.Range)
        {
            _currentState = State.PatrolA;
            return;
        }

        if (ShouldSwitchToMelee())
        {
            return;
        }

        // Stop the agent if he can fire.
        if(_aiController)
        {
            _aiController.agent.Stop();
        }

        // Find direction to player and turn towards him
        var vToPlayer = Player.transform.position - transform.position;
        var dirToPLayer = vToPlayer.normalized;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dirToPLayer), 5.0f);

        if (Vector3.Dot(transform.forward, dirToPLayer) > Mathf.Cos(Mathf.Deg2Rad * 0.5f * _rangedSettings.RangeAngleInDegrees))
        {
            if (CanSeePlayer())
            {
                // Update attack timer loop.
                if (_attackTimer <= 0)
                {
                    _attackTimer = _rangedSettings.HitTimer;
                    PerformRangedAttack();
                }
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
            var projectile = (EnemyProjectile)GameObject.Instantiate(_rangedSettings.Projectile, spawnPosition, Quaternion.LookRotation(dirToPlayer));
            projectile.SetDirection(dirToPlayer);
        }
    }

    protected bool CanSeePlayer()
    {
        RaycastHit hitInfo;
        if (Physics.Linecast(transform.position + Vector3.up, Player.position + Vector3.up, out hitInfo))
        {
            if(hitInfo.collider.CompareTag("Player"))
                return true;
        }

        return false;
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

    // ------------------------------------- Death -------------------------------------------------

    protected void PerformHitAnimation()
    {
        if (_animator)
        {
            _animator.SetTrigger("Hit");
        }
    }

    protected void PerformDeathAnimation()
    {
        if (_animator)
        {
            _animator.SetTrigger("Death");
        }
    }

    protected void SpawnDeathEffect()
    {
        if (_generalSettings.DeathEffect)
        {
            GameObject.Instantiate(_generalSettings.DeathEffect, transform.position, Quaternion.identity);
        }
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

    void OnGUI()
    {
        GUILayout.Label( string.Format("EnemyState: {0}", _currentState.ToString()) );
    }
}
