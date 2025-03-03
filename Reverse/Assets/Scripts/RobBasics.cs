using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RobBasics : MonoBehaviour
{
    // Const
    private int MELEE = 0;
    private int RANGED = 1;
    private int AOE = 2;

    //variables
    //rob values
    public int health = 100;
    public int moveSpd = 2;

    // Use this to manage attacks in the inspector (including colliders).
    public Attack[] attacks;

    ////box colliders
    //public BoxCollider2D meleeBox;
    //public BoxCollider2D rangeBox;
    //public BoxCollider2D aoeBox;

    //bools
    private bool isAttacking;

    //animator
    private Animator animator;

    [SerializeField]
    private InputTracker inputTracker;
    [SerializeField]
    private GameObject magnetoRock;
    [SerializeField]
    private Transform rockSpawnPos;
    [SerializeField]
    private Transform bobPos;

    [SerializeField] private Image attack1, attack2, attack3;

    // Start is called before the first frame update
    void Start()
    {
        health = 20;
        //moveSpd = 2;
        isAttacking = false;
        animator = gameObject.GetComponent<Animator>();

        foreach (Attack attack in attacks)
        {
            attack.collider.gameObject.SetActive(false);
        }

        //get position of bob
        if (bobPos != null)
        {
            bobPos = GameObject.Find("Bob").transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        InputCheck();
    }

    //Health function. Called from Bob's script.
    public void TakeHealth(int dmg)
    {
        health -= dmg;
        EventBus<RobHealthDecrease>.Raise(new RobHealthDecrease());
    }

    //handles all inputs
    void InputCheck()
    {
        // ban input when attacking.
        if (isAttacking)
        {
            animator.ResetTrigger("trRun");
            animator.SetTrigger("trIdle");
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            return;
        }

        //move left
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            transform.localScale = new Vector2(1, 1);
            animator.SetTrigger("trRun");
            GetComponent<Rigidbody2D>().velocity = new Vector2(-moveSpd, 0);
        }

        //move right
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            animator.SetTrigger("trRun");
            transform.localScale = new Vector2(-1, 1);
            GetComponent<Rigidbody2D>().velocity = new Vector2(moveSpd, 0);
        }

        //stop left
        if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.A))
        {
            animator.ResetTrigger("trRun");
            animator.SetTrigger("trIdle");
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }

        //stop right
        if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.D))
        {
            animator.ResetTrigger("trRun");
            animator.SetTrigger("trIdle");
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }

        //attack 1
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            animator.SetTrigger("trMelee");
            StartCoroutine(ConductAttack(attacks[MELEE]));
            //AttackMelee();
            AttackImageAnimaiton(attack1, attacks[MELEE].totalActionTime);
        }

        //attack 2
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            animator.SetTrigger("trRanged");
            GameObject rock = GameObject.Instantiate(magnetoRock, rockSpawnPos.position, Quaternion.identity, transform);
            StartCoroutine(ConductAttack(attacks[RANGED]));
            //AttackRanged();
            AttackImageAnimaiton(attack2, attacks[RANGED].totalActionTime);
        }

        //attack 3
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            animator.SetTrigger("trAoe");
            StartCoroutine(ConductAttack(attacks[AOE]));
            //AttackAoE();
            AttackImageAnimaiton(attack3, attacks[AOE].totalActionTime);
        }
    }

    private IEnumerator ConductAttack(Attack attack)
    {
        // Set isAttacking to true, so that all other inputs are banned.
        isAttacking = true;

        // Activate the corresponding attack object.
        attack.collider.gameObject.SetActive(true);

        // If collider is AoE
        if (attack.collider.gameObject.name == "AoE Attack")
        {
            attack.collider.gameObject.transform.position = new Vector3(bobPos.position.x, bobPos.position.y);
        }

        // Give input tracker attack info.
        inputTracker.AddInput(attack.id.ToString(), attack.collider, attack.timeBeforeHit);

        // wait until hitbox ends.
        yield return new WaitForSeconds(attack.timeBeforeHit + 0.05f);

        // Deactivate the attack object.
        attack.collider.gameObject.SetActive(false);

        // wait until the animation ends.
        yield return new WaitForSeconds(attack.totalActionTime - attack.timeBeforeHit);

        // set isAttcaking to false, so that other inputs are available.
        isAttacking = false;
    }

    ///<summary> Calling this for UI animaiton of selected attack button </summary>
    private void AttackImageAnimaiton(Image attackImage, float attackAnimTime)
    {
        StartCoroutine(attackImage.GetComponent<AttackSelectionHandler>().MoveCard(attackAnimTime, true));
    }
}


/// <summary>
/// This is a public class for attack managing.
/// This 'Attack' class can be accessed by any other scripts, so change it carefully.
/// </summary>
[Serializable]
public class Attack
{
    public int id;
    public float occurTimes;

    public float damage;
    public float timeBeforeHit;
    public float totalActionTime;

    public Collider2D collider;


    public float MinRange
    {
        get
        {
            float localX = Mathf.Abs(collider.transform.localPosition.x);
            float extentX = collider.bounds.extents.x;

            return Mathf.Max(0, localX - extentX);
        }
    }

    public float MaxRange
    {
        get
        {
            float localX = Mathf.Abs(collider.transform.localPosition.x);
            float extentX = collider.bounds.extents.x;

            return Mathf.Max(0, localX + extentX);
        }
    }
}
