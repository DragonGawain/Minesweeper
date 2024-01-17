using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;

public enum GameState
{
    MENU,
    STARTUP,
    PLAY,
    LOSE,
    WIN
};

public class GameManager : MonoBehaviour
{
    [SerializeField]
    Grid grid;

    [SerializeField]
    Tilemap tilemap;

    // [SerializeField, Range(3, 50)]
    // int width,
    //     height;

    [SerializeField]
    Slider width,
        height,
        minesSlider;

    [SerializeField]
    GameObject minesRemainingText;

    [SerializeField]
    GameObject cam;

    int mineCount = 0;
    int minesRemaining;
    int mineTarget;
    int flaggedCellsCount = 0;
    public static GameState gameState = GameState.MENU;
    public static int cellsRemaining = 9;
    public static bool requestRefresh = false;

    public void StartNewGame()
    {
        for (int x = tilemap.cellBounds.min.x; x < tilemap.cellBounds.max.x; x++)
        {
            for (int y = tilemap.cellBounds.min.y; y < tilemap.cellBounds.max.y; y++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), null);
            }
        }
        cellsRemaining = (int)(width.value * height.value);
        Vector3Int cellLoc;
        for (int x = 0; x < width.value; x++)
        {
            for (int y = 0; y < height.value; y++)
            {
                cellLoc = new Vector3Int(x, y, 0);
                Cell initCell = ScriptableObject.CreateInstance<Cell>();
                tilemap.SetTile(cellLoc, initCell);
                Cell cell = (Cell)tilemap.GetTile(cellLoc);
                cell.setCellState(CellState.CLOSED);
                tilemap.RefreshTile(cellLoc);
            }
        }
        cam.transform.position = new Vector3(
            width.value / 2,
            height.value / 2,
            cam.transform.position.z
        );
        tilemap.RefreshAllTiles();
        gameState = GameState.MENU;
        mineTarget = (int)minesSlider.value;
        if (mineTarget >= (width.value * height.value) - 15)
            mineTarget = (int)(width.value * height.value) - 15;
        if (mineTarget <= 1)
            mineTarget = 2;
        minesRemaining = mineTarget;
        mineCount = 0;
        flaggedCellsCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
            
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 noZ = new Vector3(pos.x, pos.y);
        Vector3Int mouseCell = grid.WorldToCell(noZ);
        // Game start (opening the first cell)
        switch (gameState)
        {
            case GameState.MENU:
                if (tilemap.HasTile(mouseCell))
                {
                    if (Input.GetMouseButtonUp(0))
                    {
                        Cell cell = (Cell)tilemap.GetTile(mouseCell);
                        cell.setCellState(CellState.OPENED);
                        // tilemap.RefreshTile(mouseCell);
                        Cell tempCell;
                        for (int i = 0; i < 8; i++)
                        {
                            tempCell = GetNeighbor(i, mouseCell, tilemap.GetComponent<Tilemap>());
                            if (tempCell != null && tempCell.getCellState() == CellState.CLOSED)
                            {
                                tempCell.setCellState(CellState.OPENED);
                            }
                        }
                        gameState = GameState.STARTUP;
                    }
                }
                break;
            // Place the mines
            case GameState.STARTUP:
                GenerateMines();
                break;
            // Standard gameplay
            case GameState.PLAY:
                if (cellsRemaining == mineTarget)
                    gameState = GameState.WIN;
                minesRemaining = mineCount - flaggedCellsCount;
                minesRemainingText.GetComponent<TextMeshProUGUI>().text = "" + minesRemaining;
                if (tilemap.HasTile(mouseCell))
                {
                    // flag
                    if (Input.GetMouseButtonUp(1))
                    {
                        Cell cell = (Cell)tilemap.GetTile(mouseCell);
                        if (cell.getCellState() == CellState.CLOSED)
                        {
                            cell.setCellState(CellState.FLAGGGED);
                            flaggedCellsCount++;
                        }
                        else if (cell.getCellState() == CellState.FLAGGGED)
                        {
                            cell.setCellState(CellState.CLOSED);
                            flaggedCellsCount--;
                        }
                        tilemap.RefreshTile(mouseCell);
                    }
                    // open and chording (they share a common input)
                    else if (Input.GetMouseButtonUp(2) || Input.GetMouseButtonUp(0))
                    {
                        Cell cell = (Cell)tilemap.GetTile(mouseCell);
                        // chord
                        if (
                            cell.getCellState() == CellState.OPENED
                            || cell.getCellState() == CellState.UPDATED
                        )
                        {
                            int cellValue = cell.getCellValue();
                            int flagValue = 0;
                            Cell tempCell;
                            for (int i = 0; i < 8; i++)
                            {
                                tempCell = GetNeighbor(
                                    i,
                                    mouseCell,
                                    tilemap.GetComponent<Tilemap>()
                                );
                                if (
                                    tempCell != null
                                    && tempCell.getCellState() == CellState.FLAGGGED
                                )
                                {
                                    flagValue++;
                                }
                            }
                            if (flagValue == cellValue)
                            {
                                for (int i = 0; i < 8; i++)
                                {
                                    tempCell = GetNeighbor(
                                        i,
                                        mouseCell,
                                        tilemap.GetComponent<Tilemap>()
                                    );
                                    if (
                                        tempCell != null
                                        && tempCell.getCellState() == CellState.CLOSED
                                    )
                                    {
                                        tempCell.setCellState(CellState.OPENED);
                                    }
                                }
                            }
                        }
                        // open - after chording so a cell is not instantly chorded if it is complete upon being opened
                        if (cell.getCellState() == CellState.CLOSED && Input.GetMouseButtonUp(0))
                        {
                            cell.setCellState(CellState.OPENED);
                            // tilemap.RefreshTile(mouseCell);
                        }
                        tilemap.RefreshAllTiles();
                    }
                }
                if (requestRefresh)
                {
                    requestRefresh = false;
                    Vector3Int cellPos;
                    for (int x = 0; x < width.value; x++)
                    {
                        for (int y = 0; y < height.value; y++)
                        {
                            cellPos = new Vector3Int(x, y, 0);
                            Cell cell = (Cell)tilemap.GetTile(cellPos);
                            if (cell.getCellState() == CellState.OPENED)
                            {
                                tilemap.RefreshTile(cellPos);
                            }
                        }
                    }
                    // tilemap.RefreshAllTiles();
                }
                break;
            case GameState.LOSE:
                Vector3Int cellLoc;
                for (int x = 0; x < width.value; x++)
                {
                    for (int y = 0; y < height.value; y++)
                    {
                        cellLoc = new Vector3Int(x, y, 0);
                        Cell cell = (Cell)tilemap.GetTile(cellLoc);
                        if (cell.getCellValue() == -1 && cell.getCellState() != CellState.DEAD)
                        {
                            cell.setCellState(CellState.OPENED);
                            tilemap.RefreshTile(cellLoc);
                        }
                    }
                }
                break;
            case GameState.WIN:
                minesRemainingText.GetComponent<TextMeshProUGUI>().text = "CONGRATS!";
                break;
        }
    }

    void GenerateMines()
    {
        for (int i = mineCount; i < mineTarget; i++)
        {
            int x = Mathf.FloorToInt(Random.Range(0, width.value));
            int y = Mathf.FloorToInt(Random.Range(0, height.value));
            Cell cell = (Cell)tilemap.GetTile(new Vector3Int(x, y, 0));
            if (cell.getCellState() == CellState.CLOSED && cell.getCellValue() != -1)
            {
                cell.setCellValue(-1);
                mineCount++;
            }
        }

        if (mineCount == mineTarget)
        {
            tilemap.RefreshAllTiles();
            gameState = GameState.PLAY;
            tilemap.RefreshAllTiles();
        }
    }

    private Cell GetNeighbor(int dir, Vector3Int position, Tilemap tilemap)
    {
        Cell cell = null;
        switch (dir)
        {
            case 0: //W
                if (tilemap.HasTile(new Vector3Int(position.x - 1, position.y, position.z)))
                    cell = (Cell)
                        tilemap.GetTile(new Vector3Int(position.x - 1, position.y, position.z));
                break;
            case 1: //NW
                if (tilemap.HasTile(new Vector3Int(position.x - 1, position.y + 1, position.z)))
                    cell = (Cell)
                        tilemap.GetTile(new Vector3Int(position.x - 1, position.y + 1, position.z));
                break;
            case 2: //N
                if (tilemap.HasTile(new Vector3Int(position.x, position.y + 1, position.z)))
                    cell = (Cell)
                        tilemap.GetTile(new Vector3Int(position.x, position.y + 1, position.z));
                break;
            case 3: //NE
                if (tilemap.HasTile(new Vector3Int(position.x + 1, position.y + 1, position.z)))
                    cell = (Cell)
                        tilemap.GetTile(new Vector3Int(position.x + 1, position.y + 1, position.z));
                break;
            case 4: //E
                if (tilemap.HasTile(new Vector3Int(position.x + 1, position.y, position.z)))
                    cell = (Cell)
                        tilemap.GetTile(new Vector3Int(position.x + 1, position.y, position.z));
                break;
            case 5: //SE
                if (tilemap.HasTile(new Vector3Int(position.x + 1, position.y - 1, position.z)))
                    cell = (Cell)
                        tilemap.GetTile(new Vector3Int(position.x + 1, position.y - 1, position.z));
                break;
            case 6: //S
                if (tilemap.HasTile(new Vector3Int(position.x, position.y - 1, position.z)))
                    cell = (Cell)
                        tilemap.GetTile(new Vector3Int(position.x, position.y - 1, position.z));
                break;
            case 7: //SW
                if (tilemap.HasTile(new Vector3Int(position.x - 1, position.y - 1, position.z)))
                    cell = (Cell)
                        tilemap.GetTile(new Vector3Int(position.x - 1, position.y - 1, position.z));
                break;
        }
        return cell;
    }
}
