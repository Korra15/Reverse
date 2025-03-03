using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AttackSelectionHandler : MonoBehaviour
{
    [SerializeField] Image AttackImageComponent;
    [SerializeField] private float verticalMoveAmt = 30f;
    [SerializeField] private float moveTime = 0.1f;
    [Range(0f, 2f), SerializeField] private float scaleAmt = 1.1f;

    private Vector3 startPos;
    private Vector3 startScale;
    //private Color unSelectedColor = new(140, 140, 140);

    private void Start()
    {
        if (!AttackImageComponent) AttackImageComponent = gameObject.GetComponent<Image>();
        startPos = transform.localPosition;
        startScale = transform.localScale;
    }

    public IEnumerator MoveCard(float duration, bool startingAnim)
    {
        Vector3 endPos;
        Vector3 endScale;

        float elapsedTime = 0f;
        while(elapsedTime < moveTime)
        {
            elapsedTime += Time.deltaTime;

            if(startingAnim)
            {
                endPos = startPos + new Vector3(0f, verticalMoveAmt, 0f);
                endScale = startScale * scaleAmt;
                AttackImageComponent.color = Color.white;
            }
            else
            {
                endPos = startPos;
                endScale = startScale;
            }

            //calculate the lerped amount for pos and scale
            Vector3 lerpedPos = Vector3.Lerp(transform.localPosition, endPos, (elapsedTime/moveTime)); 
            Vector3 lerpedScale = Vector3.Lerp(transform.localScale, endScale, (elapsedTime/moveTime)); 

            //Apply the changes to pos and scale
            transform.localPosition = lerpedPos;
            transform.localScale = lerpedScale;
        }

        yield return new WaitForSeconds(duration);

        //Reset all for when the attack is over
        AttackImageComponent.color = Color.grey;
        transform.localPosition = startPos;
        transform.localScale = startScale;
    }
}
