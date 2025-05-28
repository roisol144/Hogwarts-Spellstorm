using UnityEngine;

public class SpellCasted : MonoBehaviour
{
    [Header("Spell Information")]
    [SerializeField] private string spellName;
    [SerializeField] private string spellIntent;
    [SerializeField] private float castTime;

    public string SpellName => spellName;
    public string SpellIntent => spellIntent;
    public float CastTime => castTime;

    public void Initialize(string name, string intent)
    {
        spellName = name;
        spellIntent = intent;
        castTime = Time.time;
        
        Debug.Log($"[SpellCasted] Initialized spell: {spellName} (Intent: {spellIntent}) at time {castTime}");
    }
} 