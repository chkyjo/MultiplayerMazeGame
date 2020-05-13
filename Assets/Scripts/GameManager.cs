using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour{

    public int numPlayers;

    public int currentPlayer;

    public GameStateUIController gameStateUI;


    public void NextPlayer() {
        currentPlayer++;
        if(currentPlayer >= numPlayers) {
            currentPlayer = 0;
        }

        gameStateUI.UpdateCurrentPlayer(currentPlayer);
    }

    
}
