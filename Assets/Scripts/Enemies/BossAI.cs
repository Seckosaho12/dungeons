using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    [SerializeField] private float roamChangeDirFloat = 2f;
    [SerializeField] private float attackRange = 0f;
    [SerializeField] private float detectionRadius = 5f; // Detection radius
    [SerializeField] private MonoBehaviour enemyType;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private bool stopMovingWhileAttacking = false;
    [SerializeField] private Animator bossAnimtor;

    public Vector3 basePositionOffset;

    private bool canAttack = true;
    private bool playerDetected = false; // Track if player is detected

    private enum State
    {
        Roaming,
        Chasing,
        Attacking
    }

    private Vector2 roamPosition;
    private float timeRoaming = 0f;

    private State state;
    private EnemyPathfinding enemyPathfinding;

    private void Awake()
    {
        enemyPathfinding = GetComponent<EnemyPathfinding>();
        bossAnimtor = GetComponent<Animator>();
        state = State.Roaming;
    }

    private void Start()
    {
        roamPosition = GetRoamingPosition();
    }

    private void Update()
    {
        // Check if player is within detection radius
        float distanceToPlayer = Vector2.Distance(transform.position + basePositionOffset, PlayerController.Instance.transform.position);

        // Once player is detected, never go back to roaming
        if (!playerDetected && distanceToPlayer < detectionRadius)
        {
            playerDetected = true;
            if (state == State.Roaming)
            {
                state = State.Chasing;
            }
        }

        MovementStateControl();
    }

    private void MovementStateControl()
    {
        switch (state)
        {
            default:
            case State.Roaming:
                Roaming();
                break;

            case State.Chasing:
                ChasingPlayer();
                break;

            case State.Attacking:
                Attacking();
                break;
        }
    }

    private void Roaming()
    {
        timeRoaming += Time.deltaTime;

        enemyPathfinding.MoveTo(roamPosition);

        if (Vector2.Distance(transform.position + basePositionOffset, PlayerController.Instance.transform.position) < attackRange)
        {
            bossAnimtor.SetTrigger("Attack");
            state = State.Attacking;
        }

        if (timeRoaming > roamChangeDirFloat)
        {
            roamPosition = GetRoamingPosition();
        }
    }

    private void ChasingPlayer()
    {
        // Get direction to player
        Vector2 directionToPlayer = (PlayerController.Instance.transform.position - transform.position).normalized;

        // Set movement direction toward player
        enemyPathfinding.MoveTo(directionToPlayer);

        // If close enough to attack
        if (Vector2.Distance(transform.position + basePositionOffset, PlayerController.Instance.transform.position) < attackRange)
        {
            bossAnimtor.SetTrigger("Attack");
            state = State.Attacking;
        }
    }

    private void Attacking()
    {
        if (Vector2.Distance(transform.position + basePositionOffset, PlayerController.Instance.transform.position) > attackRange)
        {
            // Always go back to chasing once player has been detected
            state = State.Chasing;
        }

        if (attackRange != 0 && canAttack)
        {
            canAttack = false;
            if (enemyType != null)
                (enemyType as IEnemy).Attack();

            if (stopMovingWhileAttacking)
            {
                enemyPathfinding.StopMoving();
            }
            else
            {
                // Always move toward player when attacking
                Vector2 directionToPlayer = (PlayerController.Instance.transform.position - transform.position).normalized;
                enemyPathfinding.MoveTo(directionToPlayer);
            }

            StartCoroutine(AttackCooldownRoutine());
        }
    }

    private IEnumerator AttackCooldownRoutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private Vector2 GetRoamingPosition()
    {
        timeRoaming = 0f;
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    public void AttackEvent()
    {
        PlayerController.Instance.playerHealth.TakeDamage(1, transform);
    }

    void OnDrawGizmos()
    {
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + basePositionOffset, attackRange);

        // Draw detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + basePositionOffset, detectionRadius);
    }
}
