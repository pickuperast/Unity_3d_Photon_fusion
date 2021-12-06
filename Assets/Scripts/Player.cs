using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using TMPro;

namespace root {
	public class Player : NetworkBehaviour
	{
		
		[Header("Movement Settings")]
		public float _currentSpeed = 3;
		public float RotationSpeed = 3;
		public float JumpForce = 2.5f;

		[Networked] private float _speed { get; set; }
		public float Speed => _speed;
		
		private Animator _anim;
		
		[SerializeField] private Ball _prefabBall;
		[SerializeField] private PhysxBall _prefabPhysxBall;
		[Networked] private TickTimer delay { get; set; }
		[Networked(OnChanged = nameof(OnBallSpawned))]
		public NetworkBool spawned { get; set; }
		private Material _material;
		Material material
		{
			get
			{
				if(_material==null)
					_material = GetComponentInChildren<MeshRenderer>().material;
				return _material;
			}
		}
		public override void Render()
		{
			material.color = Color.Lerp(material.color, Color.blue, Time.deltaTime );
		}
		public static void OnBallSpawned(Changed<Player> changed)
		{
			changed.Behaviour.material.color = Color.white;
		}
		private NetworkCharacterController _cc;
		private Vector3 _forward;

		private void Awake()
		{
			_cc = GetComponent<NetworkCharacterController>();
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
				Debug.Log($"data.direction.Normalize(): {data.direction}; _speed: {_speed}");
				_cc.Move(_currentSpeed*data.direction*Runner.DeltaTime);
				_speed = _cc.Velocity.magnitude / _currentSpeed;

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
			}
		}
	}
}

