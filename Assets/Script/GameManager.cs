using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager insantce;

    public MatchSettings matchSettings;

    [SerializeField]
    private GameObject sceneCamera;

    private void Awake()
    {
        if (insantce != null)
        {
            Debug.Log("GameManagar has an other instance");
        }
        else
        {
            insantce = this;
        }

    }

    public void SetSceneCamraActive(bool isActive)
    {
        if (sceneCamera == null)
        {
            return;
        }

        sceneCamera.SetActive(isActive);
    }

    #region Player tracking


    public static Dictionary<string, Player> Players = new Dictionary<string, Player>();

    private const string playerNameFix = "Player ";

    public static void RegisterPlayer(string _netid, Player _player)
    {
        string id = playerNameFix + _netid;
        Players.Add(id, _player);
        _player.transform.name = id;
    }

    public static void UnRegisterPlayer(string id)
    {
        Players.Remove(id);
    }

    public static Player GetPlayer(string _id)
    {
        return Players[_id];
    }

    //void OnGUI()
    //{
    //    GUILayout.BeginArea(new Rect(200,200,200,500));

    //    GUILayout.BeginVertical();

    //    foreach (var player in Players)
    //    {
    //        GUILayout.Label(player.Key);
    //    }

    //    GUILayout.EndVertical();

    //    GUILayout.EndArea();
    //}

    #endregion

}
