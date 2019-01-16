using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using System;

[RequireComponent(typeof(PlayerSetup))]
[RequireComponent(typeof(PlayerAudioManager))]
public class Player : NetworkBehaviour {

    [SerializeField]
    private string playerGraphicsLayerName = "LocalPlayerGraphics";
    [SerializeField]
    private string deadPlayerGraphicsLayerName = "DeadPlayerGraphics";

    [SyncVar]
    private bool _isDead;
    public bool IsDead
    {
        get { return _isDead; }
        protected set { _isDead = value; }
    }

    private GameObject playerUIInstance;

    private PlayerAudioManager audioManager;

    [SerializeField]
    private GameObject playerGraphics;

    [SyncVar]
    [SerializeField]
    private int currentHp;

    [SerializeField]
    private int maxHP = 100;

    [SerializeField]
    private Behaviour[] disableOnDeath;
    private bool[] wasEnable;

    [SerializeField]
    private GameObject[] disableGameObjectOnDeath;

    [SerializeField]
    private Rigidbody rigidbody;

    [SerializeField]
    private Collider[] colliders;

    #region Effects
    [SerializeField]
    private GameObject deathEffect;
    #endregion 

    public void PlayerSetup ()
    {
        
        CmdBroadCastNewPlayerSetup();
	}

    [Command]
    private void CmdBroadCastNewPlayerSetup()
    {
        RpcSetupPlayerOnAllClient();
    }

    [ClientRpc]
    private void RpcSetupPlayerOnAllClient()
    {
        if (wasEnable == null)
        {
            wasEnable = new bool[disableOnDeath.Length];
            for (int i = 0; i < wasEnable.Length; i++)
            {
                wasEnable[i] = disableOnDeath[i].enabled;
            }
        }

        if (isLocalPlayer)
        {
            Util.SetLayerRecursively(playerGraphics, LayerMask.NameToLayer("LocalPlayerGraphics"));
        }

        //<<<<<<<< Lock cursor >>>>>>>>>>>
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SetDefault();
    }

    public void SetDefault()
    {
        IsDead = false;
        currentHp = maxHP;

        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = wasEnable[i];
        }

        for (int i = 0; i < disableGameObjectOnDeath.Length; i++)
        {
            disableGameObjectOnDeath[i].SetActive(true);
        }

        rigidbody.isKinematic = false;

        foreach (var col in colliders)
        {
            col.enabled = true;
        }


        if (isLocalPlayer)
        {
            GameManager.insantce.SetSceneCamraActive(false);
            GetComponent<PlayerSetup>().playerUIInstance.SetActive(true);
        }

    }

    private void Start()
    {
        playerUIInstance = GetComponent<PlayerSetup>().playerUIInstance;
        audioManager = GetComponent<PlayerAudioManager>();
        

    }

    private void Update()
    {
        #region cursor controll
        // show and unlock the cursor
        if (Cursor.lockState == CursorLockMode.Locked && PauseMenu.IsOn == true)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        // hide and lock teh cursor
        else if (Cursor.lockState == CursorLockMode.None && PauseMenu.IsOn == false)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        #endregion

        #region Hp Hud
        if (isLocalPlayer)
        {
            TMP_Text HpText = playerUIInstance.GetComponent<PlayerUi>().HPText;

            if (HpText != null)
            {
                HpText.text = "Hp: " + currentHp;
            }
        }
        #endregion


        //<<<<<<<<<<<<<<<< Test: Got damage !!!!!! >>>>>>>>>>>>>>>>>>>
        if (Input.GetKeyDown(KeyCode.K) && isLocalPlayer)
        {
            RpcTakeDamage(20, playerGraphics.transform.position);
        }
    }

    // <<<<<<<<<<< This check i kill the other player >>>>>>>>>>>
    internal bool WillDeath(int damage)
    {
        if (currentHp - damage <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }



    [ClientRpc]
    internal void RpcTakeDamage(int damage, Vector3 hitPoint)
    {

        if (IsDead) return;
        currentHp = currentHp - damage;
        Debug.Log(transform.name + " got " + damage + " damge  and HP: " + currentHp);

        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            ShowUIDamageEffect();
        }
    }

    private void ShowUIDamageEffect()
    {
        if (isLocalPlayer)
        {
            playerUIInstance.GetComponent<PlayerUi>().UIDamageEffectDisplay();
        }
    }

    private void Die()
    {
        RpcPlayDeathEffect();

        IsDead = true;
        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = false;
        }
        for (int i = 0; i < disableGameObjectOnDeath.Length; i++)
        {
            disableGameObjectOnDeath[i].SetActive(false);
        }

        rigidbody.isKinematic = true;

        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        GameObject _deathEffect = Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(_deathEffect, 3f);

        if (isLocalPlayer)
        {
            audioManager = GetComponent<PlayerAudioManager>();
            GameManager.insantce.SetSceneCamraActive(true);
            playerUIInstance.SetActive(false);
        }

        Debug.Log(transform.name + " is DEAD!");
        StartCoroutine(Respawn());
    }

    [ClientRpc]
    private void RpcPlayDeathEffect()
    {
        audioManager.DeathEffect.Play();
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(GameManager.insantce.matchSettings.RespawnTime);

        if (isLocalPlayer)
        {
            playerUIInstance.GetComponent<PlayerUi>().StopOnPlayerDie();
        }

        Transform _spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = _spawnPoint.position;
        transform.rotation = _spawnPoint.rotation;

        yield return new WaitForSeconds(0.1f);

        PlayerSetup();
    }
}
