using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace root {
    public class ThirdPersonAnimation : MonoBehaviour
    {
        private Player _player;
        private NetworkCharacterController _cc;
        private Animator _anim;

        private void Awake() {
            _anim = GetComponent<Animator>();
            _player = GetComponentInParent(typeof(Player)) as Player;
            _cc = GetComponentInParent(typeof(NetworkCharacterController)) as NetworkCharacterController;
        }

        void FixedUpdate() => _anim.SetFloat("Speed", _cc.Velocity.magnitude / _player.MS);
    }
}
