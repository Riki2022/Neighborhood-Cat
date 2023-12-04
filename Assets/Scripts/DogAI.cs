using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class DogAI : MonoBehaviour
{
    public static Vector2 playerKnockBackDirection;

    public int maxHealth = 50;
    public float detectRange = 10f;
    public float attackRange = 1f;
    public float speed = 1f;

    private float distanceFromPlayer;
    private float enragedSpeed;
    private Animator animator;
    private UnityEngine.Transform player;
    private bool isFlipped = false;
    private Rigidbody2D rb;
    private Vector2 enemyKnockBackDirection;
    private int currentHealth;
    private bool isDeath = false;
    private bool isActive;

    // Start is called before the first frame update
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        enragedSpeed = speed * 1.5f;
        isActive = true;
    }

    private void Update()
    {
        if (currentHealth < maxHealth / 2)
        {
            speed = enragedSpeed;
        }

        distanceFromPlayer = Vector2.Distance(player.position, rb.position);

        if (!isDeath)
        {
            if (distanceFromPlayer <= attackRange && isActive)
            {
                animator.SetBool("IsWalking", false);
                animator.SetBool("IsAttacking", true);                
                Vector2 target = new Vector2(player.position.x, rb.position.y);
                this.transform.position = Vector2.MoveTowards(rb.position, target, speed/2 * Time.fixedDeltaTime);
            }
            else
            {
                animator.SetBool("IsAttacking", false);
            }

            if (distanceFromPlayer >= attackRange && distanceFromPlayer <= detectRange && isActive)
            {
                LookAtPlayer();
                animator.SetBool("IsWalking", true);
                Vector2 target = new Vector2(player.position.x, rb.position.y);
                this.transform.position = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
            }
            else
            {
                animator.SetBool("IsWalking", false);
            }
        }
    }


    public void TakeDamage(int damage, Collider2D player)
    {
        currentHealth -= damage;
        enemyKnockBackDirection = (this.transform.position - player.transform.position).normalized;
        if (currentHealth <= 0)
        {
            StartCoroutine(Deadth());
            isDeath = true;
        }
        else
        {
            StartCoroutine(TakingDamage());
        }
    }

    //Trigger when either player hit enemy or enemy hit the player
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isActive)
            return;

        if (!isDeath)
        {
            if (other.gameObject.layer == 10)
            {
                TakeDamage(CharacterController2D.attackDamage, other);
            }

            if (other.gameObject.layer == 3 && !CharacterController2D.isDashing)
            {
                StartCoroutine(PlayerTakeDamage(other));
            }
        }
    }


    //Trigger after player got knockback
    private void OnTriggerExit2D(Collider2D other)
    {
        //Player layer is 3
        if (other.gameObject.layer == 3)
        {
            playerKnockBackDirection = Vector2.zero;
        }
    }

    public void LookAtPlayer()
    {
        Vector3 flipped = transform.localScale;
        flipped.z *= -1f;

        if (transform.position.x < player.position.x && isFlipped)
        {
            transform.localScale = flipped;
            transform.Rotate(0f, 180f, 0f);
            isFlipped = false;
        }
        else if (transform.position.x > player.position.x && !isFlipped)
        {
            transform.localScale = flipped;
            transform.Rotate(0f, 180f, 0f);
            isFlipped = true;
        }
    }

    private IEnumerator TakingDamage()
    {
        isActive = false;
        animator.SetBool("TakeDamage", true);
        rb.AddForce(enemyKnockBackDirection * 15f, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.2f);
        animator.SetBool("TakeDamage", false);
        isActive = true;
    }

    private IEnumerator Deadth()
    {
        animator.SetBool("IsDeath", true);
        yield return new WaitForSeconds(1.5f);
        //Destroy(this.gameObject);
    }

    private IEnumerator PlayerTakeDamage(Collider2D player)
    {
        isActive = false;
        playerKnockBackDirection = (player.transform.position - this.transform.position).normalized;
        player.GetComponent<Animator>().SetBool("IsTakingDamage", true);
        CharacterController2D.canMove = false;
        yield return new WaitForSeconds(0.1f);
        CharacterController2D.canMove = true;
        yield return new WaitForSeconds(0.2f);
        player.GetComponent<Animator>().SetBool("IsTakingDamage", false);
        isActive = true;
    }


}
