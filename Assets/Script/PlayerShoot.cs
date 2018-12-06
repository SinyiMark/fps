using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(WeaponManager))]
[RequireComponent(typeof(PlayerAudioManager))]
public class PlayerShoot : NetworkBehaviour {

    private const string playerTag = "Player";

    private int KillCount;

    private PlayerWeapon currentWeapon;
    private WeaponManager weaponManager;

    private PlayerAudioManager audioManager;

    public Camera cam;
    public LayerMask mask;
    private GameObject playerUIInstance;

    void Start ()
    {
        weaponManager = GetComponent<WeaponManager>();
        audioManager = GetComponent<PlayerAudioManager>();
        playerUIInstance = GetComponent<PlayerSetup>().playerUIInstance;
        CancelInvoke("Shoot");
    }
	
	void Update ()
    {
        currentWeapon = weaponManager.GetCurrentWeapon();
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
            }
            else if (Input.GetButtonUp("Fire1"))
            {
                CancelInvoke("Shoot");
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
        audioManager.ShootEffect.Play();
        weaponManager.GetCurrentGraphics().muzzleFlash.Play();
    }

    [Command]
    void CmdOnHit(Vector3 _pos, Vector3 _normal)
    {
        RpcDoHitEffect(_pos,_normal);
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

        RaycastHit _hit;
        if (Physics.Raycast(cam.transform.position,cam.transform.forward, out _hit,currentWeapon.range, mask))
        {
            if (_hit.collider.tag.Contains(playerTag) && _hit.collider.transform.name != cam.transform.parent.name)
            {
                ShowHitMark();
                CmdPlayerShot(_hit.collider.name, currentWeapon.damage, _hit.point);
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
        if (_player.WillDeath(damage))
        {
            KillCount++;
        }
        _player.RpcTakeDamage(damage, hitPoint);
    }
}
