using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using System;

public class JoinGame : MonoBehaviour {

    private NetworkManager networkManager;

    [SerializeField]
    private Text status;

    [SerializeField]
    private GameObject serverListItemPrefab;

    [SerializeField]
    private Transform serverListParent;


    List<GameObject> serverList = new List<GameObject>();

    void Start ()
    {
        networkManager = NetworkManager.singleton;
        if (networkManager.matchMaker == null)
        {
            networkManager.StartMatchMaker();
        }
        RefreshRoomList();
    }

    public void RefreshRoomList ()
    {
        ClearServerList();
        networkManager.matchMaker.ListMatches(0, 20, "", false, 0, 0, OnMatchList);
        status.text = "Loading...";
	}

    private void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> responseData)
    {
        status.text = "";

        if (responseData == null)
        {
            status.text = "Could'nt get server list.";
            return;
        }
        
        foreach (var server in responseData)
        {
            GameObject _serverListItemGO = Instantiate(serverListItemPrefab);
            _serverListItemGO.transform.SetParent(serverListParent);

            ServerListItem _serverListItem = _serverListItemGO.GetComponent<ServerListItem>();
            if (_serverListItem != null)
            {
                _serverListItem.Setup(server, JoinServer);
            }

            serverList.Add(_serverListItemGO);
        }
        if (serverList.Count == 0)
        {
            status.text = "No server found";
        }
    }

    private void JoinServer(MatchInfoSnapshot _match)
    {
        networkManager.matchMaker.JoinMatch(_match.networkId, "", "", "", 0, 0, networkManager.OnMatchJoined);
        ClearServerList();
        status.text = "Joining " + _match.name;
    }

    private void ClearServerList()
    {
        for (int i = 0; i < serverList.Count; i++)
        {
            Destroy(serverList[i]);
        }

        serverList.Clear();
    }
}
