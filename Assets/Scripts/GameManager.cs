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
    private GameObject countDownPanel;
    [SerializeField]
    private TMP_Text countDownText;
    [SyncVar(hook = nameof(HandleCountDownUpdate))]
    private int countdown;

    [SerializeField]
    private TMP_Text playerTurnTimeDisplay;
    [SyncVar(hook = nameof(HandleTurnCountDownUpdate))]
    private int playerTurnCountDown;

    private void Start() {
        countdown = 10;
        if (isServer) {
            MazeGenerator.mazeGenerated += SpawnItems;
            StartCoroutine(StartCountdown());
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

    IEnumerator StartCountdown() {
        for(int timeIndex = 1; timeIndex >= 0; timeIndex--) {
            countdown = timeIndex;
            countDownText.text = timeIndex.ToString();
            yield return new WaitForSeconds(1);
        }

        StartCoroutine(PlayerTurnCountdown());
    }

    IEnumerator PlayerTurnCountdown() {
        movementEnabled = true;
        for (int timeIndex = 10; timeIndex >= 0; timeIndex--) {
            playerTurnCountDown = timeIndex;
            playerTurnTimeDisplay.text = timeIndex.ToString();
            yield return new WaitForSeconds(1);
        }

        StartCoroutine(MazeChangeCountDown());
    }

    IEnumerator MazeChangeCountDown() {
        Debug.Log("[MazeGenerator] Changing maze");
        movementEnabled = false;
        //for (int timeIndex = 3; timeIndex >= 0; timeIndex--) {
            yield return new WaitForSeconds(1);
        //}

        //GameObject.Find("MazeGenerator(Clone)").GetComponent<MazeGenerator>().MazeChangeUp();

        StartCoroutine(StartCountdown());
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
