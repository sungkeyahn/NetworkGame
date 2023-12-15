using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public abstract class BaseController : MonoBehaviour
{
	public Define.WorldObject WorldObjectType { get; protected set; } = Define.WorldObject.Unknown;
	[SerializeField]
	protected Define.State _state = Define.State.Idle;
	public virtual Define.State State
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
	}
	[SerializeField]
	protected GameObject _lockTarget;
	[SerializeField]
	protected Vector3 _destPos;

	void Start()
	{
		Init();
	}
    void OnEnable()
    {
		Init();
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

	public abstract void Init();
	protected virtual void UpdateMoving() { }
	protected virtual void UpdateIdle() { }
	protected virtual void UpdateSkill() { }
	protected virtual void UpdateDie() { }
	protected virtual void OnDead() { }
}























