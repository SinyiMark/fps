using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerWeapon {

    public string name = "416";
    public int damage = 10;
    public float range = 100f;

    public GameObject graphics;

    public float fireRate = 0f;
    public float recoil = 1;

}
