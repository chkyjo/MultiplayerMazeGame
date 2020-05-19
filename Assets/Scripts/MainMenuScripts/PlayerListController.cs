using Mirror;
using Mirror.Examples.Basic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerListController : NetworkBehaviour{

    public Transform joinedPlayersPanel;

    public LobbyNetworkManager lobbyManager;

    public void UpdateDisplay(LobbyPlayer currentPlayer, List<LobbyPlayer> lobbyPlayers) {

        if (!currentPlayer.hasAuthority) {
            foreach (var player in lobbyPlayers) {
                if (player.hasAuthority) {
                    UpdateDisplay(player, lobbyPlayers);
                    break;
                }
            }

            return;
        }

        ClearPlayersPanel(lobbyPlayers);

        SpawnLobbyPlayers(lobbyPlayers);
    }

    public void ClearPlayersPanel(List<LobbyPlayer> lobbyPlayers) {
        for (int playerIndex = 0; playerIndex < lobbyPlayers.Count; playerIndex++) {
            Destroy(joinedPlayersPanel.GetChild(playerIndex).gameObject);
        }
    }

    public void SpawnLobbyPlayers(List<LobbyPlayer> lobbyPlayers) {
        for (int playerIndex = 0; playerIndex < lobbyPlayers.Count; playerIndex++) {
            SpawnLobbyPlayer(lobbyPlayers[playerIndex]);
        }
    }

    public void SpawnLobbyPlayer(LobbyPlayer lobbyPlayer) {
        Instantiate(lobbyPlayer.gameObject, joinedPlayersPanel);
    }

    public void StartGame(string sceneName) {
        for(int playerIndex = 0; playerIndex < joinedPlayersPanel.childCount; playerIndex++) {
            joinedPlayersPanel.GetChild(playerIndex).GetComponent<LobbyPlayer>().StartGame();
        }
    }




}
