using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobController : MonoBehaviour
{
    [SerializeField] private float attackBoundaryMin;
    [SerializeField] private float attackBoundaryMax;
    [SerializeField] private float attackOccurTimes;
    [SerializeField] private float attackDurationTemp;

    [Header("Status")]
    [SerializeField] private bool isAttacking;
    [SerializeField] private bool isPanic;
    [SerializeField] private bool isDodging;
    private bool isInAnimation;

    [Header("Moving Properties")]
    [SerializeField] private float maxSpeed;
    [SerializeField] private float driveForceScale;
    [SerializeField] private float randomForceScale;

    [Header("Dodging Properties")]
    [SerializeField] private float panicTime;
    [SerializeField] private float dodgeErrorScale;
    [SerializeField] private float fleeForceScale;
    private float familiarity;
    private float attackDuration;
    private float fleeDirection;
    private float[] attackBoundaries = new float[2];
    private Collider2D attackCollider;
    private Vector3 dodgeTarget;

    [Header("Attack Properties")]
    [SerializeField] private Weapon[] weapons;
    [SerializeField] private float attackDesire;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float attackTimer;
    private Weapon currentWeapon;

    [Header("References")]
    [SerializeField] private Transform rob;
    private Transform desiredPos;
    private Collider2D collider;
    private Rigidbody2D rigidbody;


    #region EVENT STUFF
    private EventBinding<AttackEvents.RobAttackEvent> robAttackEventBinding;

    private void OnEnable()
    {
        robAttackEventBinding = new EventBinding<AttackEvents.RobAttackEvent>((robAttackData) =>
        {
            AttackComing(robAttackData.attackBoundaries, robAttackData.occurTimes, robAttackData.duration);
        }); 
        EventBus<AttackEvents.RobAttackEvent>.Register(robAttackEventBinding);
    }

    private void OnDisable() => EventBus<AttackEvents.RobAttackEvent>.Deregister(robAttackEventBinding);
    #endregion

    #region PROPERTY
    public float AttackDesire
    {
        get { return attackDesire; }
    }

    public float CurrentWeaponRange
    {
        get
        {
            if (currentWeapon != null)
            {
                return currentWeapon.range;
            }
            else
            {
                return 0f;
            }
        }
    }
    #endregion


    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();

        if (rob == null)
        {
            rob = GameObject.Find("Rob").transform;
        }

        desiredPos = rob.Find("Bob's target").transform;
    }


    private void Start()
    {
        isAttacking = false;
        isPanic = false;
        isDodging = false;
        isInAnimation = false;

        familiarity = 0;
        attackDuration = 0;
        fleeDirection = 0;
        dodgeTarget = new Vector3(float.MaxValue, 0, 0);

        attackTimer = 0;

        if (weapons.Length != 0)
        {
            // Order the weapon according to attack range order.
            bool isCorrectOrder;
            Weapon[] weaponTemp = weapons;

            do
            {
                isCorrectOrder = true;

                for (int i = 1; i < weapons.Length; i++)
                {
                    if (weapons[i].range < weapons[i - 1].range)
                    {
                        weapons[i] = weaponTemp[i - 1];
                        weapons[i - 1] = weaponTemp[i];

                        isCorrectOrder = false;
                    }
                }
            }
            while (!isCorrectOrder);

            // Initialize current weapon.
            currentWeapon = weapons[0];
        }

        // Initialize center of mass for rotation.
        rigidbody.centerOfMass = new Vector3(0, -1f, 0);
    }


    private void Update()
    {
        // For temporary test only.
        // Press 'A' to activate pre-defined attack.
        if (Input.GetKeyDown(KeyCode.A))
        {
            float[] boundaries = { attackBoundaryMin, attackBoundaryMax };

            AttackComing(boundaries, attackOccurTimes, attackDurationTemp);
        }
    }


    private void FixedUpdate()
    {
        ApplyDrivingForce();
        LimitSpeed();
    }


    /// <summary>
    /// Limit Bob's maximum speed.
    /// </summary>
    private void LimitSpeed()
    {
        // Speed limit is bigger when dodging (depending on familiarity).
        float dodgeMult = 1;

        if (isDodging)
            dodgeMult = (1 + familiarity);

        // Limit the speed.
        if (rigidbody.velocity.x > maxSpeed * dodgeMult)
        {
            rigidbody.velocity = new Vector3(maxSpeed * dodgeMult, rigidbody.velocity.y, 0);
        }
        else if (rigidbody.velocity.x < -maxSpeed * dodgeMult)
        {
            rigidbody.velocity = new Vector3(-maxSpeed * dodgeMult, rigidbody.velocity.y, 0);
        }
    }


    private void ApplyDrivingForce()
    {
        Vector3 drivingForce = Vector3.zero;

        // If Bob's in panic/attacking/animation state, do nothing.
        if (isPanic || isAttacking || isInAnimation)
        {
            return;
        }
        // If Bob's in dodging state, add force towards the pre-calculated dodge target.
        else if (isDodging)
        {
            fleeDirection = Mathf.Sign(dodgeTarget.x - transform.position.x);

            // Driving force
            drivingForce.x += fleeForceScale * fleeDirection;
            // Random force to make driving force less efficient.
            // Will be less influential as familiarity grows.
            drivingForce.x += randomForceScale * GaussianRandom(0, 0.5f) * (1 - familiarity);
        }
        // If Bob's not in any state, add force towards the desired position (Bob's target).
        else
        {
            // Driving force.
            // Will be more influential as getting farther to desired position.
            drivingForce.x += driveForceScale * (desiredPos.position.x - transform.position.x);
            // Random force to make driving force less efficient.
            drivingForce.x += randomForceScale * GaussianRandom(0, 0.5f);
        }

        rigidbody.AddForce(drivingForce);
    }


    /// <summary>
    /// Input info of coming attack. For temporary test only.
    /// </summary>
    /// <param name="attackBoundaries">The boundaries of the attack at any order (in world coord).</param>
    /// <param name="occurTimes">The number of times this attack has occurred.</param>
    /// <param name="duration">The duration of this attack (until the hitbox end)</param>
    public void AttackComing(float[] attackBoundaries, float occurTimes, float duration)
    {
        // Operate only when Bob's in Rob is not already dodging.
        if (isPanic || isDodging)
            return;

        // Reset attack information.
        dodgeTarget = new Vector3(float.MaxValue, 0, 0);
        fleeDirection = 0;

        // Check if is currently in attack range. Do nothing if not.
        this.attackBoundaries[0] = Mathf.Min(attackBoundaries[0], attackBoundaries[1]) - collider.bounds.extents.x;
        this.attackBoundaries[1] = Mathf.Max(attackBoundaries[0], attackBoundaries[1]) + collider.bounds.extents.x;

        float[] errors = new float[2];
        errors[0] = this.attackBoundaries[0] - transform.position.x;
        errors[1] = this.attackBoundaries[1] - transform.position.x;

        // 2 boundaries are at the same side of Bob
        if (Mathf.Sign(errors[0]) == Mathf.Sign(errors[1]))        
        {
            Debug.Log("Attack will not hit Bob.");
            return;
        }

        // If is in the range, read coming attack information.
        familiarity = occurTimes / (occurTimes + 2f);
        attackDuration = duration;

        // Compare distance of 2 boundaries. Possibly make mistake when unfamiliar.
        // Start at a random order.
        int randomOrder = UnityEngine.Random.Range(0, 2);
        for (int i = randomOrder; i < randomOrder + 2; i++)
        {
            if (Mathf.Abs(errors[i % 2]) <
                Mathf.Abs(transform.position.x - dodgeTarget.x) * (0.2f + 0.8f * UnityEngine.Random.Range(familiarity, 1)))
            {
                fleeDirection = Mathf.Sign(errors[i % 2]);
                dodgeTarget.x = this.attackBoundaries[i % 2];
            }
        }

        // Adjust dodge position to consider Bob's mistakes.
        dodgeTarget.x += fleeDirection * dodgeErrorScale * (Mathf.Exp(GaussianRandom(0, 1 - familiarity)) - 0.6f);

        Debug.Log("Dodge target: " + dodgeTarget.x);

        StartCoroutine(UpdateDodgingState());
    }


    /// <summary>
    /// Input info of coming attack (collider2D ver.).
    /// </summary>
    /// <param name="attackCollider">The collider of the coming attack.</param>
    /// <param name="occurTimes">The number of times this attack has occurred.</param>
    /// <param name="duration">The duration of this attack (until the hitbox end).</param>
    public void AttackComing(Collider2D attackCollider, float occurTimes, float duration)
    {
        // Record the attack collider info.
        // This is to check if the OnTriggerStay2D method is reacting to the right collider.
        this.attackCollider = attackCollider;

        // Store a familiarity according to attack occurred times.
        familiarity = occurTimes / (occurTimes + 2f);
        attackDuration = duration;
    }


    /// <summary>
    /// Triggered when an Rob's attack range is activated and bob is in that range.
    /// Will use the collider info to find a target position where Bob can dodge it (move outside the bounds).
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerStay2D(Collider2D other)
    {
        // Operate only when Bob's in Rob's attack range and is not already dodging.
        if (other != attackCollider || isPanic || isDodging)
            return;

        // Reset attack information.
        dodgeTarget = new Vector3(float.MaxValue, 0, 0);
        fleeDirection = 0;

        // Calculate the attack boundaries.
        attackBoundaries[0] = other.bounds.min.x - collider.bounds.extents.x;
        attackBoundaries[1] = other.bounds.max.x + collider.bounds.extents.x;

        // familiarity = ?
        // attackDuration = ?

        // Decide which boundary to head for.
        // Start at a random order.
        int randomOrder = UnityEngine.Random.Range(0, 2);
        for (int i = randomOrder; i < randomOrder + 2; i++)
        {
            // Bob may make mistake by underestimating distance to current target.
            // This is the underestimate multiplier.
            float calculateError = (0.2f + 0.8f * UnityEngine.Random.Range(familiarity, 1));

            // Compare the boundary with the existing target.
            if (Mathf.Abs(transform.position.x - attackBoundaries[i % 2]) <
                Mathf.Abs(transform.position.x - dodgeTarget.x) * calculateError)
            {
                fleeDirection = Mathf.Sign(transform.position.x - attackBoundaries[i % 2]);
                dodgeTarget.x = attackBoundaries[i % 2];
            }
        }

        // Bob may make mistake to underestimate or overestimate the attack range.

        // If the multiplier is negative, Bob will get hit due to underestimating the range.
        // But for most of the time, Bob will overestimate it and move farther away.

        // NOTE: when familiarity = 0.5f, negative rate is approximately 15.6%.
        float estimateError = (Mathf.Exp(GaussianRandom(0, 1 - familiarity)) - 0.6f);
        dodgeTarget.x += fleeDirection * dodgeErrorScale * estimateError;

        Debug.Log("Dodge target: " + dodgeTarget.x);

        // Update dodging state.
        StartCoroutine(UpdateDodgingState());
    }


    /// <summary>
    /// Update dodging status. Simutaneous attacks not considered.
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdateDodgingState()
    {
        isPanic = true;
        isDodging = false;

        // Bob will panic to jump when an attack comes.
        rigidbody.velocity = Vector3.zero;
        rigidbody.AddForce(100f * Vector3.up);

        // Bob will stay in panic state for a while, depending on his familiarity.
        yield return new WaitForSeconds(panicTime * (1 - familiarity));

        isPanic = false;
        isDodging = true;

        // Bob will stop dodging after the attack is fully ended.
        yield return new WaitForSeconds(attackDuration - panicTime * (1 - familiarity) + 0.05f);

        isPanic = false;
        isDodging = false;
    }


    private void TryAttack()
    {

    }


    #region Help Functions
    /// <summary>
    /// Generate a gaussian random number.
    /// </summary>
    /// <param name="mean"></param>
    /// <param name="stdDev"></param>
    /// <returns></returns>
    public static float GaussianRandom(float mean, float stdDev)
    {
        float u1 = 1.0f - UnityEngine.Random.Range(0, 1f);
        float u2 = 1.0f - UnityEngine.Random.Range(0, 1f);

        float z = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Cos(2.0f * Mathf.PI * u2);

        return mean + stdDev * z;
    }
    #endregion
}


[Serializable]
public class Weapon
{
    public string name;
    public int id;
    public bool isOwned = false;
    public float range;
    public float damage;

    private float desire = 0;

    public void WantIt()
    {
        desire += 20;
    }

    public void GetWeapon()
    {
        if (desire >= 100)
        {
            isOwned = true;
        }
    }
}
