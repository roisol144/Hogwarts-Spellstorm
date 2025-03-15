using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public GameObject accioEffectPrefab;
    public GameObject bombardoEffectPrefab;
    public GameObject depulsoEffectPrefab;
    public GameObject expectoPatronumEffectPrefab;
    public GameObject stupefyEffectPrefab;

    public void TriggerEffect(string spellName, Vector3 position)
    {
        GameObject effectPrefab = null;

        switch (spellName)
        {
            case "cast_accio":
                effectPrefab = accioEffectPrefab;
                break;
            case "cast_bombardo":
                effectPrefab = bombardoEffectPrefab;
                break;
            case "cast_depulso":
                effectPrefab = depulsoEffectPrefab;
                break;
            case "cast_expecto_patronum":
                effectPrefab = expectoPatronumEffectPrefab;
                break;
            case "cast_stupefy":
                effectPrefab = stupefyEffectPrefab;
                break;
            default:
                Debug.LogWarning($"No effect found for spell: {spellName}");
                return;
        }

        if (effectPrefab != null)
        {
            Instantiate(effectPrefab, position, Quaternion.identity);
        }
    }
}
