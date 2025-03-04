using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Weather;

public class BobController : MonoBehaviour
{
    [SerializeField] private float attackBoundaryMin;
    [SerializeField] private float attackBoundaryMax;
    [SerializeField] private float attackOccurTimes;
    [SerializeField] private float attackDurationTemp;
    [SerializeField] private Collider2D testCollider;

    [Header("Status")]
    [SerializeField] private bool isInRange;
    [SerializeField] private bool isAttacking;
    [SerializeField] private bool isFreeze;
    [SerializeField] private bool isDodging;
    [SerializeField] private float health;
    [SerializeField] private float overallFamiliarity;

    private float maxHealth = 1.0f;
    private bool isInAnimation;

    [Header("Moving Properties")]
    [SerializeField] private float maxSpeed;
    [SerializeField] private float driveForceScale;
    [SerializeField] private float randomForceScale;

    [Header("Dodging Properties")]
    [SerializeField] private float panicTime;
    [SerializeField] private float dodgeErrorScale;
    [SerializeField] private float fleeForceScale;
    private float fleeDirection;
    private Vector3 dodgeTarget;
    private float[] attackBoundaries = new float[2];
    private List<AttackStatus> attackStatuses = new List<AttackStatus>(0);

    [Header("Attack Properties")]
    [SerializeField] private Weapon[] weapons;
    [SerializeField] private float attackDesire;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float attackTimer;
    private Weapon currentWeapon;

    [Header("References")]
    [SerializeField] private Transform rob;
    [SerializeField] private RobBasics robBasics;
    [SerializeField] private Transform respawnPos;
    private Transform desiredPos;
    private Collider2D collider;
    private Rigidbody2D rigidbody;
    private Animator animator;

    [Header("Bob's Drip")]
    [SerializeField] GameObject[] drip;
    int dripCounter = -1;


    #region EVENT STUFF
    private EventBinding<RobAttackEvent> robAttackEventBinding;
    private EventBinding<BobDieEvent> bobDieEventBinding;

    private void OnEnable()
    {
        robAttackEventBinding = new EventBinding<RobAttackEvent>((robAttackData) =>
        {
            AttackComing(robAttackData.attackBoundaries, robAttackData.occurTimes, robAttackData.duration);
        }); 
        EventBus<RobAttackEvent>.Register(robAttackEventBinding);
    }

    private void OnDisable() => EventBus<RobAttackEvent>.Deregister(robAttackEventBinding);
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
        animator = GetComponentInChildren<Animator>();

        if (rob == null)
        {
            rob = GameObject.Find("Rob").transform;
        }

