using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(WeaponManager))]
public class PlayerShoot : NetworkBehaviour {

    private const string playerTag = "Player";

    private PlayerWeapon currentWeapon;
    private WeaponManager weaponManager;

    public Camera cam;
    public LayerMask mask;

	void Start ()
    {
        weaponManager = GetComponent<WeaponManager>();
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

    }
    [Command]
    void CmdShoot()
    {
        RpcDoShootEffect();
    }

    [ClientRpc]
    void RpcDoShootEffect()
    {
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

        FindObjectOfType<MyAudioManager>().Play("Shoot");

        CmdShoot();

        RaycastHit _hit;
        if (Physics.Raycast(cam.transform.position,cam.transform.forward, out _hit,currentWeapon.range, mask))
        {
            if (_hit.collider.tag.Contains(playerTag) && _hit.collider.transform.name != cam.transform.parent.name)
            {
                CmdPlayerShot(_hit.collider.name, currentWeapon.damage);
            }

            CmdOnHit(_hit.point, _hit.normal);
        }
    }
    [Command]
    void CmdPlayerShot(string playerId, int damage)
    {
        Player _player = GameManager.GetPlayer(playerId);
        _player.RpcTakeDamage(damage);
    }
}
