using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WaddleDee : MonoBehaviour
{
    //*******DON'T CHANGE THESE*******
    public Sprite stand;
    public Sprite walk;
    public Sprite scared;
    public float groundedCastLength;
    public float groundedInFrontDistance;
    private bool prevGrounded = false;
    public LayerMask groundedLayerMask;
    public float accel;
    public float maxSpeed;
    public Vector2 jumpForce;
    private Rigidbody2D rb;
    private Camera mainCam;
    //********************************


    //***USE THESE FOR THE EXERCISE***
    private int facing = -1; //A value of 1 faces Waddle Dee to the right, - 1 faces them to the left
    private bool jumped = false; //True when Waddle Dee has jumped and has not landed yet
    private bool grounded = false; //Is Waddle Dee on the ground?
    private bool groundInFront = false; //Is there ground in front of Waddle Dee?
    private bool wallInFront = false; //Is there a wall in front of Waddle Dee?
    //********************************

    private AIState currentState = AIState.StateOne;
    public enum AIState
    {
        //TODO: Change these to your own desired states
        StateOne,
        StateTwo,
        StateThree,
    }

    private void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
    }

    private void Update()
    {
        UpdateState();
    }

    private void StartState(AIState newState)
    {
        EndState(currentState);

        switch (newState)
        {
            case AIState.StateOne:
                //your code here
                break;
            case AIState.StateTwo:
                //your code here
                break;
            case AIState.StateThree:
                //your code here
                break;
        }
    }

    private void UpdateState()
    {
        switch (currentState)
        {
            case AIState.StateOne:
                //your code here
                break;
            case AIState.StateTwo:
                //your code here
                break;
            case AIState.StateThree:
                //your code here
                break;
        }
    }

    private void EndState(AIState oldState)
    {
        switch (oldState)
        {
            case AIState.StateOne:
                //your code here
                break;
            case AIState.StateTwo:
                //your code here
                break;
            case AIState.StateThree:
                //your code here
                break;
        }
    }



    //Call every frame to update the boolean variables "grounded," "groundInFront," and "wallInFront"
    private void UpdateGrounded()
    {
        prevGrounded = grounded;

        grounded = Physics2D.Raycast(this.transform.position, Vector2.down, groundedCastLength, groundedLayerMask);
        groundInFront = Physics2D.Raycast(this.transform.position + (Vector3.right * groundedInFrontDistance * facing), Vector2.down, groundedCastLength * 1.5f, groundedLayerMask);
        wallInFront = Physics2D.Raycast(this.transform.position, Vector2.right * facing, groundedCastLength * 1.5f, groundedLayerMask);

        Debug.DrawRay(this.transform.position, Vector2.down * groundedCastLength, Color.red);
        Debug.DrawRay(this.transform.position + (Vector3.right * groundedInFrontDistance * facing), Vector2.down * groundedCastLength, Color.red);
        Debug.DrawRay(this.transform.position, Vector2.right * facing * groundedCastLength, Color.red);

        if (!prevGrounded && grounded)
            jumped = false;
    }

    //Call every frame to walk the Waddle Dee forward based on their facing
    private void Walk()
    {
        rb.AddForce(Vector2.right * facing * accel * (maxSpeed - Mathf.Abs(rb.velocity.x)));
    }

    //Call once to make Waddle Dee jump forward at an angle
    private void Jump()
    {
        if (jumped)
            return;

        rb.AddForce(new Vector2(jumpForce.x * facing, jumpForce.y), ForceMode2D.Impulse);
        jumped = true;
    }

    //Call to get the mouse position in world coordinates
    private Vector3 GetMousePosition()
    {
        //Grab the mouse position
        Vector3 mousePos = Input.mousePosition;

        //convert it to world coordinates
        return mainCam.ScreenToWorldPoint(mousePos);
    }

    //Call this and pass it the "stand," "walk," or "scared" sprite variables to change the sprite
    private void ChangeSprite(Sprite newSprite)
    {
        this.GetComponent<SpriteRenderer>().sprite = newSprite;
    }
}
