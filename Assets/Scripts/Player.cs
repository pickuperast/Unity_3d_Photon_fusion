using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using TMPro;

namespace root {
	public class Player : NetworkBehaviour {

		private float _currentSpeed = 3f;
		public float MS => _currentSpeed;
		
		[Networked]
		private Vector3 moveDirection { get; set; }

		[Networked]
		private Vector3 aimDirection { get; set; }
		[Header("Movement Settings")]
		public float RotationSpeed = 3;
		public float JumpForce = 2.5f;
		
		[SerializeField] private Animator _anim;
		
		[SerializeField] private Ball _prefabBall;
		[SerializeField] private PhysxBall _prefabPhysxBall;
		[Networked] private TickTimer delay { get; set; }
		[Networked(OnChanged = nameof(OnBallSpawned))]
		public NetworkBool spawned { get; set; }
		public static void OnBallSpawned(Changed<Player> changed)
		{
			//changed.Behaviour.material.color = Color.white;
		}
		private NetworkCharacterController _cc;
		private Vector3 _forward;

		private void Awake()
		{
			_cc = GetComponent<NetworkCharacterController>();
			if (_anim == null)
				_anim = GetComponent<NetworkMecanimAnimator>().Animator;
			_forward = transform.forward;
		}

		
		//As the RPC call is the actual networked message, there is no need to extend the input struct.
		//Also, as RPCs are not tick aligned anyways, there is no need to use Fusions input handling,
		//so open Player.cs and add:
		private void Update()
		{
			//Note the check for Object.HasInputAuthority - this is because this code runs on all clients,
			//but only the client that controls this player should call the RPC.
			if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.R))
			{
				RPC_SendMessage("Hey Mate!");
			}
			
			
		}
		private TextMeshProUGUI _messages;

		[Rpc(RpcSources.InputAuthority, RpcTargets.All)]
		public void RPC_SendMessage(string message, RpcInfo info = default)
		{
			if (_messages == null)
				_messages = FindObjectOfType<TextMeshProUGUI>();
			if (info.Source == Runner.Simulation.LocalPlayer)
				message = $"You said: {message}\n";
			else
				message = $"Some other player said: {message}\n";
			_messages.text += message;
		}

		public override void FixedUpdateNetwork()
		{
			if (GetInput(out NetworkInputData data)) {
				data.direction.Normalize();
				//Debug.Log($"data.direction.Normalize(): {data.direction}; _speed: {_cc.Velocity.magnitude / _currentSpeed}");
				_cc.Move(data.direction);//_currentSpeed * Runner.DeltaTime);

				if (data.direction.sqrMagnitude > 0)
					_forward = data.direction;

				if (delay.ExpiredOrNotRunning(Runner))
				{
					if ((data.buttons & NetworkInputData.MOUSEBUTTON1) != 0)
					{
						delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
						Runner.Spawn(_prefabBall,
							transform.position+_forward,
							Quaternion.LookRotation(_forward),
							Object.InputAuthority,
							(runner, o) =>
							{
								// Initialize the Ball before synchronizing it
								o.GetComponent<Ball>().Init();
							});
						spawned = !spawned;
					}
					else if ((data.buttons & NetworkInputData.MOUSEBUTTON2) != 0)
					{
						delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
						Runner.Spawn(_prefabPhysxBall,
							transform.position+_forward,
							Quaternion.LookRotation(_forward),
							Object.InputAuthority,
							(runner, o) =>
							{
								o.GetComponent<PhysxBall>().Init( 10*_forward );
							});
						spawned = !spawned;
					}
				}
				SetDirections(data.direction, data.direction);
			}
		}
		
		public void SetDirections(Vector3 moveDirection, Vector3 aimDirection)
		{
			this.moveDirection = moveDirection;
			this.aimDirection = aimDirection;
		}
		/// <summary>
		/// Render is the Fusion equivalent of Unity's Update() and unlike FixedUpdateNetwork which is very different from FixedUpdate,
		/// Render is in fact exactly the same. It even uses the same Time.deltaTime time steps. The purpose of Render is that
		/// it is always called *after* FixedUpdateNetwork - so to be safe you should use Render over Update if you're on a
		/// SimulationBehaviour.
		///
		/// Here, we use Render to update visual aspects of the Tank that does not involve changing of networked properties.
		/// </summary>
		public override void Render()
		{
			// Add a little visual-only movement to the mesh
			SetMeshOrientation();
		}
		private void SetMeshOrientation()
		{
			if (moveDirection.magnitude > 0.1f)
				_cc.transform.forward = Vector3.Lerp(_cc.transform.forward, moveDirection, Time.deltaTime * 20f);
		}
	}
}

