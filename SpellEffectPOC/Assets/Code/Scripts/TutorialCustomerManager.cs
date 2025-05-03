using UnityEngine;

public class TutorialCustomerManager : MonoBehaviour
{
    public void AcceptOrderByCustomerName(string customerName)
    {
        // TODO: Implement your tutorial-specific customer order acceptance logic here
        Debug.Log($"Tutorial: Order accepted for customer: {customerName}");
    }
} 