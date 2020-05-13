using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostPrefsController : MonoBehaviour{

    [SerializeField]
    private LobbyNetworkManager networkManager = null;

    [SerializeField]
    private Toggle hostAndJoinToggle = null;
    [SerializeField]
    private TMP_Dropdown numPlayersDropdown = null;

    [SerializeField]
    private GameObject playerListPanel = null;
    [SerializeField]
    private GameObject enterPlayerPanel = null;


    public void SubmitForm() {
        networkManager.maxConnections = numPlayersDropdown.value + 2;
        networkManager.StartHost();

        if (hostAndJoinToggle.isOn) {
            enterPlayerPanel.SetActive(true);
        }
        else {
            playerListPanel.SetActive(true);
        }

        gameObject.SetActive(false);
    }

}
