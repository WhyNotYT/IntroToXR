using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillHitBox : MonoBehaviour
{
    public ParticleSystem particleEffect1;
    public ParticleSystem particleEffect2;
    public float damageInterval = 0.5f;
    public AudioSource drillSound;
    private HashSet<Collider> enemiesInRange = new HashSet<Collider>();
    private bool isDrilling = false;
    private bool isDrillButtonPressed = false;

    private void OnEnable()
    {
        isDrillButtonPressed = true;
        // Only start particles if there are valid enemies in range
        if (HasValidEnemiesInRange())
        {
            PlayParticles();
            if (!isDrilling)
            {
                StartCoroutine(DealDamageRoutine());
            }
        }
    }

    private void OnDisable()
    {
        isDrillButtonPressed = false;
        StopParticles();
        isDrilling = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesInRange.Add(other);
            
            // Only play particles if drill button is being pressed
            if (isDrillButtonPressed)
            {
                PlayParticles();
                if (!isDrilling)
                {
                    StartCoroutine(DealDamageRoutine());
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesInRange.Remove(other);
            if (!HasValidEnemiesInRange())
            {
                StopParticles();
                isDrilling = false;
            }
        }
    }

    private bool HasValidEnemiesInRange()
    {
        // Create a temporary list to store enemies that should be removed
        List<Collider> enemiesToRemove = new List<Collider>();
        
        foreach (var enemy in enemiesInRange)
        {
            // Check if the enemy is null or has been destroyed
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
            {
                enemiesToRemove.Add(enemy);
            }
        }
        
        // Remove invalid enemies from the HashSet
        foreach (var enemy in enemiesToRemove)
        {
            enemiesInRange.Remove(enemy);
        }
        
        // Return true if there are valid enemies remaining
        return enemiesInRange.Count > 0;
    }

    private void PlayParticles()
    {
        if (!particleEffect1.isPlaying) particleEffect1.Play();
        if (!particleEffect2.isPlaying) particleEffect2.Play();
        if (!drillSound.isPlaying) drillSound.Play(); 
    }

    private void StopParticles()
    {
        if (particleEffect1.isPlaying) particleEffect1.Stop();
        if (particleEffect2.isPlaying) particleEffect2.Stop();
        if (drillSound.isPlaying) drillSound.Stop(); 
    }

    private IEnumerator DealDamageRoutine()
    {
        isDrilling = true;
        
        while (isDrillButtonPressed && HasValidEnemiesInRange())
        {
            bool anyEnemyDamaged = false;
            
            foreach (var enemy in new HashSet<Collider>(enemiesInRange))
            {
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    enemy.SendMessage("TakeDamage", SendMessageOptions.DontRequireReceiver);
                    anyEnemyDamaged = true;
                }
            }
            
            if (!anyEnemyDamaged)
            {
                StopParticles();
                break;
            }
            
            yield return new WaitForSeconds(damageInterval);
        }
        
        if (!HasValidEnemiesInRange())
        {
            StopParticles();
        }
        
        isDrilling = false;
    }
}