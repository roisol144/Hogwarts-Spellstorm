using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public void AcceptOrderByCustomerName(string customerName)
    {
        // TODO: Implement your customer order acceptance logic here
        Debug.Log($"Order accepted for customer: {customerName}");
    }
} 