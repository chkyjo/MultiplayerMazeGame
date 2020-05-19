using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class MazeGenerator : NetworkBehaviour{

    public static Action mazeGenerated;

    public GameObject mazeParent;

    public GameObject frontWall;
    public GameObject rightWall;
    public GameObject backWall;
    public GameObject leftWall;

    public GameObject pillars;

    public GameObject searchedIndicator;

    public int rows;
    public int columns;

    public int unitSize;

    public MazeCell[] cells;

    List<int> usedSpawnPoints;
    List<GameObject> randomWallsRemoved;

    private void Awake() {
        mazeParent = new GameObject("MazeParent");
        usedSpawnPoints = new List<int>();
        
        Instance = this;
    }

    public static MazeGenerator Instance { get; private set; }

    [System.Serializable]
    public struct MazeCell {
        public bool searched;
        public Vector3 position;
        public GameObject[] walls; //top, right, bottom, left

    }

    private void Start() {

        if (isServer) {
            Debug.Log("[MazeGenerator] Starting maze generation");
            cells = new MazeCell[rows * columns];
            unitSize = 6;
            AddWalls();
            AddPillars();

            CreatePaths((rows / 2) * (columns / 2));
            int average = Mathf.CeilToInt((rows + columns) / 2);
            average *= Mathf.FloorToInt(average / 3);
            RemoveRandomWalls(average);
        }
        else {
            Debug.Log("[MazeGenerator] Registering maze components");
            ClientScene.RegisterPrefab(frontWall);
            ClientScene.RegisterPrefab(rightWall);
            ClientScene.RegisterPrefab(backWall);
            ClientScene.RegisterPrefab(leftWall);
            ClientScene.RegisterPrefab(pillars);
        }
    }

    public void AddWalls() {
        Vector3 unitPosition = Vector3.zero;

        //first unit
        cells[0].position = unitPosition;
        cells[0].walls = new GameObject[4];
        cells[0].walls[0] = Instantiate(frontWall, unitPosition, Quaternion.identity, mazeParent.transform);
        NetworkServer.Spawn(cells[0].walls[0]);
        cells[0].walls[1] = Instantiate(rightWall, unitPosition, Quaternion.identity, mazeParent.transform);
        NetworkServer.Spawn(cells[0].walls[1]);
        cells[0].walls[2] = Instantiate(backWall, unitPosition, Quaternion.identity, mazeParent.transform);
        NetworkServer.Spawn(cells[0].walls[2]);
        cells[0].walls[3] = Instantiate(leftWall, unitPosition, Quaternion.identity, mazeParent.transform);
        NetworkServer.Spawn(cells[0].walls[3]);

        //first row
        for (int columnIndex = 1; columnIndex < columns; columnIndex++) {
            unitPosition.x = columnIndex * unitSize;

            cells[columnIndex].position = unitPosition;
            cells[columnIndex].walls = new GameObject[4];
            cells[columnIndex].walls[0] = Instantiate(frontWall, unitPosition, Quaternion.identity, mazeParent.transform);
            NetworkServer.Spawn(cells[columnIndex].walls[0]);
            cells[columnIndex].walls[1] = Instantiate(rightWall, unitPosition, Quaternion.identity, mazeParent.transform);
            NetworkServer.Spawn(cells[columnIndex].walls[1]);
            cells[columnIndex].walls[2] = Instantiate(backWall, unitPosition, Quaternion.identity, mazeParent.transform);
            NetworkServer.Spawn(cells[columnIndex].walls[2]);
            cells[columnIndex].walls[3] = cells[columnIndex - 1].walls[1];
        }

        for (int rowIndex = 1; rowIndex < rows; rowIndex++) {
            unitPosition.z = rowIndex * unitSize;
            unitPosition.x = 0;

            cells[rowIndex * columns].position = unitPosition;
            cells[rowIndex * columns].walls = new GameObject[4];
            cells[rowIndex * columns].walls[0] = Instantiate(frontWall, unitPosition, Quaternion.identity, mazeParent.transform);
            NetworkServer.Spawn(cells[rowIndex * columns].walls[0]);
            cells[rowIndex * columns].walls[1] = Instantiate(rightWall, unitPosition, Quaternion.identity, mazeParent.transform);
            NetworkServer.Spawn(cells[rowIndex * columns].walls[1]);
            cells[rowIndex * columns].walls[2] = cells[(rowIndex - 1) * columns].walls[0];
            cells[rowIndex * columns].walls[3] = Instantiate(leftWall, unitPosition, Quaternion.identity, mazeParent.transform);
            NetworkServer.Spawn(cells[rowIndex * columns].walls[3]);

            for (int columnIndex = 1; columnIndex < columns; columnIndex++) {
                unitPosition.x = columnIndex * unitSize;

                cells[rowIndex * columns + columnIndex].position = unitPosition;
                cells[rowIndex * columns + columnIndex].walls = new GameObject[4];
                cells[rowIndex * columns + columnIndex].walls[0] = Instantiate(frontWall, unitPosition, Quaternion.identity, mazeParent.transform);
                NetworkServer.Spawn(cells[rowIndex * columns + columnIndex].walls[0]);
                cells[rowIndex * columns + columnIndex].walls[1] = Instantiate(rightWall, unitPosition, Quaternion.identity, mazeParent.transform);
                NetworkServer.Spawn(cells[rowIndex * columns + columnIndex].walls[1]);
                cells[rowIndex * columns + columnIndex].walls[2] = cells[(rowIndex - 1) * columns + columnIndex].walls[0];
                cells[rowIndex * columns + columnIndex].walls[3] = cells[rowIndex * columns + columnIndex - 1].walls[1];
            }
        }
    }

    void AddPillars() {
        Vector3 unitPosition = Vector3.zero;
        GameObject pillar;

        for(int rowIndex = 0; rowIndex < rows; rowIndex += 2) {
            unitPosition.z = rowIndex * unitSize;
            for (int columnIndex = 0; columnIndex < columns; columnIndex += 2) {
                unitPosition.x = columnIndex * unitSize;
                pillar = Instantiate(pillars, unitPosition, Quaternion.identity, mazeParent.transform);
                NetworkServer.Spawn(pillar);
            }
        }

        mazeGenerated?.Invoke();
    }

    void CreatePaths(int startIndex) {

        int cellIndex = startIndex;
        cells[cellIndex].searched = true;
        int cellsSearched = 1;
        //Instantiate(searchedIndicator, cells[0].position, Quaternion.identity);

        List<int> unSearched = new List<int>();
        List<int> cellStack = new List<int>();

        while(cellsSearched < rows * columns) {
            //if not on the top row
            if(cellIndex - columns >= 0) {
                if(!cells[cellIndex - columns].searched) {
                    unSearched.Add(cellIndex - columns);
                }
            }
            //if not on the right edge
            if(cellIndex % columns != columns - 1) {
                if (!cells[cellIndex + 1].searched) {
                    unSearched.Add(cellIndex + 1);
                }
            }
            //if not on the bottom row
            if (cellIndex + columns < rows * columns) {
                if (!cells[cellIndex + columns].searched) {
                    unSearched.Add(cellIndex + columns);
                }
            }
            //if not on the left edge
            if (cellIndex % columns != 0) {
                if (!cells[cellIndex - 1].searched) {
                    unSearched.Add(cellIndex - 1);
                }
            }

            if(unSearched.Count > 0) {
                int newCellIndex = unSearched[UnityEngine.Random.Range(0, unSearched.Count)];
                if(newCellIndex == cellIndex + columns) {
                    cells[cellIndex].walls[0].SetActive(false);
                }
                else if(newCellIndex == cellIndex + 1) {
                    cells[cellIndex].walls[1].SetActive(false);
                }
                else if (newCellIndex == cellIndex - columns) {
                    cells[cellIndex].walls[2].SetActive(false);
                }
                else if (newCellIndex == cellIndex - 1) {
                    cells[cellIndex].walls[3].SetActive(false);
                }

                cellStack.Add(cellIndex);
                cellIndex = newCellIndex;

                cells[cellIndex].searched = true;
                cellsSearched++;
                unSearched.Clear();
                //Instantiate(searchedIndicator, cells[cellIndex].position, Quaternion.identity, mazeParent);
            }
            else {
                if(cellStack.Count > 0) {
                    cellIndex = cellStack[cellStack.Count - 1];
                    cellStack.RemoveAt(cellStack.Count - 1);
                }
                else {
                    break;
                }
            }
        }

        for(int cell = 0; cell < cells.Length; cell++) {
            cells[cell].searched = false;
        }

    }

    public void MazeChangeUp() {
        Debug.Log("Adding back " + randomWallsRemoved.Count / 3 + " walls");
        for(int wallIndex = 0; wallIndex < randomWallsRemoved.Count; wallIndex+=3) {
            randomWallsRemoved[wallIndex].SetActive(true);
        }

        RemoveRandomWalls(randomWallsRemoved.Count);
    }

    public void RemoveRandomWalls(int numWalls) {
        Debug.Log("Removing " + numWalls + " walls");
        int cellIndex = 0;
        int wall = 0;
        randomWallsRemoved = new List<GameObject>();
        for(int wallIndex = 0; wallIndex < numWalls; wallIndex++) {
            do {
                cellIndex = UnityEngine.Random.Range(0, rows * columns);
            } while (cellIndex < columns || cellIndex > columns * (rows - 1) || cellIndex % columns == 0 || cellIndex % columns == columns - 1);

            wall = UnityEngine.Random.Range(0, 4);

            if (cells[cellIndex].walls[wall].activeInHierarchy) {
                cells[cellIndex].walls[wall].SetActive(false);
                randomWallsRemoved.Add(cells[cellIndex].walls[wall]);
            }
        }
    }

    public Vector3 GetUnusedSpawnPoint() {
        int locationIndex;

        bool newSpawn;

        do {
            newSpawn = true;
            locationIndex = UnityEngine.Random.Range(0, rows * columns);

            //check if same as other used spawn points
            for (int spawnIndex = 0; spawnIndex < usedSpawnPoints.Count; spawnIndex++) {
                if (locationIndex == usedSpawnPoints[spawnIndex]) {
                    newSpawn = false;
                }
            }
        } while (!newSpawn);

        usedSpawnPoints.Add(locationIndex);
        Debug.Log("[MazeGenerator] Spawning at location index: " + locationIndex);
        return cells[locationIndex].position;
    }

    public Vector3 GetCellLocation(int cellIndex) {
        return cells[cellIndex].position;
    }
}
