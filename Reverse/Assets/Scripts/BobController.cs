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

                        weapons[i].id = i;
                        weapons[i - 1].id = i - 1;

                        isCorrectOrder = false;
                    }
                }
            }
            while (!isCorrectOrder);

            currentWeapon = weapons[0];
        }

        rigidbody.centerOfMass = new Vector3(0, -1f, 0);
    }


    private void Update()
    {
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

    private void LimitSpeed()
    {
        if (rigidbody.velocity.x > maxSpeed)
        {
            rigidbody.velocity = new Vector3(maxSpeed, rigidbody.velocity.y, 0);
        }
        else if (rigidbody.velocity.x < -maxSpeed)
        {
            rigidbody.velocity = new Vector3(-maxSpeed, rigidbody.velocity.y, 0);
        }
    }

    private void ApplyDrivingForce()
    {
        Vector3 drivingForce = Vector3.zero;

        if (isPanic || isAttacking || isInAnimation)
        {
            return;
        }
        else if (isDodging)
        {
            fleeDirection = Mathf.Sign(dodgeTarget.x - transform.position.x);

            drivingForce.x += fleeForceScale * fleeDirection;
            drivingForce.x += randomForceScale * GaussianRandom(0, 0.5f) * (1 - familiarity);
        }
        else
        {
            drivingForce.x += driveForceScale * (desiredPos.position.x - transform.position.x);
            drivingForce.x += randomForceScale * GaussianRandom(0, 0.5f);
        }

        rigidbody.AddForce(drivingForce);
    }

    /// <summary>
    /// Input info of coming attack
    /// </summary>
    /// <param name="attackBoundaries">The boundaries of the attack at any order (in world coord).</param>
    /// <param name="occurTimes">The number of times this attack has occurred.</param>
    /// <param name="duration">The duration of this attack (until the hitbox end)</param>
    public void AttackComing(float[] attackBoundaries, float occurTimes, float duration)
    {
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
        familiarity = occurTimes / (occurTimes + 5f);
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
    /// Update dodging status. Simutaneous attacks are not considered.
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdateDodgingState()
    {
        isPanic = true;
        isDodging = false;
        rigidbody.velocity = Vector3.zero;
        rigidbody.AddForce(100f * Vector3.up);

        yield return new WaitForSeconds(panicTime * (1 - familiarity));

        isPanic = false;
        isDodging = true;

        yield return new WaitForSeconds(attackDuration - panicTime * (1 - familiarity));

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
