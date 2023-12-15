using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterController : BaseController
{
	Stat _stat;
	public override Define.State State
	{
		get { return _state; }
		set
		{
			_state = value;
			Animator anim = GetComponent<Animator>();
			switch (_state)
			{
				case Define.State.Die:
					anim.CrossFade("DEAD", 0.1f, -1, 0);
					OnDead();
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
	}
	[SerializeField]
	const float _scanRange = 10;
	[SerializeField]
	const float _attackRange = 0.5f;
	[SerializeField]
	const float _chaseSpeed = 7;
	[SerializeField]
	const float _wanderRange = 8; // 배회 간격, 범위, 재시작
	float wanderInterval = 5f; 
	float timer = 0f;

	public override void Init()
    {
		WorldObjectType = Define.WorldObject.Monster;
		_stat = gameObject.GetComponent<Stat>();
		State = Define.State.Idle;
	}

	protected override void UpdateIdle()
	{
		_lockTarget = SearchClosePlayer();
		if (_lockTarget == null) return; //월드에 플레이어가 존재하지 않을시 무한대로 멈춰있기 
	
		float distance = (_lockTarget.transform.position - transform.position).magnitude;
		if (distance <= _scanRange) //추격 상태 전환
		{
			State = Define.State.Moving;
			_stat.MoveSpeed = _chaseSpeed;
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
	protected override void UpdateMoving()
	{
		if (_lockTarget == null) return;

		_destPos = _lockTarget.transform.position;
		float distance = (_destPos - transform.position).magnitude;

		//인식 범위 체크 
		if (distance >= _scanRange)
		{
			State = Define.State.Idle;
			_stat.MoveSpeed = 3;
			return;
		}
		//공격 가능 체크 
		if (distance <= _attackRange)
		{
			NavMeshAgent nma = gameObject.GetOrAddComponent<NavMeshAgent>();
			nma.SetDestination(transform.position);
			State = Define.State.Skill;
			_stat.MoveSpeed = 3;
			return;
		}
		//추격
		Vector3 dir = _destPos - transform.position;
		if (dir.magnitude < 0.1f)
		{
			State = Define.State.Idle;
			_stat.MoveSpeed = 3;
		}
		else
		{
			NavMeshAgent nma = gameObject.GetOrAddComponent<NavMeshAgent>();
			nma.SetDestination(_destPos);
			nma.speed = _stat.MoveSpeed;

			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 20 * Time.deltaTime);
		}
	}
	protected override void UpdateSkill()
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
				_stat.MoveSpeed = _chaseSpeed;
			}
		}
		else
		{
			State = Define.State.Idle;
			_stat.MoveSpeed = 3;
		}
	}

	protected override void OnDead()
	{
		//기능정지 + 기본값으로 초기화 함수 실행필요 
		Debug.Log("MonsterDie");
		Invoke("DestroyMonster", 5.0f);
	}
	void OnHitEvent()
	{
		if (_lockTarget != null)
		{
			Stat targetStat = _lockTarget.GetComponent<Stat>();
			if (!targetStat.HPisZero)
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

	GameObject SearchClosePlayer() //월드 내의 플레이어를 감지 
	{
		PlayerController[] players = FindObjectsOfType<PlayerController>();
		float minDistance=float.MaxValue;
		int closePlayerNum=-1;
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
	void DestroyMonster()
	{ Managers.Game.Despawn(gameObject); }
}
