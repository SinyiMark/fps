using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class PauseMenu : MonoBehaviour {

    private NetworkManager networkManager;

    public static bool IsOn = false;

    private void Start()
    {
        networkManager = NetworkManager.singleton;
    }

    public void ExitServer()
    {
        MatchInfo match = networkManager.matchInfo;
        networkManager.matchMaker.DropConnection(match.networkId, match.nodeId, 0, networkManager.OnDropConnection);
        networkManager.StopHost();
    }
}