        desiredPos = rob.Find("Bob's target").transform;
    }


    private void Start()
    {
        isAttacking = false;
        isFreeze = false;
        isDodging = false;
        isInAnimation = false;
        health = maxHealth;

        overallFamiliarity = 1.0f;
        fleeDirection = 0;
        dodgeTarget = new Vector3(float.MaxValue, 0, 0);

        attackTimer = -7f;

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

        //testCollider.gameObject.SetActive(false);
    }

    #region Update Methods
    private void Update()
    {
        // For temporary test only.
        // Press 'A' to activate pre-defined attack.
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    //float[] boundaries = { attackBoundaryMin, attackBoundaryMax };
        //    testCollider.gameObject.SetActive(true);

        //    AttackComing(testCollider, attackOccurTimes, attackDurationTemp);
        //}
        PrepareAttack();
    }

    private void CheckDeath()
    {
        if (health > 0.0f) return;

        EventBus<BobDieEvent>.Raise(new BobDieEvent() { });

        maxHealth += 0.4f;
        health = maxHealth;
        StartCoroutine(Killed());
        Debug.Log("Bob is killed!");
    }
    #endregion

    #region LateUpdate Methods
    private void LateUpdate()
    {
        CheckState();
    }

    private void CheckState()
    {
        isInRange = false;
        isDodging = false;

        for (int i = 0; attackStatuses != null && i < attackStatuses.Count; i++)
        {
            // Skip the ended attacks.
            if (attackStatuses[i].hasEnded) continue;

            // if is in active attack's range
            else if (attackStatuses[i].isInRange)
            {
                isInRange = true;
                isDodging = true;
            }
        }
    }
    #endregion


    private void FixedUpdate()
    {
        #region Animation
        if (rigidbody.velocity.x > 0.2f)
        {
            transform.localScale = new Vector3(1, transform.localScale.y, 1);
        }
        else if (rigidbody.velocity.x < -0.2f)
        {
            transform.localScale = new Vector3(-1, transform.localScale.y, 1);
        }

        if (Mathf.Abs(rigidbody.velocity.x) > 0.2f)
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
        #endregion

        ApplyDrivingForce();
        LimitSpeed();
    }

    /// <summary>
    /// Limit Bob's maximum speed.
    /// </summary>
    private void LimitSpeed()
    {
        // Speed limit is bigger when dodging.
        // This multiplier is calculated from attack overallFamiliarity and overall overallFamiliarity.
        float dodgeMult = 1;

        if (isDodging)
            dodgeMult = (1 + 12f * (attackStatuses[^1].familiarity - 0.8f)) * overallFamiliarity;

        // Limit the speed with multiplier.
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
        if (isFreeze || isAttacking || isInAnimation)
        {
            return;
        }
        // If Bob's in dodging state, add force towards the pre-calculated dodge target.
        else if (isDodging)
        {
            fleeDirection = Mathf.Sign(dodgeTarget.x - transform.position.x);

            // Driving force. Will be affected by *overall overallFamiliarity*.
            drivingForce.x += fleeForceScale * fleeDirection * overallFamiliarity;
            // Random force to make driving force less efficient. Will be influenced by *attack overallFamiliarity*.
            float familiarFactor = 5f * (1 - attackStatuses[^1].familiarity);
            drivingForce.x += randomForceScale * GaussianRandom(0, familiarFactor) * familiarFactor;
        }
        // If Bob's not in any state, add force towards the desired position (Bob's target).
        else
        {
            // Driving force.
            // Will be more influential as getting farther to desired position.
            drivingForce.x += driveForceScale * (desiredPos.position.x - transform.position.x);
            // Random force to make driving force less efficient. Will be influenced by *overall overallFamiliarity*.
            drivingForce.x += randomForceScale * GaussianRandom(0, 0.5f / overallFamiliarity) / overallFamiliarity;
        }

        rigidbody.AddForce(drivingForce);
    }

    #region Unused method
    ///// <summary>
    ///// Input info of coming attack. For temporary test only.
    ///// </summary>
    ///// <param name="attackBoundaries">The boundaries of the attack at any order (in world coord).</param>
    ///// <param name="occurTimes">The number of times this attack has occurred.</param>
    ///// <param name="duration">The duration of this attack (until the hitbox end)</param>
    //public void AttackComing(float[] attackBoundaries, float occurTimes, float duration)
    //{
    //    // Operate only when Bob's in Rob is not already dodging.
    //    if (isFreeze || isDodging)
    //        return;

    //    // Reset attack information.
    //    dodgeTarget = new Vector3(float.MaxValue, 0, 0);
    //    fleeDirection = 0;

    //    // Check if is currently in attack range. Do nothing if not.
    //    this.attackBoundaries[0] = Mathf.Min(attackBoundaries[0], attackBoundaries[1]) - collider.bounds.extents.x;
    //    this.attackBoundaries[1] = Mathf.Max(attackBoundaries[0], attackBoundaries[1]) + collider.bounds.extents.x;

    //    float[] errors = new float[2];
    //    errors[0] = this.attackBoundaries[0] - transform.position.x;
    //    errors[1] = this.attackBoundaries[1] - transform.position.x;

    //    // 2 boundaries are at the same side of Bob
    //    if (Mathf.Sign(errors[0]) == Mathf.Sign(errors[1]))        
    //    {
    //        Debug.Log("Attack will not hit Bob.");
    //        return;
    //    }

    //    // If is in the range, read coming attack information.
    //    overallFamiliarity = occurTimes / (occurTimes + 2f);

    //    // Compare distance of 2 boundaries. Possibly make mistake when unfamiliar.
    //    // Start at a random order.
    //    int randomOrder = UnityEngine.Random.Range(0, 2);
    //    for (int i = randomOrder; i < randomOrder + 2; i++)
    //    {
    //        if (Mathf.Abs(errors[i % 2]) <
    //            Mathf.Abs(transform.position.x - dodgeTarget.x) * (0.2f + 0.8f * UnityEngine.Random.Range(overallFamiliarity, 1)))
    //        {
    //            fleeDirection = Mathf.Sign(errors[i % 2]);
    //            dodgeTarget.x = this.attackBoundaries[i % 2];
    //        }
    //    }

    //    // Adjust dodge position to consider Bob's mistakes.
    //    dodgeTarget.x += fleeDirection * dodgeErrorScale * (Mathf.Exp(GaussianRandom(0, 1 - overallFamiliarity)) - 0.6f);

    //    Debug.Log("Dodge target: " + dodgeTarget.x);

    //    StartCoroutine(UpdateDodgingState());
    //}
    #endregion


    #region Bob Dodge
    /// <summary>
    /// Input info of coming attack (collider2D ver.).
    /// </summary>
    /// <param name="attackCollider">The collider of the coming attack.</param>
    /// <param name="occurTimes">The number of times this attack has occurred.</param>
    /// <param name="duration">The duration of this attack (until the hitbox end).</param>
    public void AttackComing(Collider2D attackCollider, float occurTimes, float duration)
    {
        if (transform.position.x < -13) return;
        // Record the attack info, and start updating its state.
        // This is to check if the OnTriggerStay2D method is reacting to the right collider.
        attackStatuses.Add(new AttackStatus(attackCollider, duration, occurTimes));
        StartCoroutine(UpdateAttackState(attackStatuses[^1]));

        // Decrease overall overallFamiliarity according to attack overallFamiliarity.
        overallFamiliarity *= attackStatuses[^1].familiarity;

        Debug.Log("Attack familiarity :" + attackStatuses[^1].familiarity);
    }


    /// <summary>
    /// Triggered when an Rob's attack range is activated and bob is in that range.
    /// Will use the collider info to find a target position where Bob can dodge it (move outside the bounds).
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerStay2D(Collider2D other)
    {
        AttackStatus attack = new AttackStatus(null, 0, 0);

        // Operate only when Bob's in an attack range.
        for (int i = 0; i < attackStatuses.Count; i++ )
        {
            // if attack list is empty, return.
            if (attackStatuses.Count == 0) { return; }
            

            // if any of the collider match 'other', break the loop.
            // NOTE: when an attack ends, it will be removed from list, causing an index change here. This can cause non-crashing errors.
            if ( attackStatuses.Count > 0 && other == attackStatuses[i].collider)
            {
                //Debug.Log("Attack index: " + i);
                attack = attackStatuses[i];
                break;
            }

            // if none of the collider matches until the last element, return.
            if (i == attackStatuses.Count) return;
        }


        /************  UPDATE ATTACK STATE  ************/

        attack.isInRange = true;

        // If Bob has reacted to this attack, return.
        if (attack.isReacted) return;
        else attack.isReacted = true;

        // If Bob is currently freezing, return.
        if (isFreeze) return;


        /************  CALCULATE ATTACK INFO  ************/

        // Reset attack information.
        dodgeTarget = new Vector3(float.MaxValue, 0, 0);
        fleeDirection = 0;

        // Initialize attack boundaries.
        attackBoundaries[0] = transform.position.x;
        attackBoundaries[1] = transform.position.x;

        // Calculate the attack boundaries.
        for (int i = 0; i < attackStatuses.Count; ++i)
        {
            if (attackStatuses[i].hasEnded || !attackStatuses[i].isInRange) continue;

            // Update the left boundary.
            attackBoundaries[0] =
                Mathf.Min(attackStatuses[i].collider.bounds.min.x - 2f * collider.bounds.extents.x,
                attackBoundaries[0]);

            // Update the right boundary.
            attackBoundaries[1] =
                Mathf.Max(attackStatuses[i].collider.bounds.max.x + 2f * collider.bounds.extents.x,
                attackBoundaries[1]);
        }


        /************  CALCULATE DODGE TARGET  ************/

        // Decide which boundary to head for.
        // Start at a random order.
        int randomOrder = UnityEngine.Random.Range(0, 2);
        for (int i = randomOrder; i < randomOrder + 2; i++)
        {
            // Bob may make mistake by underestimating distance to current target.
            // This is the underestimate multiplier.
            float calculateError = overallFamiliarity * 5.0f * (UnityEngine.Random.Range(attack.familiarity, 1) - 0.8f);

            // Compare the boundary with the existing target.
            if (Mathf.Abs(transform.position.x - attackBoundaries[i % 2]) <
                Mathf.Abs(transform.position.x - dodgeTarget.x) * calculateError)
            {
                fleeDirection = -Mathf.Sign(transform.position.x - attackBoundaries[i % 2]);
                dodgeTarget.x = attackBoundaries[i % 2];
            }
        }

        // Bob may make mistake to underestimate or overestimate the attack range.

        // If the multiplier is negative, Bob will get hit due to underestimating the range.
        // But for most of the time, Bob will overestimate it and move farther away.

        // NOTE: when overallFamiliarity = 0.5f, negative rate is approximately 15.6%.
        float estimateError = (Mathf.Exp(GaussianRandom(0, 10f * (1 - attack.familiarity))) * overallFamiliarity - 0.6f);
        dodgeTarget.x += fleeDirection * dodgeErrorScale * estimateError;

        //Debug.Log("Dodge target: " + dodgeTarget.x);

        // Update dodging state.
        StartCoroutine(UpdateDodgingState(attack));
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Operate only when Bob's moving out from an attack range.
        for (int i = 0; attackStatuses != null && i < attackStatuses.Count; i++)
        {
            // if any of the collider match 'other', break the loop.
            if (collision == attackStatuses[i].collider)
            {
                attackStatuses[i].isInRange = false;
                break;
            }
        }
    }


    /// <summary>
    /// Update attack status. Deal damage if is in range at end.
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdateAttackState(AttackStatus attackStatus)
    {
        yield return new WaitForSeconds(attackStatus.duration);

        // If Bob is still in range at attack end, he will get hurt (red flash for now).
        if (attackStatus.isInRange)
        {
            health -= 1.0f;
            CheckDeath();
            StartCoroutine(Blink());
        }

        attackStatus.hasEnded = true;
        attackStatuses.Remove(attackStatus);
    }


    /// <summary>
    /// Update dodging status. Simutaneous attacks not considered.
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdateDodgingState(AttackStatus attack)
    {
        isFreeze = true;

        // Bob will panic to jump when an attack comes.
        rigidbody.velocity = Vector3.zero;
        rigidbody.AddForce(100f * Vector3.up);

        // Bob will stay in panic state for a while, depending on his attack overallFamiliarity and overall overallFamiliarity.
        float actualPanicTime = panicTime * 5f * (1 - attack.familiarity);
        yield return new WaitForSeconds(Mathf.Max(0, GaussianRandom(actualPanicTime, 1 - overallFamiliarity)));

        isFreeze = false;
    }
    #endregion


    #region Bob Attack
    private void PrepareAttack()
    {
        attackTimer += Time.deltaTime;

        // If attack is not yet ready or bob is in any attack's range, return.
        if (attackTimer < 0 || isInRange) return;

        // When attack is ready, attack chance will grow as time.
        attackDesire = 0.3f * attackTimer;
        float attackChance = (1 - 1f / (1f + AttackDesire)) * Time.deltaTime;

        // When in dodging state(not in range), bob's attack chance grow according to familiarity.
        if (isDodging)
        {
            attackChance *= 1 + 3 * (attackStatuses[^1].familiarity - 0.8f);
        }

        // On certain condition, bob tries to conduct attack
        // if he failed due to lack of appropriate weapon, he wants it.
        if (UnityEngine.Random.Range(0, 1f) < attackChance)
        {
            // Go through the weapon type (from melee to ranged) to find suitable weapon to attack.
            foreach (Weapon weapon in weapons)
            {
                if (Mathf.Abs(transform.position.x - rob.position.x) > weapon.range)
                {
                    continue;
                }
                // Conduct attack and reset cooldown if he has the weapon.
                else if (weapon.isOwned)
                {
                    Debug.Log("Bob attacked!");
                    StartCoroutine(ConductAttack(weapon));
                    attackTimer = -attackCooldown;
                    return;
                }
                // If he does not have it, he wants it, also reset cooldown.
                else if (!weapon.isOwned)
                {
                    Debug.Log("Bob has no weapon! Bob wants a " + weapon.name + "!");
                    weapon.WantIt();
                    attackTimer = -0.7f * attackCooldown;
                    return;
                }
            }
        }
    }

    private IEnumerator ConductAttack(Weapon weapon)
    {
        isAttacking = true;
        rigidbody.velocity = Vector3.zero;
        animator.SetTrigger(weapon.name);

        yield return new WaitForSeconds(weapon.timeBeforeAttack);
        robBasics.TakeHealth(1 + dripCounter); //scaled it by dripcounter for now

        // Deal damage

        yield return new WaitForSeconds(weapon.totalActionTime - weapon.timeBeforeAttack);

        isAttacking = false;
    }
    #endregion


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

    private IEnumerator Blink()
    {
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.color = Color.red;
        }

        yield return new WaitForSeconds(0.2f);

        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.color = Color.white;
        }
    }

    private IEnumerator Killed()
    {
        isFreeze = true;

        yield return new WaitForSeconds(1.0f);

        transform.position = respawnPos.position;
        overallFamiliarity = 1;

        // Update bob weapons.
        attackCooldown *= 0.9f;
        attackTimer = -10f;

        foreach (Weapon weapon in weapons)
        {
            weapon.GetWeapon();
        }

        EventBus<BobRespawnEvent>.Raise(new BobRespawnEvent() { });
        isFreeze = false;

        // Upgrade the drip
        if (dripCounter + 1 < drip.Length) drip[++dripCounter].SetActive(true);


        // Clear all attack info.
        attackStatuses.Clear();
        StopAllCoroutines();
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
    public float timeBeforeAttack;
    public float totalActionTime;

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

public class AttackStatus
{
    public Collider2D collider;
    public float duration;
    public float familiarity;
    public bool isReacted = false;
    public bool isInRange = false;
    public bool hasEnded = false;

    public AttackStatus(Collider2D collider, float duration, float occurTimes)
    {
        this.collider = collider;
        this.duration = duration;
        familiarity = occurTimes; // Need update

        if (occurTimes >= 3)
        {
            familiarity = 1.0f;
        }
        else if (occurTimes >= 1)
        {
            familiarity = 0.96f;
        }
        else
        {
            familiarity = 0.8f;
        }
    }
}
