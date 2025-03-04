using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobsTarget : MonoBehaviour
{
    private float dodgeDis;
    private float attackDis;

    [SerializeField] private Attack[] attacks;
    [SerializeField] private float maxCost;
    private float attackTotalCount;
    private float closestRange;
    private float farthestRange;

    [SerializeField] 
    Transform bob;
    Transform rob;
    private BobController bobController;

    
    //EVENT STUFF
    private EventBinding<BobDesiredPositionUpdateAttackEvent> robAttackEventBinding;

    private void OnEnable()
    {
        robAttackEventBinding = new EventBinding<BobDesiredPositionUpdateAttackEvent>((robAttackData) =>
        {
            UpdateAttackInfo(robAttackData.attackId, robAttackData.attackTimes);
        }); 
        EventBus<BobDesiredPositionUpdateAttackEvent>.Register(robAttackEventBinding);
    }

    private void OnDisable() => EventBus<BobDesiredPositionUpdateAttackEvent>.Deregister(robAttackEventBinding);

    private void Awake()
    {
        rob = transform.parent;
        bobController = bob.GetComponent<BobController>();
    }

    // Start is called before the first frame update
    private void Start()
    {
        dodgeDis = 0;
        attackDis = 0;

        attackTotalCount = 1;
        closestRange = 0;
        farthestRange = 0;

        attacks = rob.GetComponent<RobBasics>().attacks;

        if (attacks.Length != 0)
        {
            // Order the attacks according to minimum range order.
            bool isCorrectOrder;
            Attack[] attackTemp = attacks;

            do
            {
                isCorrectOrder = true;

                for (int i = 1; i < attacks.Length; i++)
                {
                    if (attacks[i].MinRange < attacks[i - 1].MinRange)
                    {
                        attacks[i] = attackTemp[i - 1];
                        attacks[i - 1] = attackTemp[i];

                        isCorrectOrder = false;
                    }
                }
            }
            while (!isCorrectOrder);

            // Initialize attack related parameters.
            foreach (Attack attack in attacks)
            {
                attackTotalCount += attack.occurTimes;
            }

            closestRange = attacks[0].MinRange;
            farthestRange = attacks[attacks.Length - 1].MaxRange;
        }

        UpdateDodgeDis();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        transform.localPosition = DesiredPos();
    }

    private Vector3 DesiredPos()
    {
        UpdateAttackDis();

        float desiredDis = 0;

        if (attackDis > dodgeDis)
        {
            // If can reach Rob, stay at desired dodge distance.
            desiredDis = dodgeDis;
        }
        else
        {
            // If cannot reach Rob, try to get closer according to attack desire.
            desiredDis = bobController.AttackDesire * attackDis + dodgeDis;
        }

        // The desired position is at the closer-to-bob side.
        Vector3 desiredPos = desiredDis * rob.right * 
            Mathf.Sign(bob.position.x - rob.position.x) * rob.transform.localScale.x;

        return desiredPos;
    }

    private void UpdateAttackDis()
    {
        attackDis = bobController.CurrentWeaponRange;
    }

    /// <summary>
    /// Call to recalculate desired position (may use when Bob respawn).
    /// </summary>
    public void UpdateDodgeDis()
    {
        float[] testPos = new float[20];
        float[] costs = new float[20];

        for (int i = 0; i < testPos.Length; i++)
        {
            testPos[i] = Mathf.Lerp(closestRange, farthestRange, (i + 1) / 20f);

            foreach (Attack attack in attacks)
            {
                if (!attack.hasInfluence) continue;

                // TO DO: known issue.
                // Calculate cost according to how far it is to escape an attack from a test point.
                float errorToMax = attack.MaxRange - testPos[i];
                float errorToMin = attack.MinRange > 0.6f ? (attack.MinRange - testPos[i]) : (-attack.MaxRange - testPos[i]);

                // 2 boundaries are at the same side, indicating the test point is not in attack range.
                if (Mathf.Sign(errorToMin) == Mathf.Sign(errorToMax))
                {
                    continue;
                }

                // Multiply the attack frequency to the cost.
                costs[i] += (attack.occurTimes / attackTotalCount) * 
                    Mathf.Min(Mathf.Abs(errorToMin), Mathf.Abs(errorToMax));
            }

            Debug.Log($"cost[{i}] = " + costs[i]);

            if (costs[i] < maxCost)
            {
                dodgeDis = testPos[i];
                break;
            }
        }
    }

    /// <summary>
    /// Call when certain attack has changed occur times.
    /// Need to call multiple times when multiple attacks updated.
    /// </summary>
    /// <param name="attackId">follow the id in attacks list in inspector</param>
    /// <param name="occurTimes">how many time in total has this attack occurred</param>
    public void UpdateAttackInfo(int attackId, int occurTimes)
    {
        attackTotalCount = 1;

        foreach (Attack attack in attacks)
        {
            if (attack.id == attackId)
                attack.occurTimes = occurTimes;

            attackTotalCount += attack.occurTimes;
        }

        UpdateDodgeDis();
    }
}
