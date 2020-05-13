using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MazeGenerator : MonoBehaviour{

    public Transform mazeParent;

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

    private void Awake() {
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
        cells = new MazeCell[rows * columns];
        unitSize = 6;
        AddWalls();
        AddPillars();

        CreatePaths((rows/2) * (columns/2));
        //CreatePaths(rows * columns - 1);
    }

    public void AddWalls() {
        Vector3 unitPosition = Vector3.zero;

        //first unit
        cells[0].position = unitPosition;
        cells[0].walls = new GameObject[4];
        cells[0].walls[0] = Instantiate(frontWall, unitPosition, Quaternion.identity, mazeParent);
        cells[0].walls[1] = Instantiate(rightWall, unitPosition, Quaternion.identity, mazeParent);
        cells[0].walls[2] = Instantiate(backWall, unitPosition, Quaternion.identity, mazeParent);
        cells[0].walls[3] = Instantiate(leftWall, unitPosition, Quaternion.identity, mazeParent);

        //first row
        for (int columnIndex = 1; columnIndex < columns; columnIndex++) {
            unitPosition.x = columnIndex * unitSize;

            cells[columnIndex].position = unitPosition;
            cells[columnIndex].walls = new GameObject[4];
            cells[columnIndex].walls[0] = Instantiate(frontWall, unitPosition, Quaternion.identity, mazeParent);
            cells[columnIndex].walls[1] = Instantiate(rightWall, unitPosition, Quaternion.identity, mazeParent);
            cells[columnIndex].walls[2] = Instantiate(backWall, unitPosition, Quaternion.identity, mazeParent);
            cells[columnIndex].walls[3] = cells[columnIndex - 1].walls[1];
        }

        for (int rowIndex = 1; rowIndex < rows; rowIndex++) {
            unitPosition.z = rowIndex * unitSize;
            unitPosition.x = 0;

            cells[rowIndex * columns].position = unitPosition;
            cells[rowIndex * columns].walls = new GameObject[4];
            cells[rowIndex * columns].walls[0] = Instantiate(frontWall, unitPosition, Quaternion.identity, mazeParent);
            cells[rowIndex * columns].walls[1] = Instantiate(rightWall, unitPosition, Quaternion.identity, mazeParent);
            cells[rowIndex * columns].walls[2] = cells[(rowIndex - 1) * columns].walls[0];
            cells[rowIndex * columns].walls[3] = Instantiate(leftWall, unitPosition, Quaternion.identity, mazeParent);

            for (int columnIndex = 1; columnIndex < columns; columnIndex++) {
                unitPosition.x = columnIndex * unitSize;

                cells[rowIndex * columns + columnIndex].position = unitPosition;
                cells[rowIndex * columns + columnIndex].walls = new GameObject[4];
                cells[rowIndex * columns + columnIndex].walls[0] = Instantiate(frontWall, unitPosition, Quaternion.identity, mazeParent);
                cells[rowIndex * columns + columnIndex].walls[1] = Instantiate(rightWall, unitPosition, Quaternion.identity, mazeParent);
                cells[rowIndex * columns + columnIndex].walls[2] = cells[(rowIndex - 1) * columns + columnIndex].walls[0];
                cells[rowIndex * columns + columnIndex].walls[3] = cells[rowIndex * columns + columnIndex - 1].walls[1];
            }
        }
    }

    void AddPillars() {
        Vector3 unitPosition = Vector3.zero;

        for(int rowIndex = 0; rowIndex < rows; rowIndex += 2) {
            unitPosition.z = rowIndex * unitSize;
            for (int columnIndex = 0; columnIndex < columns; columnIndex += 2) {
                unitPosition.x = columnIndex * unitSize;
                Instantiate(pillars, unitPosition, Quaternion.identity, mazeParent);
            }
        }
    }

    void CreatePaths(int startIndex) {

        int cellIndex = startIndex;
        cells[cellIndex].searched = true;
        int cellsSearched = 1;
        //Instantiate(searchedIndicator, cells[0].position, Quaternion.identity);

        List<int> unSearched = new List<int>();
        List<int> cellStack = new List<int>();

        while(cellsSearched + 1 < rows * columns) {
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
                int newCellIndex = unSearched[Random.Range(0, unSearched.Count)];
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
                Instantiate(searchedIndicator, cells[cellIndex].position, Quaternion.identity);
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

    public Vector3 GetCellLocation(int cellIndex) {
        return cells[cellIndex].position;
    }
}
