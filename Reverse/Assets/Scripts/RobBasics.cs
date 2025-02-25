using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobBasics : MonoBehaviour
{
    public int health;
    public int moveSpd;
    public BoxCollider2D meleeBox;
    public BoxCollider2D rangeBox;
    public BoxCollider2D aoeBox;

    // Start is called before the first frame update
    void Start()
    {
        health = 20;
        moveSpd = 2;
    }

    // Update is called once per frame
    void Update()
    {
        InputCheck();
    }

    //Health function
    void TakeHealth(int dmg)
    {
        health -= dmg;
    }

    //handles all inputs
    void InputCheck()
    {
        //move left
        if (Input.GetKey(KeyCode.A))
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(-moveSpd, 0);
        }

        //move right
        if (Input.GetKey(KeyCode.D))
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(moveSpd, 0);
        }

        //stop left
        if (Input.GetKeyUp(KeyCode.A))
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }

        //stop right
        if (Input.GetKeyUp(KeyCode.D))
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }

        //attack 1
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            AttackMelee();
        }

        //attack 2
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            AttackRanged();
        }

        //attack 3
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            AttackAoE();
        }
    }

    //melee attack functionality, called in InputCheck() (attack1)
    void AttackMelee()
    {

    }

    //ranged attack functionality, called in InputCheck() (attack2)
    void AttackRanged()
    {
        
    }

    //aoe attack functionality, called in InputCheck() (attack3)
    void AttackAoE()
    {

    }
}
