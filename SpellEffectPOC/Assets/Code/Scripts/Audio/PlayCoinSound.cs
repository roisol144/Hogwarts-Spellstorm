using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayCoinSound : MonoBehaviour
{

    [SerializeField] AudioSource source; 
    [SerializeField] AudioClip[] coinSFX;

    private Rigidbody rb;
    private float prob = 0.45f; 
    bool playedLastTick = false; 
   


    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {


           
      
        if (Random.Range(0f,1f) >= prob)
        {
            if (!source.isPlaying) {
                source.PlayOneShot(coinSFX[Random.Range(0, coinSFX.Length)]);
            }
            if(prob == 0.5f)
            {
                prob = 0.95f;
            }
            else {
                prob -= 0.15f;
            }
        }
        else {
            prob = 0.45f;
        }
       



    }
}
