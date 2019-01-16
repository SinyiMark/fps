using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerUi : MonoBehaviour {

    public GameObject PauseMenuUi;
    public GameObject CrossHair;
    public GameObject HitMark;
    public GameObject UIDamageEffect;
    public TMP_Text HPText;
    public TMP_Text KillCountText;

    private void Start()
    {
        PauseMenu.IsOn = false;
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    private void TogglePauseMenu()
    {
        PauseMenuUi.SetActive(!PauseMenuUi.activeSelf);
        PauseMenu.IsOn = PauseMenuUi.activeSelf;
    }


    #region Ui Damage Effect
    public void UIDamageEffectDisplay()
    {
        StopCoroutine(HideUIDamageEffect());
        if (UIDamageEffect != null)
        {
            UIDamageEffect.gameObject.SetActive(true);
        }
        StartCoroutine(HideUIDamageEffect());
    }

    IEnumerator HideUIDamageEffect()
    {
        yield return new WaitForSeconds(0.5f);

        UIDamageEffect.gameObject.SetActive(false);


    }

    public void StopOnPlayerDie()
    {
        StopCoroutine(HideUIDamageEffect());
    }

    #endregion
}
