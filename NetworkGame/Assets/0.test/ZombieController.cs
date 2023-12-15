using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class ZombieController : MonoBehaviourPun, IPunObservable
{
	#region Photon
	protected Photon.Pun.PhotonView pv;
	protected Vector3 currPos;
	protected Quaternion currRot;
	#endregion

	#region Behavior
	const float _scanRange = 10; //탐지,공격,배회,배회간격
	const float _attackRange = 0.5f;
	const float _wanderRange = 8; 
	const float wanderInterval = 5f;
	float timer = 0f;
	#endregion

	#region State
	protected Define.State _state = Define.State.Idle;
	public virtual Define.State State
	{
		get { return _state; }
		set
		{
			_state = value;
			photonView.RPC("SyncState", RpcTarget.AllBuffered, _state);
		}
	}
	protected GameObject _lockTarget;
	protected Vector3 _destPos;
	#endregion

	#region Stat
	ZombieStat _stat;
	#endregion
	NavMeshAgent nma;


	void Start()
	{
		pv = GetComponent<PhotonView>();
		State = Define.State.Idle;
		_stat = gameObject.GetOrAddComponent<ZombieStat>();
		_stat.OnHPisZero += OnDeadEvent;
        //여기서 현재 스폰된 위치가 네비메시에 붙어있는지 확인 
      
		nma = gameObject.GetOrAddComponent<NavMeshAgent>();

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

	protected void UpdateIdle()
	{
		_lockTarget = SearchClosePlayer();
		if (_lockTarget == null) return; //월드에 플레이어가 존재하지 않을시 무한 대기

		float distance = (_lockTarget.transform.position - transform.position).magnitude;
		if (distance <= _scanRange) //추격 상태 전환
		{
			State = Define.State.Moving;
			return;
		}
		else //배회 
		{
			timer += Time.deltaTime;
			if (timer >= wanderInterval)
			{
				NavMeshAgent nma = gameObject.GetComponent<NavMeshAgent>();
				Vector3 randomDirection = Random.insideUnitSphere * _wanderRange;
				randomDirection += transform.position;
				NavMeshHit navHit;
				NavMesh.SamplePosition(randomDirection, out navHit, _wanderRange, -1);
				Vector3 randomPoint = navHit.position;
				nma.SetDestination(randomPoint);
				timer = 0f;
			}
		}

	}
	protected void UpdateMoving()
	{
		if (_lockTarget == null) return;
		if (nma==null) return;

		_destPos = _lockTarget.transform.position;
		float distance = (_destPos - transform.position).magnitude;

		//인식 범위 체크 
		if (distance >= _scanRange)
		{
			State = Define.State.Idle;			
			return;
		}
		//공격 가능 체크 
		if (distance <= _attackRange)
		{
			NavMeshAgent nma = gameObject.GetOrAddComponent<NavMeshAgent>();
			nma.SetDestination(transform.position);
			State = Define.State.Skill;
			return;
		}
		//추격
		Vector3 dir = _destPos - transform.position;
		if (dir.magnitude < 0.1f)
		{
			State = Define.State.Idle;
		}
		else
		{
			if (nma)
			{
				nma.SetDestination(_destPos);
				if (_state == Define.State.Moving)
					nma.speed = _stat.RunSpeed;
				else
					nma.speed = _stat.MoveSpeed;

				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 20 * Time.deltaTime);
			}
		}
	}
	protected void UpdateSkill()
	{
		if (_lockTarget != null)
		{
			_destPos = _lockTarget.transform.position;
			float distance = (_destPos - transform.position).magnitude;
			if (distance <= _attackRange)
			{
				Vector3 dir = _lockTarget.transform.position - transform.position;
				Quaternion quat = Quaternion.LookRotation(dir);
				transform.rotation = Quaternion.Lerp(transform.rotation, quat, 20 * Time.deltaTime);
			}
			else
			{
				State = Define.State.Moving;
			}
		}
		else
		{
			State = Define.State.Idle;
		}
	}
	protected void UpdateDie()
	{}

	public void OnHitEvent()
	{
		pv.RPC("SyncHitEvent", RpcTarget.AllBuffered);
	}
	public void OnDeadEvent()
	{ pv.RPC("SyncDeadEvent", RpcTarget.AllBuffered); }

	GameObject SearchClosePlayer() //월드 내의 가장 가까운 플레이어 감지 
	{
		PlayerController[] players = FindObjectsOfType<PlayerController>();
		float minDistance = float.MaxValue;
		int closePlayerNum = -1;
		for (int i = 0; i < players.Length; i++)
		{
			float distance = (players[i].transform.position - transform.position).magnitude;
			if (distance < minDistance)
			{
				minDistance = distance;
				closePlayerNum = i;
			}
		}

		if (closePlayerNum == -1) return null;
		else
			return players[closePlayerNum].gameObject;
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
				anim.CrossFade("WANDER", 0.1f);
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
			if (targetStat.Hp>0)
			{
				targetStat.OnAttacked(_stat);
				float distance = (_lockTarget.transform.position - transform.position).magnitude;
				if (distance <= _attackRange)
					State = Define.State.Skill;
				else
					State = Define.State.Moving;
			}
			else
			{
				State = Define.State.Idle;
			}
		}
		else
		{
			State = Define.State.Idle;
		}
	}
	[PunRPC]
	void SyncDeadEvent()
	{
		State = Define.State.Die;
		Manager_Player.Instance.AddMonsterCount(-1);
		if (gameObject)
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
