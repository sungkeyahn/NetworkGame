using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
public class PlayerController : MonoBehaviourPun, IPunObservable
{
	int _mask = (1 << (int)Define.Layer.Ground) | (1 << (int)Define.Layer.Monster);

	#region Photon
	PhotonView pv;
    protected Vector3 currPos;
    protected Quaternion currRot;
	#endregion

	#region Input
	PlayerInput _input;
	#endregion

	#region State
	protected Define.State _state = Define.State.Idle;
	public virtual Define.State State
	{
		get { return _state; }
		set
		{
			_state = value;
			pv.RPC("SyncState", RpcTarget.AllBuffered, _state);
		}
	}
	protected GameObject _lockTarget;
    protected Vector3 _destPos;
	bool _stopSkill = false;
	#endregion

	#region Stat
	PlayerStat _stat;
	#endregion


	void Start()
    {
		pv = GetComponent<PhotonView>();
		State = Define.State.Idle;
		_stat = gameObject.GetOrAddComponent<PlayerStat>();
		_stat.OnHPisZero += OnDeadEvent;
		_input = gameObject.GetOrAddComponent<PlayerInput>();
		_input.MouseAction -= OnPlayerMouseEvent;
		_input.MouseAction += OnPlayerMouseEvent;
		Manager_Player.Instance.player = gameObject;
	}
    void Update()
	{
		switch (State)
		{
			case Define.State.Die:
				UpdateDie();
				break;
			case Define.State.Moving:
				UpdateMoving();
				break;
			case Define.State.Idle:
				UpdateIdle();
				break;
			case Define.State.Skill:
				UpdateSkill();
				break;
		}
	}

	void OnPlayerMouseEvent(Define.MouseEvent evt)
	{
		if (!pv.IsMine) return;
		switch (State)
		{
			case Define.State.Idle:
				MouseActionFromIdle(evt);
				break;
			case Define.State.Moving:
				MouseActionFromIdle(evt);
				break;
			case Define.State.Skill:
				MouseActionFromSkill(evt);
				break;
		}
	}
	void MouseActionFromIdle(Define.MouseEvent evt)
	{
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		bool raycastHit = Physics.Raycast(ray, out hit, 100.0f, _mask);
		Debug.DrawRay(Camera.main.transform.position, ray.direction * 100.0f, Color.red, 1.0f);

		switch (evt)
		{
			case Define.MouseEvent.PointerDown:
				{
					if (raycastHit)
					{
						if (hit.collider.gameObject.layer == (int)Define.Layer.Ground)
						{
							_destPos = hit.point;
							State = Define.State.Moving;
							_stopSkill = false;
						}
						if (hit.collider.gameObject.layer == (int)Define.Layer.Monster)
							_lockTarget = hit.collider.gameObject;
						else
							_lockTarget = null;
					}
				}
				break;
			case Define.MouseEvent.Press:
				{
					if (_lockTarget == null && raycastHit)
					{
						if (hit.collider.gameObject.layer == (int)Define.Layer.Ground)
							_destPos = hit.point;
						else if(hit.collider.gameObject.layer == (int)Define.Layer.Monster)
							_lockTarget = hit.collider.gameObject;
					}
				}
				break;
			case Define.MouseEvent.PointerUp:
				_stopSkill = true;
				break;
		}
	}
	void MouseActionFromSkill(Define.MouseEvent evt)
	{
		if (evt == Define.MouseEvent.PointerUp)
			_stopSkill = true;
	}

	protected void UpdateMoving() 
	{
		if (!pv.IsMine)
		{
			if (transform.position != currPos)
				transform.position = Vector3.Lerp(transform.position, currPos, Time.deltaTime * 10.0f);
			if (transform.rotation != currRot)
				transform.rotation = Quaternion.Lerp(transform.rotation, currRot, Time.deltaTime * 10.0f);
			return;
		}
		// 몬스터를 사정거리내에서  클릭시 공격 상태로 변환 
		if (_lockTarget != null)
		{
			_destPos = _lockTarget.transform.position;
			float distance = (_destPos - transform.position).magnitude;
			if (distance <= 1)
			{
				State = Define.State.Skill;
				return;
			}
		}
		// 이동
		Vector3 dir = _destPos - transform.position;
		if (dir.magnitude < 0.1f)
		{
			State = Define.State.Idle;
		}
		else
		{
			Debug.DrawRay(transform.position + Vector3.up * 0.5f, dir.normalized, Color.green);
			if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, 1.0f, LayerMask.GetMask("Block")))
			{
				if (Input.GetMouseButton(0) == false)
					State = Define.State.Idle;
				return;
			}
			float moveDist = Mathf.Clamp(_stat.MoveSpeed * Time.deltaTime, 0, dir.magnitude);
			transform.position += dir.normalized * moveDist;
			//transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 20 * Time.deltaTime);
			transform.rotation=Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir, Vector3.up), 20 * Time.deltaTime);
		}
	}
	protected void UpdateSkill()
	{
		if (!pv.IsMine) return;
		if (_lockTarget != null)
		{
			Vector3 dir = _lockTarget.transform.position - transform.position;
			Quaternion quat = Quaternion.LookRotation(dir);
			transform.rotation = Quaternion.Lerp(transform.rotation, quat, 20 * Time.deltaTime);
		}
	}
	protected void UpdateIdle()
	{
	}
	protected void UpdateDie() 
	{
	}

	public void OnHitEvent()
	{
		pv.RPC("SyncHitEvent", RpcTarget.AllBuffered);
	}
	public void OnDeadEvent()
	{
		if (pv.IsMine)
			Manager_UI.Instance.OpenUI("Dead");
		pv.RPC("SyncDeadEvent", RpcTarget.AllBuffered);
	}

	[PunRPC]
	void SyncState(Define.State newState)
	{
		_state = newState;
		Animator anim = GetComponent<Animator>();
		switch (_state)
		{
			case Define.State.Die:
				anim.CrossFade("DEAD", 0.1f, -1, 0);
				break;
			case Define.State.Idle:
				anim.CrossFade("IDLE", 0.1f);
				break;
			case Define.State.Moving:
				anim.CrossFade("RUN", 0.1f);
				break;
			case Define.State.Skill:
				anim.CrossFade("ATTACK", 0.1f, -1, 0);
				break;
		}
	}
	[PunRPC]
	void SyncHitEvent()
	{
		if (_lockTarget != null)
		{
			Stat targetStat = _lockTarget.GetComponent<Stat>();
			if (targetStat.Hp!=0)
				targetStat.OnAttacked(_stat);
		}
		if (_stopSkill) 
			State = Define.State.Idle;
		else
			State = Define.State.Skill;
	}
	[PunRPC]
	void SyncDeadEvent()
	{
		//2초 정도 뒤에 기능정지 + 기본값으로 초기화 함수 실행필요 
		State = Define.State.Die;
		Manager_Player.Instance.isPlayerDead = true;
		Invoke("CharacterDestroy", 2.0f);
	}
	void CharacterDestroy()
	{PhotonNetwork.Destroy(gameObject);}
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
			stream.SendNext(_state);
		}
		else if (stream.IsReading)
		{
			currPos = (Vector3)stream.ReceiveNext();
			currRot = (Quaternion)stream.ReceiveNext();
			_state = (Define.State)stream.ReceiveNext();
		}
	}


}
