using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(WeaponManager))]
[RequireComponent(typeof(PlayerAudioManager))]
public class PlayerShoot : NetworkBehaviour {

    private const string playerTag = "Player";
    [SyncVar]
    private int KillCount;

    private PlayerWeapon currentWeapon;
    private WeaponManager weaponManager;

    private PlayerAudioManager audioManager;

    public Camera cam;
    public LayerMask mask;
    private GameObject playerUIInstance;
    private PlayerMotor motor;

    private System.Random rnd = new System.Random();

    [SerializeField]
    private float maxBurstlong = 0.05f;

    [SerializeField]
    private float burstlongIncrease = 0.005f;

    [SerializeField]
    private float _burtsLong = 0;
    private float BurstLong
    {
        get { return _burtsLong; }
        set
        {
            if (value >= 0 && value < maxBurstlong)
            {
                if (value < 0)
                {
                    _burtsLong = 0;
                }
                _burtsLong = value;
            }
        }
    }

    [SerializeField]
    private bool canResetBurstLong = true;

    void Start()
    {
        weaponManager = GetComponent<WeaponManager>();
        audioManager = GetComponent<PlayerAudioManager>();
        playerUIInstance = GetComponent<PlayerSetup>().playerUIInstance;
        motor = GetComponent<PlayerMotor>();
        CancelInvoke("Shoot");
    }

    void Update()
    {
        currentWeapon = weaponManager.GetCurrentWeapon();

        if (PauseMenu.IsOn) return;

        if (currentWeapon == null)
        {
            return;
        }

        if (currentWeapon.fireRate <= 0)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot();
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                InvokeRepeating("Shoot", 0f, 1f / currentWeapon.fireRate);
                canResetBurstLong = false;
            }
            else if (Input.GetButtonUp("Fire1"))
            {
                CancelInvoke("Shoot");
                canResetBurstLong = true;
                ResetBurstLong();
            }
        }

        if (isLocalPlayer)
        {
            TMP_Text KillCountText = playerUIInstance.GetComponent<PlayerUi>().KillCountText;

            if (KillCountText != null)
            {
                KillCountText.text = "Kills: " + KillCount;
            }
        }


    }


    private void ResetBurstLong()
    {
        StopCoroutine(ResetBurstLongTimer());
        StartCoroutine(ResetBurstLongTimer());
    }

    IEnumerator ResetBurstLongTimer()
    {
        yield return new WaitForSeconds(2f);
        if (canResetBurstLong == true)
        {
            BurstLong = 0;
        }
    }

    public void OnDisable()
    {
        CancelInvoke("Shoot");
    }

    [Command]
    void CmdShoot()
    {
        RpcDoShootEffect();
    }

    [ClientRpc]
    void RpcDoShootEffect()
    {
        motor.RotateCamera(currentWeapon.recoil * BurstLong * 10);
        audioManager.ShootEffect.Play();
        weaponManager.GetCurrentGraphics().muzzleFlash.Play();
    }

    [Command]
    void CmdOnHit(Vector3 _pos, Vector3 _normal)
    {
        RpcDoHitEffect(_pos, _normal);
    }

    [ClientRpc]
    void RpcDoHitEffect(Vector3 _pos, Vector3 _normal)
    {
        GameObject _hitEffect = (GameObject)Instantiate(weaponManager.GetCurrentGraphics().hitEffectPrefab, _pos, Quaternion.LookRotation(_normal));
        Destroy(_hitEffect, 1f);

    }

    [Client]
    private void Shoot()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        CmdShoot();

        Vector3 randomDir = new Vector3(GetRandomFloat(), GetRandomFloat(), GetRandomFloat());
        Vector3 _direction = cam.transform.forward + randomDir;
        BurstLong = BurstLong + burstlongIncrease;

        RaycastHit _hit;
        if (Physics.Raycast(cam.transform.position, _direction, out _hit, currentWeapon.range, mask))
        {
            Debug.DrawLine(cam.transform.position, _hit.transform.position, Color.green, 0.5f);
            if (_hit.collider.tag.Contains(playerTag) && _hit.collider.transform.name != cam.transform.parent.name)
            {
                ShowHitMark();
                CmdPlayerShot(_hit.collider.name, currentWeapon.damage, _hit.point);
                Player _player = GameManager.GetPlayer(_hit.collider.name);
                if (_player.WillDeath(currentWeapon.damage))
                {
                    KillCount++;
                }
            }

            CmdOnHit(_hit.point, _hit.normal);
        }
    }

    private void ShowHitMark()
    {
        StopCoroutine(HideHitMark());
        GetComponent<PlayerSetup>().playerUIInstance.GetComponent<PlayerUi>().HitMark.gameObject.SetActive(true);
        StartCoroutine(HideHitMark());
    }

    IEnumerator HideHitMark()
    {
        yield return new WaitForSeconds(0.3f);
        GetComponent<PlayerSetup>().playerUIInstance.GetComponent<PlayerUi>().HitMark.gameObject.SetActive(false);
    }

    [Command]
    void CmdPlayerShot(string playerId, int damage, Vector3 hitPoint)
    {
        Player _player = GameManager.GetPlayer(playerId);
        _player.RpcTakeDamage(damage, hitPoint);
    }

    private float GetRandomFloat() { 
        double val = rnd.NextDouble(); // range 0.0 to 1.0
        val -= 0.5; // expected range now -0.5 to +0.5
        val *= 2; // expected range now -1.0 to +1.0
        return BurstLong * (float) val;
    }   
}
