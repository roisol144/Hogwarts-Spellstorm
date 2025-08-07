using UnityEngine;

public class Impact01Projectile : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 15f;
    public float lifetime = 5f;
    private float timer;

    [Header("Impact Settings")]
    public GameObject explosionPrefab; // Optional: assign an explosion effect prefab
    public AudioClip impactSound;      // Optional: assign an impact sound

    private AudioSource audioSource;

    private void Start()
    {
        // Cache the AudioSource if needed
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Move forward in local space
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // Update the lifetime timer and destroy after exceeding lifetime
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[Impact01] Impact01 collided with: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");

        bool hitEnemy = false;

        // Check if we hit an enemy
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                Debug.Log($"[Impact01] Found Enemy component on {collision.gameObject.name}");
                
                // Get the spell information from this projectile
                SpellCasted spellCasted = GetComponent<SpellCasted>();
                string spellIntent = spellCasted?.SpellIntent ?? "unknown";
                
                Debug.Log($"[Impact01] Spell cast was: {spellIntent}");
                
                // Determine enemy type
                string enemyType = GetEnemyType(enemy.gameObject);
                Debug.Log($"[Impact01] Enemy type is: {enemyType}");
                
                // Check if this is the correct spell-enemy combination
                bool isCorrectSpell = IsCorrectSpellForEnemy(spellIntent, enemyType);
                
                if (isCorrectSpell)
                {
                    Debug.Log($"[Impact01] ✅ CORRECT SPELL! {spellIntent} can instantly kill {enemyType}");
                    // Apply special damage (instant kill)
                    enemy.TakeDamage(0f, true);
                    hitEnemy = true;
                    Debug.Log($"[Impact01] Successfully applied instant kill!");
                }
                else
                {
                    Debug.Log($"[Impact01] ❌ WRONG SPELL! {spellIntent} cannot kill {enemyType} - reducing points and applying regular damage");
                    
                    // Show hint message to the player
                    string correctSpell = GetCorrectSpellForEnemy(enemyType);
                    MagicalDebugUI.ShowHint($"Try using {correctSpell}...");
                    
                    // Play audio hint for the enemy type
                    EnemySpellHintAudio.PlayHintForEnemy(enemyType);
                    
                    // Reduce points for using wrong spell (penalty: -5 points)
                    ScoreManager.NotifyPenalty(5, $"Wrong spell: {spellIntent} used on {enemyType}");
                    Debug.Log($"[Impact01] Applied penalty for wrong spell usage");
                    
                    // Apply regular damage instead of instant kill
                    enemy.TakeDamage(0f, false);
                    hitEnemy = true;
                    Debug.Log($"[Impact01] Applied regular damage instead of instant kill");
                }
            }
            else
            {
                Debug.LogError($"[Impact01] GameObject {collision.gameObject.name} has 'Enemy' tag but no Enemy component!");
            }
        }
        else
        {
            Debug.Log($"[Impact01] Hit non-enemy object: {collision.gameObject.name} (Tag: {collision.gameObject.tag})");
        }

        // Only show the Impact01's explosion if we did NOT hit an enemy
        if (!hitEnemy && explosionPrefab != null)
        {
            Debug.Log($"[Impact01] Spawning explosion effect");
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        if (impactSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(impactSound);
        }

        Debug.Log($"[Impact01] Destroying Impact01 projectile");
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Determines the enemy type by checking for specific enemy components
    /// </summary>
    private string GetEnemyType(GameObject enemyObject)
    {
        if (enemyObject.GetComponent<Dementor>() != null)
        {
            return "dementor";
        }
        else if (enemyObject.GetComponent<Basilisk>() != null)
        {
            return "basilisk";
        }
        else if (enemyObject.GetComponent<Troll>() != null)
        {
            return "troll";
        }
        else
        {
            return "unknown";
        }
    }
    
    /// <summary>
    /// Checks if the spell is effective against the specific enemy type
    /// </summary>
    private bool IsCorrectSpellForEnemy(string spellIntent, string enemyType)
    {
        switch (spellIntent)
        {
            case "cast_expecto_patronum":
                return enemyType == "dementor";
            case "cast_bombardo":
                return enemyType == "basilisk";
            case "cast_stupefy":
                return enemyType == "troll";
            default:
                return false; // Unknown spells are never correct
        }
    }
    
    /// <summary>
    /// Gets the correct spell name for the specific enemy type
    /// </summary>
    private string GetCorrectSpellForEnemy(string enemyType)
    {
        switch (enemyType)
        {
            case "dementor":
                return "Expecto Patronum";
            case "basilisk":
                return "Bombardo";
            case "troll":
                return "Stupefy";
            default:
                return "Unknown Spell";
        }
    }
} 