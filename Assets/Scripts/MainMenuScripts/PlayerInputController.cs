using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInputController : MonoBehaviour{

    [SerializeField]
    private LobbyNetworkManager lobbyManager;

    [Header("UI")]
    [SerializeField]
    private TMP_InputField nameInputField = null;
    [SerializeField]
    private Button continueButton = null;

    [SerializeField]
    private GameObject playerListPanel;

    public static string DisplayName { get; private set; }

    private const string PlayerPrefsNameKey = "PlayerName";

    private void Start() => SetUpInputField();

    private void SetUpInputField() {
        if (!PlayerPrefs.HasKey(PlayerPrefsNameKey)) { return; }

        string defaultName = PlayerPrefs.GetString(PlayerPrefsNameKey);

        nameInputField.text = defaultName;

        SetPlayerName(defaultName);
    }

    public void SetPlayerName(string name) {
        continueButton.interactable = !string.IsNullOrEmpty(nameInputField.text);
    }

    public void SavePlayerName() {
        DisplayName = nameInputField.text;

        PlayerPrefs.SetString(PlayerPrefsNameKey, DisplayName);
    }

    public void CancelJoin() {
        if (lobbyManager.isNetworkActive) {
            playerListPanel.SetActive(true);
        }

        gameObject.SetActive(false);
    }
}
