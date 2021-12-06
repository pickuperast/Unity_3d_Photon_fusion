using System;
using UnityEngine;
using Fusion;

public class PhysxBall : NetworkBehaviour
{
    [Networked] private TickTimer life { get; set; }
    [Networked] private float speed { get; set; }
    
    public void Init(Vector3 forward)
    {
        life = TickTimer.CreateFromSeconds(Runner, 5.0f);
        GetComponent<Rigidbody>().velocity = forward;
    }

    public override void FixedUpdateNetwork()
    {
        if(life.Expired(Runner))
            Runner.Despawn(Object);
    }

    private void FixedUpdate() {
        speed = GetComponent<Rigidbody>().velocity.magnitude / 5f;
        GetComponent<NetworkMecanimAnimator>().Animator.SetFloat("Speed", speed);
    }
}
