using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AttackSelectionHandler : MonoBehaviour
{
    [SerializeField] private float verticalMoveAmt = 30f;
    [SerializeField] private float moveTime = 0.1f;
    [Range(0f, 2f), SerializeField] private float scaleAmt = 1.1f;

    private Vector3 startPos;
    private Vector3 startScale;
    private Image currentAttackIcon = null;

    RobBasics robScript;
    [SerializeField] Image meteorIcon;
    [SerializeField] Image hellfireIcon;
    private void Awake()
    {
        robScript = FindObjectOfType<RobBasics>();
    }

    private void Update()
    {
        if(robScript.CanUseMeteor()) meteorIcon.color = Color.white;
        else meteorIcon.color = Color.black;

        if (robScript.CanUseHellfire()) hellfireIcon.color = Color.white;
        else hellfireIcon.color = Color.black;
    }

    public IEnumerator MoveCard(Image attack1Icon, Image attack2Icon, Image attack3Icon, float duration, bool startingAnim, int attackNum)
    {
        switch (attackNum)
        {
            case 1:
                currentAttackIcon = attack1Icon;
                attack2Icon.color = Color.grey;
                attack3Icon.color = Color.grey;
                break;
            case 2:
                currentAttackIcon = attack2Icon;
                attack1Icon.color = Color.grey;
                attack3Icon.color = Color.grey;
                break;
            case 3:
                currentAttackIcon = attack3Icon;
                attack1Icon.color = Color.grey;
                attack2Icon.color = Color.grey;
                break;
        }

        if(currentAttackIcon)
        {
            startPos = currentAttackIcon.transform.localPosition;
            startScale = currentAttackIcon.transform.localScale;

            Vector3 endPos;
            Vector3 endScale;

            float elapsedTime = 0f;
            while (elapsedTime < moveTime)
            {
                elapsedTime += Time.deltaTime;

                if (startingAnim)
                {
                    endPos = startPos + new Vector3(0f, verticalMoveAmt, 0f);
                    endScale = startScale * scaleAmt;
                    currentAttackIcon.color = Color.white;
                }
                else
                {
                    endPos = startPos;
                    endScale = startScale;
                }

                //calculate the lerped amount for pos and scale
                Vector3 lerpedPos = Vector3.Lerp(transform.localPosition, endPos, (elapsedTime / moveTime));
                Vector3 lerpedScale = Vector3.Lerp(transform.localScale, endScale, (elapsedTime / moveTime));

                //Apply the changes to pos and scale
                currentAttackIcon.transform.localPosition = lerpedPos;
                currentAttackIcon.transform.localScale = lerpedScale;
            }

            yield return new WaitForSeconds(duration);

            //Reset all for when the attack is over
            attack1Icon.color = Color.white;
            attack2Icon.color = Color.white;
            attack3Icon.color = Color.white;
            currentAttackIcon.transform.localPosition = startPos;
            currentAttackIcon.transform.localScale = startScale;
        }
    }
}
