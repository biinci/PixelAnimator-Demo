using UnityEngine;
using binc.PixelAnimator;

public class PlayerController : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private PixelAnimator animator;
    [SerializeField] private PixelAnimation idle;
    [SerializeField] private PixelAnimation walk;
    [SerializeField] private PixelAnimation run;
    [SerializeField] private PixelAnimation jump;
    [SerializeField] private PixelAnimation fall;
    [SerializeField] private PixelAnimation attackOne;
    [SerializeField] private PixelAnimation attackTwo;
    
    [Header("Movement")]
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GroundCheck groundCheck;
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem dustParticle;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.3f;
    
    
    // State variables
    private bool isFalling;
    private float horizontalInput;
    public bool IsAttackingOne { get; private set; }
    public bool IsAttackingTwo { get; private set; }
    
    private bool IsGrounded => groundCheck.IsGrounded;
    private bool IsInAir => !IsGrounded || animator.PlayingAnimation == jump || isFalling;
    private bool IsAttacking => IsAttackingOne || IsAttackingTwo;

    private void Start()
    {
        animator.Play(idle);        
    }

    private void Update()
    {
        HandleInput();
        UpdateAnimationState();
    }

    private void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        
        // Jump input
        if (!(IsAttackingOne && IsAttackingTwo) && IsGrounded)
        {
            if (Input.GetKeyDown(KeyCode.Space) && animator.PlayingAnimation != jump)
            {
                animator.Play(jump);
            }
        }
        
        // Attack input - Only check when grounded and not in mid-air
        if (!IsInAir)
        {
            HandleAttackInput();
        }
    }
    
    private void HandleAttackInput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (animator.PlayingAnimation != attackOne && !IsAttackingTwo)
            {
                IsAttackingOne = true;
                animator.Play(attackOne);
            }
            else if (animator.PlayingAnimation == attackOne && IsAttackingOne)
            {
                IsAttackingTwo = true;
            }
        }
        
        // Handle attack animations completion
        if (IsAttackingOne && animator.PlayingAnimation == attackOne && !animator.IsPlaying)
        {
            IsAttackingOne = false;
            
            if (IsAttackingTwo)
            {
                animator.Play(attackTwo);
            }
            else
            {
                animator.Play(idle);
            }
        }
        
        if (IsAttackingTwo && animator.PlayingAnimation == attackTwo && !animator.IsPlaying)
        {
            IsAttackingTwo = false;
            animator.Play(idle);
        }
    }

    private void UpdateAnimationState()
    {
        // Check falling state
        if (!IsGrounded && animator.PlayingAnimation != fall && body.velocityY < 0f && !IsAttacking)
        {
            isFalling = true;
            animator.Play(fall);
            return;
        }
        
        // Don't change animations if attacking or in air
        if (IsAttacking || IsInAir) return;
        
        // Handle movement animations
        UpdateMovementAnimation();
    }
    
    private void UpdateMovementAnimation()
    {
        // Idle state
        if (Mathf.Abs(horizontalInput) < Mathf.Epsilon && animator.PlayingAnimation != idle)
        {
            animator.Play(idle);
            return;
        }
        
        // Movement state
        if (!(Mathf.Abs(horizontalInput) > 0)) return;
        spriteRenderer.flipX = horizontalInput < 0;
            
        var isRunning = Input.GetKey(KeyCode.LeftShift);
        switch (isRunning)
        {
            case true when animator.PlayingAnimation != run:
                animator.Play(run);
                break;
            case false when animator.PlayingAnimation != walk && animator.PlayingAnimation != run:
                animator.Play(walk);
                break;
        }
    }

    #region Public Methods
    public void Attack(Collider2D other, float amount)
    {
        if (other.gameObject.layer != 6) return;
        
        var enemyBody = other.transform.root.GetComponent<Rigidbody2D>();
        float direction = spriteRenderer.flipX ? -0.5f : 0.5f;
        
        enemyBody.velocityX += direction;
        enemyBody.velocityY += 0.3f;
    }
    
    public void FallParticle(Collision2D other, ParticleSystem.MinMaxCurve x, ParticleSystem.MinMaxCurve y)
    {
        isFalling = false;
        PlayParticleByVelocity(x, y);
    }
    
    public void PlayParticleByVelocity(ParticleSystem.MinMaxCurve x, ParticleSystem.MinMaxCurve y)
    {
        var reverseDirection = spriteRenderer.flipX ? 1 : -1;
        var velocityModule = dustParticle.velocityOverLifetime;
        
        velocityModule.x = new ParticleSystem.MinMaxCurve(x.curveMultiplier * reverseDirection, x.curve);
        velocityModule.y = y;
        dustParticle.Play();
    }
    
    public void Jump(float amount) 
    {
        body.velocityY += amount;
    }
    
    public void AddForceToPlayer(float amount)
    {
        var input = Input.GetAxisRaw("Horizontal");
        body.AddForceX(input * amount, ForceMode2D.Impulse); 
    }

    public void AddXVelocityToPlayer(float amount)
    {
        var input = Input.GetAxisRaw("Horizontal");
        body.velocityX += amount * input;
    }
    
    public void Enter(Collision2D other)
    {
        Debug.Log("Enter!");
    }
    
    public void WalkSound()
    {
        if (footstepSounds == null || footstepSounds.Length == 0 || audioSource == null)
            return;
            
        // Play a random footstep sound from the array
        int randomIndex = Random.Range(0, footstepSounds.Length);
        AudioClip footstepSound = footstepSounds[randomIndex];
        
        // Adjust volume based on movement type
        float volumeScale = Input.GetKey(KeyCode.LeftShift) ? 1.0f : 0.7f;
        
        audioSource.pitch = Random.Range(0.95f, 1.05f); // Add slight pitch variation
        audioSource.PlayOneShot(footstepSound, volumeScale);
    }
    #endregion
}