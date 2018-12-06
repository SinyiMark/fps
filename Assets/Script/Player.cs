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

    [SerializeField]
    private GameObject spawnEffect;

    [SerializeField]
    private Canvas UIDamageEffect;
    #endregion 

    private bool cursorLock = true;

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

        //Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SetDefault();
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
        if (cursorLock == true && Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            cursorLock = false;
        }
        // hide and lock teh cursor
        else if (cursorLock == false && Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            cursorLock = true;
        }
        #endregion

        if (isLocalPlayer)
        {
            TMP_Text HpText = playerUIInstance.GetComponent<PlayerUi>().HPText;

            if (HpText != null)
            {
                HpText.text = "Hp: " + currentHp;
            }
        }
        
        

        //Test: suicide !!!!!!
        if (Input.GetKeyDown(KeyCode.K) && isLocalPlayer)
        {
            RpcTakeDamage(20, playerGraphics.transform.position);
        }
    }

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

    #region Ui Damage Effect
    private void UIDamageEffectDisplay()
    {
        StopCoroutine(HideUIDamageEffect());
        UIDamageEffect.gameObject.SetActive(true);
        StartCoroutine(HideUIDamageEffect());
    }

    IEnumerator HideUIDamageEffect()
    {
        yield return new WaitForSeconds(1);

        UIDamageEffect.gameObject.SetActive(false);
        

    }
    #endregion

    [ClientRpc]
    internal void RpcTakeDamage(int damage, Vector3 hitPoint)
    {
        //Hit Indicator !!!!!
        //Vector3 targetDir = playerGraphics.transform.position - hitPoint;
        //float angle = Vector3.Angle(targetDir, transform.forward);
        //Debug.Log(angle);

        if (IsDead) return;
        currentHp = currentHp - damage;
        Debug.Log(transform.name + " got " + damage + " damge" );

        if (currentHp <= 0)
        {
            Die();
        }
        //else
        //{
        //    UIDamageEffectDisplay();
        //}
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

            GameObject _spawnEffect = Instantiate(spawnEffect, transform.position, Quaternion.identity);
            Destroy(_spawnEffect, 3f);
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

        Transform _spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = _spawnPoint.position;
        transform.rotation = _spawnPoint.rotation;

        yield return new WaitForSeconds(0.1f);

        PlayerSetup();
    }
}
