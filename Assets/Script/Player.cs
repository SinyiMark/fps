using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

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
    private GameObject deathEffect;

    [SerializeField]
    private GameObject spawnEffect;

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
        audioManager = GetComponent<PlayerAudioManager>();
    }


    //Test: kill myself
    private void Update()
    {
        if (cursorLock == true && Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            cursorLock = false;
        }
        else if (cursorLock == false && Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            cursorLock = true;
        }


            if (!isLocalPlayer)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            RpcTakeDamage(300);
        }
    }

    [ClientRpc]
    internal void RpcTakeDamage(int damage)
    {
        if (IsDead) return;
        currentHp = currentHp - damage;
        Debug.Log(transform.name + " got " + damage + " damge" );

        if (currentHp <= 0)
        {
            Die();
        }
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

        Collider _col = GetComponent<Collider>();
        if (_col != null)
        {
            _col.enabled = true;
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

        Collider _col = GetComponent<Collider>();
        if (_col != null)
        {
            _col.enabled = false;
        }

        GameObject _deathEffect = Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(_deathEffect, 3f);

        if (isLocalPlayer)
        {
            audioManager = GetComponent<PlayerAudioManager>();
            GameManager.insantce.SetSceneCamraActive(true);
            GetComponent<PlayerSetup>().playerUIInstance.SetActive(false);
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
