using UnityEngine;
using UnityEngine.Networking;

public class Bullet : NetworkBehaviour
{

    public Rigidbody rigidbody;
    public Collider collider;
    public GameObject hitEffectPrefab;

    void Start () {
        rigidbody.AddForce(rigidbody.transform.forward*10000);
	}
	
	void Update () {
	}

    [Command]
    void CmdBulletOnHit(Vector3 _pos, Vector3 _normal)
    {
        RpcBulletDoHitEffect(_pos, _normal);
    }


    [ClientRpc]
    void RpcBulletDoHitEffect(Vector3 _pos, Vector3 _normal)
    {
        GameObject _hitEffect = (GameObject)Instantiate(hitEffectPrefab, _pos, Quaternion.LookRotation(_normal));
        Destroy(_hitEffect, 1f);
    }


    void OnCollisionEnter(Collision col)
    {
        Destroy(collider.gameObject);
    }
}
