using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieStat : Stat
{
	[SerializeField]
	protected float _runSpeed;
	public float RunSpeed { get { return _runSpeed; } set { _runSpeed = value; } }

    void Start()
	{
		_maxHp = 100;
		_attack = 10;
		_moveSpeed = 3.0f;
		_runSpeed = 6.0f;
		_hp = _maxHp;
	}
	void OnEnable()
	{
		_maxHp = 100;
		_attack = 10;
		_moveSpeed = 3.0f;
		_runSpeed = 6.0f;
		_hp = _maxHp;
	}
}
