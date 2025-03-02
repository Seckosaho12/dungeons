using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : Singleton<PlayerController>
{
    public bool FacingLeft { get { return facingLeft; } set { facingLeft = value; } }


    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float dashSpeed = 4f;
    [SerializeField] private TrailRenderer myTrailRenderer;
    [SerializeField] private Transform weaponCollider;
    [SerializeField] private AudioSource step;

    private PlayerControls playerControls;
    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator myAnimator;
    private SpriteRenderer mySpriteRender;
    private Knockback knockback;
    Vector3 lastMousePosition;
    public Vector3 currentFalsePosition;
    bool leftFlap;
    bool rightFlap;

    private bool facingLeft = false;
    private bool isDashing = false;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        mySpriteRender = GetComponent<SpriteRenderer>();
        knockback = GetComponent<Knockback>();
    }

    private void Start()
    {
        playerControls.Combat.Dash.performed += _ => Dash();

        dashSpeed = moveSpeed;
    }

    private void OnEnable()
    {
        playerControls = new PlayerControls();
        playerControls.Enable();
        playerControls.Turn.TurnLeft.performed += LeftFlapDetect;
        playerControls.Turn.TurnLeft.canceled += LeftFlapDetect;
        playerControls.Turn.TurnRight.performed += RightFlapDetect;
        playerControls.Turn.TurnRight.canceled += RightFlapDetect;
    }
    private void OnDisable()
    {
        playerControls = new PlayerControls();
        playerControls.Disable();
        playerControls.Turn.TurnLeft.performed -= LeftFlapDetect;
        playerControls.Turn.TurnLeft.canceled -= LeftFlapDetect;
        playerControls.Turn.TurnRight.performed -= RightFlapDetect;
        playerControls.Turn.TurnRight.canceled -= RightFlapDetect;

    }

    private void LeftFlapDetect(InputAction.CallbackContext ctx)
    {
        leftFlap = ctx.ReadValueAsButton();
        Debug.Log("Triggered");
    }
    private void RightFlapDetect(InputAction.CallbackContext ctx)
    {
        rightFlap = ctx.ReadValueAsButton();
    }



    private void Update()
    {
        PlayerInput();
    }
    private void FixedUpdate()
    {
        AdjustPlayerFacingDirection();
        Move();
    }

    public Transform GetWeaponCollider()
    {
        return weaponCollider;
    }

    private void PlayerInput()
    {
        movement = playerControls.Movement.Move.ReadValue<Vector2>();

        myAnimator.SetFloat("moveX", movement.x);
        myAnimator.SetFloat("moveY", movement.y);
    }

    private void Move() {
        if (knockback.GettingKnockedBack) { return; }

        rb.MovePosition(rb.position + movement * (moveSpeed * Time.fixedDeltaTime));
    }

    public void PlayStep()
    {
        step.pitch = Random.Range(0.8f, 1.2f);
        step.Play();
    }

    private void AdjustPlayerFacingDirection()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(transform.position);
        Vector3 newMousePos = currentFalsePosition;
        if (lastMousePosition == mousePos)
        {
            Debug.Log("in");
            newMousePos = AdjustTurn(newMousePos);
        }
        else
        {
            newMousePos = mousePos;
        }
        ApplyDirectionChanges(newMousePos, playerScreenPoint);
        currentFalsePosition = newMousePos;
        lastMousePosition = mousePos;
    }

    private void ApplyDirectionChanges(Vector3 newMousePos, Vector3 playerScreenPoint)
    {
        if (newMousePos.x < playerScreenPoint.x)
        {
            mySpriteRender.flipX = true;
            facingLeft = true;
        }
        else
        {
            mySpriteRender.flipX = false;
            facingLeft = false;
        }
    }


    private Vector3 AdjustTurn(Vector3 newMousePos)
    {
        Debug.Log(leftFlap);
        if (leftFlap)
        {
            newMousePos.x = -100000;
        }
        else if (rightFlap)
        {
            newMousePos.x = 100000;
        }
        return newMousePos;
    }
    private void Dash()
    {
        if (!isDashing)
        {
            isDashing = true;
            moveSpeed *= dashSpeed;
            myTrailRenderer.emitting = true;
            AudioManager.instance.PlayDashSFX();
            StartCoroutine(EndDashRoutine());
        }
    }

    private IEnumerator EndDashRoutine()
    {
        float dashTime = .2f;
        float dashCD = .25f;
        yield return new WaitForSeconds(dashTime);
        moveSpeed = dashSpeed;
        myTrailRenderer.emitting = false;
        yield return new WaitForSeconds(dashCD);
        isDashing = false;
    }

   }
