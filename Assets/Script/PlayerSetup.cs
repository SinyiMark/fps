using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Player))]
public class PlayerSetup : NetworkBehaviour {

    public Behaviour[] componetsToDisable;
    public string remoteLayerName = "RemotePlayer";

    [SerializeField]
    GameObject playerUIPrefab;

    [HideInInspector]
    public GameObject playerUIInstance;

	void Start ()
    {
        if (!isLocalPlayer)
        {
            DisableComponents();
            AssignRemoteLayer();
        }
        else
        {
            playerUIInstance = Instantiate(playerUIPrefab);
            playerUIInstance.name = playerUIPrefab.name;

            GetComponent<Player>().PlayerSetup();
        }
	}

    public override void OnStartClient()
    {
        base.OnStartClient();

        string _netId = this.netId.ToString();
        Player player = GetComponent<Player>();

        GameManager.RegisterPlayer(_netId, player);
    }

    void AssignRemoteLayer()
    {
        gameObject.layer = LayerMask.NameToLayer(remoteLayerName);
    }

    void DisableComponents()
    {
        for (int i = 0; i < componetsToDisable.Length; i++)
        {
            componetsToDisable[i].enabled = false;
        }
    }
	
	void OnDisable ()
    {
        Destroy(playerUIInstance);

        if(isLocalPlayer)
            GameManager.insantce.SetSceneCamraActive(true);

        GameManager.UnRegisterPlayer(transform.name);
    }
}
