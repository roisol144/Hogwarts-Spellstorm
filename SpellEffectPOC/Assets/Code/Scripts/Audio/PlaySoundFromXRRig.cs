using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundFromXRRig : MonoBehaviour
{
    [SerializeField]
    private AudioClip audioClipToPlay;

    private GameObject xrRig;

    private void Start()
    {
        xrRig = GameObject.FindGameObjectWithTag("XR Rig");
    }

    public void Play()
    {
        xrRig.GetComponent<AudioSource>().PlayOneShot(audioClipToPlay);
    }

}
