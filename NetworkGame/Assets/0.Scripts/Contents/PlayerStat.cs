using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : Stat
{
	private void Start()
	{
		_maxHp=100;
		_attack=10;
		_moveSpeed = 5.0f;
		_hp=_maxHp;
	}

	/*
	public void SetStat(Stat stat)
	{
		_hp = stat.Hp;
		_maxHp = stat.MaxHp;
		_attack = stat.Attack;
	}*/

}
