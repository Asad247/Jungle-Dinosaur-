using System.Collections;
using UnityEngine;

namespace IE.RSB
{
    public class DinosaurHit : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField] private Animator m_animator;
        [SerializeField] private string m_intParameterName = "Idle";
        [SerializeField] private int m_deathValue = -1;

        [Header("Audio")]
        [SerializeField] private AudioSource m_audioSource;
        [SerializeField] private AudioClip m_deathSound;
        [SerializeField] private float m_volume = 1.0f;

        private Rigidbody[] m_hitboxBodies;
        private bool m_isDead = false;
        [SerializeField] private GameObject lcpanle;

        void Awake()
        {
            // Auto-assign components if left empty in Inspector
            if (m_animator == null) m_animator = GetComponent<Animator>();
            if (m_audioSource == null) m_audioSource = GetComponent<AudioSource>();

            // Get all child rigidbodies to act as bullet targets
            m_hitboxBodies = GetComponentsInChildren<Rigidbody>();
        }

        private void OnEnable() => SniperAndBallisticsSystem.EAnyHit += OnAnyHit;
        private void OnDisable() => SniperAndBallisticsSystem.EAnyHit -= OnAnyHit;

        private void OnAnyHit(BulletPoint point)
        {
            if (m_isDead) return;

            GetComponent<DinoAI>().enabled = false;
            // Check if the bullet's hit transform matches any child rigidbody
            for (int i = 0; i < m_hitboxBodies.Length; i++)
            {
                if (point.m_hitTransform == m_hitboxBodies[i].transform)
                {
                    KillDinosaur();
                    break;
                }
            }
        }

        private void KillDinosaur()
        {
            m_isDead = true;

            // Play the death sound independently
            if (m_audioSource != null && m_deathSound != null)
            {
                m_audioSource.PlayOneShot(m_deathSound, m_volume);
            }

            // Trigger the death animation
            if (m_animator != null)
            {
                // Forces the Idle parameter to -1 to trigger the transition
                m_animator.SetInteger(m_intParameterName, m_deathValue);
            }
            StartCoroutine(wait2secs());
            Debug.Log("Dino Killed Independently: Sound and Animation triggered.");
        }

        IEnumerator wait2secs()
        {
            yield return new WaitForSeconds(2f);
            lcpanle
            .SetActive(true);
        }
    }
}