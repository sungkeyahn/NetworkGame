using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class Stat : MonoBehaviour
{
    public Action OnHPisZero;

    [SerializeField]
    protected int _hp;
    [SerializeField]
    protected int _maxHp;
    [SerializeField]
    protected int _attack;
    [SerializeField]
    protected float _moveSpeed;

    public int Hp { get { return _hp; } set { _hp = value; } }
    public int MaxHp { get { return _maxHp; } set { _maxHp = value; } }
    public int Attack { get { return _attack; } set { _attack = value; } }
    public float MoveSpeed { get { return _moveSpeed; } set { _moveSpeed = value; } }

    public virtual void OnAttacked(Stat attacker)
    {
        int damage = Mathf.Max(0, attacker.Attack);
        Hp -= damage;
        if (Hp <= 0)
        {
            Hp = 0;
            OnHPisZero.Invoke();
        }
    }
    [PunRPC]
    void RPCOnAttacked(int damage)
    {
        Hp -= damage;
        if (Hp <= 0)
        {
            Hp = 0;
            OnHPisZero.Invoke();
        }
    }

}
