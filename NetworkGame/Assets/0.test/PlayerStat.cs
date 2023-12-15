using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : Stat
{
	void Start()
	{
		_maxHp=100;
		_attack=10;
		_moveSpeed = 5.0f;
		_hp=_maxHp;
	}
	void OnEnable()
	{
		_maxHp = 100;
		_attack = 10;
		_moveSpeed = 5.0f;
		_hp = _maxHp;
	}
}
