using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class CharacterController2D : MonoBehaviour
{
    public float runSpeed = 20f; // Amount of force added when the player move.
    public float wallSlidingSpeed = 2f;
    public float jumpForce = 400f;  // Amount of force added when the player jumps.
    public float jumpTime = 0.35f;   // Amount of time the player allow to jump.
    public float maxFallingSpeed = 18f;   //SPeed limit of falling character.
    [Range(0, 1)] public float crouchSpeed = .36f;   // Amount of maxSpeed applied to crouching movement. 1 = 100%
    [Range(0, .3f)] public float movementSmoothing = .05f; // How much to smooth out the movement
    public bool airControl = true;   // Whether or not a player can steer while jumping;
    public LayerMask wallLayer;   // A mask determining what is Wall to the character
    public Transform wallCheck;   // A position marking where to check for Wall
    public LayerMask groundLayer;    // A mask determining what is ground to the character
    public Transform groundCheck;   // A position marking where to check if the player is grounded.
    public LayerMask ledgeLayer;    // A mask determining what is ledge to the character
    public Transform ceilingCheck;   // A position marking where to check for ceilings
    public Collider2D crouchDisableCollider;  // A collider that will be disabled when crouching
    public GameObject trail; // trail of the character dashing animation
    public Transform cloudPosition; // position right below the character for cloud
    public GameObject cloud; // A cloud for double jump effect
    public Collider2D attackCollider; // A collider for attack range
    public static int attackDamage = 5; //Attack damage of the character

    public GameObject audio; //Audio Object to control sound play

    private ContactFilter2D attackFilter;
    private Collider2D[] hitEnemies;


    public static PlayerInput playerInput;
    private InputAction horizontalMovement;
    private InputAction dashAction;
    private InputAction jumpAction;
    private InputAction attackAction;

    public Animator animator;
    const float checkRadius = .2f; // Radius of the overlap circle to check for ground, ceiling and wall
    private float jumpTimeCounter; // / Counter of time the player has jumping.
    private float lastJumpStart = -0.5f;  //Last time the jump key was pressed
    private float jumpLaunchTime = 0.1f; //Time before first collision check is done

    private bool isCrouch; // Whether or not the player is crouch.
    private float horizontal;
    public static bool canMove = true;
    private Rigidbody2D rb2d;
    private bool isFacingRight = true;  // For determining which way the player is currently facing.
    private float gravityScale;
    private Vector3 Velocity = Vector3.zero;
    private bool wasCrouching = false;

    private bool isJumping;
    private float coyoteTime = 0.1f;
    private float coyoteTimeCounter;

    private float jumpBufferTime = 0.1f;
    private float jumpBufferTimeCounter;

    public static bool canDoubleJump = false;
    public static bool doubleJumpEnable = false;
    private float doubleJumpForce;

    public static bool dashEnable = false;
    private bool canDash = false;
    public static bool isDashing;

    public static bool wallSlideEnable = false;
    private bool isWallSliding;
    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.1f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.35f;
    private Vector2 wallJumpingPower;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb2d = GetComponent<Rigidbody2D>();
        gravityScale = rb2d.gravityScale;
        doubleJumpForce = jumpForce * 0.75f;
        wallJumpingPower = new Vector2(jumpForce * .8f, jumpForce * .8f);
        attackFilter.layerMask = 7;
        canMove = true;
        doubleJumpEnable = false;
        dashEnable = false;
        wallSlideEnable = false;    
    }

    //Update Function is call every running frame of the game
    private void Update()
    {
        //If the player is dashing, do nothing
        if (isDashing)
        {
            return;
        }
        // Clamp y velocity to make max falling speed
        rb2d.velocity = new Vector2(rb2d.velocity.x, Mathf.Clamp(rb2d.velocity.y, -maxFallingSpeed, maxFallingSpeed));
        
        // Horizontal movement
        horizontalMovement = playerInput.actions["Move"];
        if (canMove)
            horizontal = (horizontalMovement.ReadValue<Vector2>().x * runSpeed) * Time.fixedDeltaTime;
        else
            horizontal = 0;

        //Setup keys for the input system of Unity
        dashAction = playerInput.actions["Dash"];
        jumpAction = playerInput.actions["Jump"];
        attackAction = playerInput.actions["Attack"];

        //Set Speed varaible of the animtor to play the runing animation or not
        animator.SetFloat("Speed", Mathf.Abs(horizontal));

        //Set the IsGrounded varaible of the animator to assit playing which animation of the character
        animator.SetBool("IsGrounded", IsGrounded());

        //If the player is touching the ground
        if (IsGrounded())
        {
            //Check if  the time of jump launch is less the current time - the time of the last jump start
            //If yes, reset abilities and stop jumping animation
            if (Time.time - lastJumpStart > jumpLaunchTime)
            {
                rb2d.gravityScale = gravityScale;
                animator.SetBool("IsJumping", false);
                canDash = true;
                canDoubleJump = true;
                isJumping = false;
            }
        }

        //If the player is touching wall, also rest abilities
        if (IsWalled())
        {
            canDash = true;
            canDoubleJump = true;
        }

        //Call others important functions
        Jump();
        WallSlide();
        WallJump();
        Crouch();
        Attack();
        Dash();

        //If the player is not clinging to the wall, call the function Flip()
        if (!isWallJumping)
        {
            Flip();
        }

    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }
        if (!isWallJumping)
        {
            PlayerControl();
        }
    }

    // If the character has a ceiling preventing them from standing up, keep them crouching
    private bool IsStandupable()
        
    {
        bool isStandupable = true;
        if (Physics2D.OverlapCircle(ceilingCheck.position, checkRadius, groundLayer) ||
            Physics2D.OverlapCircle(ceilingCheck.position, checkRadius, wallLayer) ||
            Physics2D.OverlapCircle(ceilingCheck.position, checkRadius, ledgeLayer))
        {
            isStandupable = false;
        }
        return isStandupable;
    }

    // Check if the player is grounded
    private bool IsGrounded()
    {
        bool isGrounded = false;
        if (Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer) ||
            Physics2D.OverlapCircle(groundCheck.position, checkRadius, ledgeLayer))
        {
            isGrounded = true;
        }
        return isGrounded;
    }

    // Check if the player is touching a wall
    private bool IsWalled()
    {
        bool isWalled = false;
        if ((Physics2D.OverlapCircle(wallCheck.position, checkRadius, wallLayer) ||
            Physics2D.OverlapCircle(wallCheck.position, checkRadius, ledgeLayer)) && !IsGrounded() && !isJumping)
        {
            isWalled = true;
        }
        return isWalled;
    }

    //Jump Functions, all of the jumping of the Character process is here
    private void Jump()
    {
        // Coyote time
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Jump Buffering
        if (jumpAction.WasPressedThisFrame())
        {
            rb2d.gravityScale = gravityScale;
            jumpBufferTimeCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferTimeCounter -= Time.deltaTime;
        }

        // If the player should jump...
        if ((coyoteTimeCounter > 0f && jumpBufferTimeCounter > 0f && canMove) || (jumpAction.triggered && canDoubleJump && doubleJumpEnable && canMove))
        {
            audio.GetComponentsInChildren<AudioSource>()[1].Play();
            isJumping = true;
            lastJumpStart = Time.time;
            jumpBufferTimeCounter = 0f;
            jumpTimeCounter = jumpTime;
            animator.SetBool("IsGrounded", false);
            animator.SetBool("IsJumping", true);
            // Add a vertical force to the player.
            rb2d.velocity = new Vector2(rb2d.velocity.x, canDoubleJump ? jumpForce : jumpForce);
            //Double jump if player can
            if (jumpAction.triggered && canDoubleJump && doubleJumpEnable && !isWallSliding && !IsGrounded())
            {
                cloud.SetActive(true);
                cloud.GetComponent<Transform>().position = cloudPosition.position;
                cloud.GetComponent<Animator>().SetTrigger("FadeOut");
                canDoubleJump = !canDoubleJump;
            }
        }

        //if player release jump button, set y velecity to 0 to make player drop down imediately
        if (jumpAction.WasReleasedThisFrame() && rb2d.velocity.y > 0)
        {
            isJumping = false;
            if (!isWallJumping)
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, 0);
                WallSlide();
            }
            rb2d.gravityScale *= 1.5f;
            animator.SetBool("IsJumping", false);
            coyoteTimeCounter = 0f;
        }

        //if velocity of the character in the y axis is lower than 0, meaning the character is falling
        //Therefore set the player to not Jumping
        if (rb2d.velocity.y < 0)
        {
            isJumping = false;
            animator.SetBool("IsJumping", false);            
        }
    }

    private void WallSlide()
    {
        if (!wallSlideEnable)
            return;


        if (IsWalled() && !IsGrounded())
        {
            canDoubleJump = true;
            isWallSliding = true;
            rb2d.velocity = new Vector2(rb2d.velocity.x, Mathf.Clamp(runSpeed * -0.1f, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
        animator.SetBool("IsWalled", isWallSliding);
    }

    private void WallJump()
    {
        if (!wallSlideEnable)
            return;

        if (isWallSliding)
        {
            canDoubleJump = true;
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if (jumpAction.WasPressedThisFrame() && wallJumpingCounter > 0f && canMove)
        {
            animator.SetBool("IsJumping", true);
            if (horizontal * -wallJumpingDirection > 0)
            {
                isWallJumping = true;
                rb2d.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x * .15f, wallJumpingPower.y * 1.3f);
                wallJumpingCounter = 0f;
            }
            else if (horizontal * -wallJumpingDirection < 0 || horizontal == 0f)
            {
                isWallJumping = true;
                rb2d.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x * .8f, wallJumpingPower.y * 1.2f);
                wallJumpingCounter = 0f;
            }

            if (transform.localScale.x != wallJumpingDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    private void Crouch()
    {
        if (!canMove)
            return;

        if (horizontalMovement.ReadValue<Vector2>().y < 0)
        {
            isCrouch = true;
        }
        // If stop crouching, check to see if the character can stand up
        if (horizontalMovement.ReadValue<Vector2>().y >= 0)
        {
            isCrouch = !IsStandupable();
        }
    }

 

    //Attacking Functions
    private void Attack()
    {
        //Check for attacking input and validate if the character can perform the attack
        if (attackAction.WasPressedThisFrame() && IsStandupable() && !isWallSliding && canMove)
        {
            //if the character can perform attack process to the function IEnumerator  Attacking()
            StartCoroutine(Attacking());
        }
    }
    IEnumerator Attacking()
    {
        //Play the attack sound
        audio.GetComponentsInChildren<AudioSource>()[3].Play();
        //Disable movement of the player
        canMove = false;
        //Play the Attacking Animation
        animator.SetBool("IsAttacking", true);
        //Enable the attack hitbox for Calucations
        attackCollider.enabled = true;
        //wait for a split second
        yield return new WaitForSeconds(0.15f);
        //Enable movement of the player
        canMove = true;
        //wait for another split second
        yield return new WaitForSeconds(0.1f);
        //Disable the attack hitbox so that the player don't constantly hiting enemies
        attackCollider.enabled = false;
        //Turn off the Attacking Animation
        animator.SetBool("IsAttacking", false);
    }

    //Dash Function
    private void Dash()
    {
        if (dashAction.WasPressedThisFrame() && IsStandupable() && canDash && canMove && dashEnable)
        {

            isDashing = true;
            StartCoroutine(Dashing());
        }
    }

    IEnumerator Dashing()
    {
        audio.GetComponentsInChildren<AudioSource>()[5].Play();
        canDash = false;
        float originalGravity = rb2d.gravityScale;
        rb2d.gravityScale = 0;
        animator.SetBool("IsDashing", true);
        if (isWallSliding)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
        rb2d.velocity = new Vector2(transform.localScale.x * runSpeed * 0.5f, 0f);
        trail.SetActive(true);

        yield return new WaitForSeconds(0.3f);
        trail.SetActive(false);
        animator.SetBool("IsDashing", false);
        rb2d.gravityScale = originalGravity;
        isDashing = false;
    }

    //This Funcion mostly Controll player's speed
    private void PlayerControl()
    {
        //only control the player if grounded or airControl is turned on
        if (IsGrounded() || airControl)
        {
            // If crouching
            if (isCrouch && IsGrounded())
            {
                if (!wasCrouching)
                {
                    wasCrouching = true;
                    animator.SetBool("IsCrouching", isCrouch);
                }

                // Reduce the speed by the crouchSpeed multiplier
                horizontal *= crouchSpeed;

                // Disable one of the colliders when crouching
                if (crouchDisableCollider != null)
                    crouchDisableCollider.enabled = false;
            }
            else
            {
                // Enable the collider when not crouching
                if (crouchDisableCollider != null)
                    crouchDisableCollider.enabled = true;

                if (wasCrouching)
                {
                    wasCrouching = false;
                    animator.SetBool("IsCrouching", isCrouch);
                }
            }

            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(horizontal * 10f, rb2d.velocity.y);
            // And then smoothing it out and applying it to the character
            rb2d.velocity = Vector3.SmoothDamp(rb2d.velocity, targetVelocity, ref Velocity, movementSmoothing);
            //If the player to hit by an enemy, apply a knockback
            if (Enemy.playerKnockBackDirection != Vector2.zero ||
                FlyingEnemy.playerKnockBackDirection != Vector2.zero ||
                DogAI.playerKnockBackDirection != Vector2.zero)
            {
                audio.GetComponentsInChildren<AudioSource>()[4].Play();
                rb2d.velocity = Vector3.zero;
                rb2d.AddForce(Enemy.playerKnockBackDirection * 200f, ForceMode2D.Impulse);
                rb2d.AddForce(FlyingEnemy.playerKnockBackDirection * 200f, ForceMode2D.Impulse);
                rb2d.AddForce(DogAI.playerKnockBackDirection * 200f, ForceMode2D.Impulse);
            }
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    //Flip the player
    private void Flip()
    {
        // if the input is moving the player right and the player is facing left 
        //otherwise if the input is moving the player left and the player is facing right
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f && !isDashing)
        {
            // Switch the way the player is labelled as facing.
            isFacingRight = !isFacingRight;

            // Multiply the player's x local scale by -1.
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }
    }
}
