using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    [Networked] private TickTimer life { get; set; }
    
    public void Init()
    {
        //Instead of storing the current remaining time, it stores the end-time in ticks.
        //This means the timer does not need to be sync'ed on every tick but just once, when it is created.
        life = TickTimer.CreateFromSeconds(Runner, 5.0f);
        //The timer should be set before the object is spawned, and because Spawned() is called only after a
        //local instance has been created, it should not be used to initialize network state.
    }
    
    public override void FixedUpdateNetwork()
    {
        if(life.Expired(Runner))
            Runner.Despawn(Object);
        else
            transform.position += 5 * transform.forward * Runner.DeltaTime;
    }
}
