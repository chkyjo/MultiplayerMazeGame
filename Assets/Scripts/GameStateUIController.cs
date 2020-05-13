using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameStateUIController : MonoBehaviour{

    public TextMeshProUGUI currentPlayerText;

    public void UpdateCurrentPlayer(int currentPlayer) {
        currentPlayerText.text = currentPlayer.ToString();
    }
}
