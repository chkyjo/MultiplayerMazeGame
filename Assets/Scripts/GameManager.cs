using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : NetworkBehaviour{

    public static Action gameManagerActive;

    public int numPlayers;

    public int currentPlayer;

    public GameStateUIController gameStateUI;

    private bool movementEnabled = false;

    [SerializeField]
    private Transform playersParent;

    [SerializeField]
    private GameObject countDownPanel;
    [SerializeField]
    private TMP_Text countDownText;
    [SyncVar(hook = nameof(HandleCountDownUpdate))]
    private int countdown;

    [SerializeField]
    private Transform currentPlayerPanel;
    [SerializeField]
    private TMP_Text currentPlayerDisplayName;
    [SyncVar(hook = nameof(HandleCurrentPlayerNameUpdate))]
    private string currentPlayerName;
    [SerializeField]
    private TMP_Text playerTurnTimeDisplay;
    [SyncVar(hook = nameof(HandleTurnCountDownUpdate))]
    private int playerTurnCountDown;

    private void Start() {
        countdown = 3;
        if (isServer) {
            MazeGenerator.mazeGenerated += SpawnItems;
            StartCoroutine(PlayerRotation());
        }
        gameManagerActive?.Invoke();


    }

    private void SpawnItems() {
        MazeGenerator mazeGenerator = GameObject.Find("MazeGenerator(Clone)").GetComponent<MazeGenerator>();

        ItemSpawnManager itemSpawnManager = GetComponent<ItemSpawnManager>();

        for(int itemIndex = 0; itemIndex < 10; itemIndex++) {
            Vector3 spawnPosition = mazeGenerator.GetUnusedSpawnPoint();
            itemSpawnManager.SpawnItem(spawnPosition);
        }

    }

    IEnumerator PlayerRotation() {
        LobbyNetworkManager lobbyManager = NetworkManager.singleton as LobbyNetworkManager;

        while(playersParent.childCount != lobbyManager.GetNumPlayers()) {
            Debug.Log("[GameManager] Waiting for players...");
            yield return new WaitForSeconds(1);
        }

        PlayerController currentPlayer;
        
        while (true) {
            Debug.Log("[GameManager] Player with " + playersParent.childCount + " players");
            for (int playerIndex = 1; playerIndex < playersParent.childCount; playerIndex++) {

                //set up next player
                currentPlayer = playersParent.GetChild(playerIndex).GetComponent<PlayerController>();
                currentPlayerName = currentPlayer.DisplayName;
                currentPlayerDisplayName.text = currentPlayerName;

                yield return StartCoroutine(StartCountDown());
                currentPlayer.SetCurrentPlayer(true);
                yield return StartCoroutine(PlayerTurnCountdown());
                currentPlayer.SetCurrentPlayer(false);

            }

        }
    }

    IEnumerator StartCountDown() {
        countDownPanel.SetActive(true);
        for (int timeIndex = 3; timeIndex > 0; timeIndex--) {
            countdown = timeIndex;
            countDownText.text = timeIndex.ToString();
            yield return new WaitForSeconds(1);
        }
        countdown = 0;
        countDownPanel.SetActive(false);
    }

    IEnumerator PlayerTurnCountdown() {
        movementEnabled = true;
        for (int timeIndex = 20; timeIndex > 0; timeIndex--) {
            playerTurnCountDown = timeIndex;
            playerTurnTimeDisplay.text = timeIndex.ToString();
            yield return new WaitForSeconds(1);
        }
        playerTurnCountDown = 0;
        playerTurnTimeDisplay.text = "0";
        //StartCoroutine(MazeChangeCountDown());
    }

    IEnumerator MazeChangeCountDown() {
        Debug.Log("[MazeGenerator] Changing maze");
        movementEnabled = false;
        //for (int timeIndex = 3; timeIndex >= 0; timeIndex--) {
            yield return new WaitForSeconds(1);
        //}

        //GameObject.Find("MazeGenerator(Clone)").GetComponent<MazeGenerator>().MazeChangeUp();

        //StartCoroutine(StartCountdown());
    }

    private void HandleCountDownUpdate(int oldValue, int newValue) {
        countDownText.text = newValue.ToString();
        if (newValue == 0) {
            countDownPanel.SetActive(false);
        }
        else {
            countDownPanel.SetActive(true);
        }
    }

    private void HandleTurnCountDownUpdate(int oldValue, int newValue) {
        playerTurnTimeDisplay.text = newValue.ToString();
    }

    private void HandleCurrentPlayerNameUpdate(string oldVal, string newVal) {
        currentPlayerDisplayName.text = newVal;
        countDownPanel.transform.GetChild(1).GetComponent<TMP_Text>().text = newVal;
    }

    public void NextPlayer() {
        currentPlayer++;
        if(currentPlayer >= numPlayers) {
            currentPlayer = 0;
        }

        gameStateUI.UpdateCurrentPlayer(currentPlayer);
    }

    public bool GetMovement() {
        return movementEnabled;
    }
    
}
