[System.Serializable]
public class WitResponse
{
    public Intent[] intents;
    public bool is_final;
}

[System.Serializable]
public class Intent
{
    public string name;
    public float confidence;
}
