using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostPrefsController : NetworkBehaviour{

    [SerializeField]
    private LobbyNetworkManager networkManager = null;

    [SerializeField]
    private GameObject playerPrefab;

    [SerializeField]
    private Toggle hostAndJoinToggle = null;
    [SerializeField]
    private TMP_Dropdown numPlayersDropdown = null;
    [SerializeField]
    private TMP_InputField hostName = null;
    [SerializeField]
    private Button hostButton = null;

    [SerializeField]
    private GameObject playerListPanel = null;
    [SerializeField]
    private GameObject playerInputPanel = null;

    public void OnToggleChange(bool value) {
        if (value) {
            if (hostName.text == "") {
                hostButton.interactable = false;
            }
        }
        else {
            hostButton.interactable = true;
        }
    }

    public void OnInputNameUpdated() {
        if(hostName.text == "") {
            if (hostAndJoinToggle.isOn) {
                hostButton.interactable = false;
            }
        }
        else {
            hostButton.interactable = true;
        }
    }


    public void SubmitForm() {
        playerListPanel.SetActive(true);
        playerInputPanel.GetComponent<PlayerInputController>().SetPlayerName(hostName.text);
        networkManager.maxConnections = numPlayersDropdown.value + 2;

        if (hostAndJoinToggle.isOn) {
            networkManager.StartHost();
        }
        else {
            networkManager.StartServer();
        }

        gameObject.SetActive(false);
    }

}
