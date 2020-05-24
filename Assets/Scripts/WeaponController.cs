using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour{

    [SerializeField]
    int _damage;
    Weapon weapon;

    private void Awake() {
        weapon = new Weapon(_damage);
    }

    public Weapon GetWeapon() {
        return weapon;
    }

}
