using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

public class ServerListItem : MonoBehaviour {

    public delegate void JoinServerDelegat(MatchInfoSnapshot _match);
    private JoinServerDelegat joinServerDelegat;

    [SerializeField]
    private Text serverNameTitle;

    private MatchInfoSnapshot match;

    public void Setup(MatchInfoSnapshot _match, JoinServerDelegat _joinServerCallBack)
    {
        match = _match;
        joinServerDelegat = _joinServerCallBack;
        serverNameTitle.text = match.name + " (" + match.currentSize + "/" + match.maxSize + ")";
    }

    public void JoinServer()
    {
        joinServerDelegat.Invoke(match);
    }
}
