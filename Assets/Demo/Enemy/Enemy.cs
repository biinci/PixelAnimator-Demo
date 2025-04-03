using System.Collections;
using System.Collections.Generic;
using binc.PixelAnimator;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public PixelAnimation idle;
    public PixelAnimator animator;
    
    // Start is called before the first frame update
    void Start()
    {
        animator.Play(idle);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
