using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace root {
    public class ThirdPersonAnimation : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Player _player;
        private float maxSpeed = 5f;

        void Update() => animator.SetFloat("speed", _player.Speed);
    }
}
