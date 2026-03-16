using UnityEngine;
using DG.Tweening;
using IE.RSB; // Required for BulletPoint and SniperAndBallisticsSystem

public class DinoAI : MonoBehaviour
{
    private enum DinoState { Patrolling, Idle, Chasing, Attacking, Dead }
    private DinoState _currentState = DinoState.Patrolling;

    [Header("Movement Settings")]
    public Transform[] waypoints;
    public float moveSpeed = 3f;
    public float runSpeed = 6f;
    public float idleWaitTime = 2f;

    [Header("Animator Parameters")]
    public Animator animator;
    public string moveParam = "Move";   // 0=Idle, 1=Walk, 2=Run
    public string idleParam = "Idle";   // 0, 1, or 2. -1 for Death.
    public string attackParam = "Attack";

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip movingSound, idleSound, runSound, attackSound, deathSound;

    private int _currentIndex = 0;
    private Transform _playerTarget;
    private Rigidbody[] _hitboxBodies;

    void Awake()
    {
        _hitboxBodies = GetComponentsInChildren<Rigidbody>();
    }

    void Start()
    {
        if (waypoints != null && waypoints.Length > 0)
            EnterPatrolState();
    }

    private void OnEnable()
    {
        SniperAndBallisticsSystem.EAnyHit += OnAnyHit;
    }

    private void OnAnyHit(BulletPoint point)
    {
        if (_currentState == DinoState.Dead) return;

        for (int i = 0; i < _hitboxBodies.Length; i++)
        {
            if (point.m_hitTransform == _hitboxBodies[i].transform)
            {
                KillDinosaur();
                break;
            }
        }
    }

    public void KillDinosaur()
    {
        if (_currentState == DinoState.Dead) return;

        _currentState = DinoState.Dead;

        // Stop all active DOTween movements/loops and current looping sounds
        transform.DOKill();
        DOTween.Kill(this);
        StopSound();

        // Trigger Death Animation
        if (animator) animator.SetInteger(idleParam, -1);

        // Play Death Sound independently of the looping audio logic
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        Debug.Log("Dino Killed: Sound and Animation triggered.");

        // Disable the AI script so it stops thinking
        this.enabled = false;
    }

    private void EnterPatrolState()
    {
        if (_currentState == DinoState.Dead) return;

        _currentState = DinoState.Patrolling;
        if (animator) animator.SetInteger(moveParam, 1);

        PlaySound(movingSound, true);

        Vector3 target = waypoints[_currentIndex].position;
        float duration = Vector3.Distance(transform.position, target) / moveSpeed;

        transform.LookAt(target);
        transform.DOMove(target, duration)
            .SetEase(Ease.Linear)
            .OnComplete(EnterIdleState)
            .SetTarget(this);
    }

    private void EnterIdleState()
    {
        if (_currentState == DinoState.Chasing || _currentState == DinoState.Dead) return;

        StopSound();
        _currentState = DinoState.Idle;

        if (animator)
        {
            animator.SetInteger(moveParam, 0);
            animator.SetInteger(idleParam, Random.Range(0, 3));
        }

        PlaySound(idleSound, false);

        DOVirtual.DelayedCall(idleWaitTime, () =>
        {
            if (_currentState == DinoState.Idle)
            {
                _currentIndex = (_currentIndex + 1) % waypoints.Length;
                EnterPatrolState();
            }
        }).SetTarget(this);
    }

    public void TriggerAggro(Transform player)
    {
        if (_currentState == DinoState.Chasing || _currentState == DinoState.Attacking || _currentState == DinoState.Dead) return;

        _playerTarget = player;
        _currentState = DinoState.Chasing;

        StopSound();
        transform.DOKill();

        if (animator) animator.SetInteger(moveParam, 2);
        PlaySound(runSound, true);

        ChaseLoop();
    }

    private void ChaseLoop()
    {
        if (_currentState != DinoState.Chasing || _playerTarget == null) return;

        transform.LookAt(_playerTarget.position);
        transform.position = Vector3.MoveTowards(transform.position, _playerTarget.position, runSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _playerTarget.position) < 1.5f)
        {
            EnterAttackState();
        }
        else
        {
            DOVirtual.DelayedCall(0.02f, ChaseLoop).SetTarget(this);
        }
    }

    private void EnterAttackState()
    {
        if (_currentState == DinoState.Dead) return;

        _currentState = DinoState.Attacking;
        StopSound();

        if (animator)
        {
            animator.SetInteger(moveParam, 0);
            animator.SetTrigger(attackParam);
        }

        PlaySound(attackSound, false);
        Debug.Log("Dino attacked");
    }

    private void PlaySound(AudioClip clip, bool loop)
    {
        if (audioSource == null || clip == null) return;
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.Play();
    }

    private void StopSound()
    {
        if (audioSource != null) audioSource.Stop();
    }

    private void OnDisable()
    {
        SniperAndBallisticsSystem.EAnyHit -= OnAnyHit;
        transform.DOKill();
        DOTween.Kill(this);
        StopSound();
    }
}