using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.Tilemaps.Tile;

public enum CellState
{
    OPENED,
    CLOSED,
    FLAGGGED,
    UPDATED,
    DEAD,
    WRONG
};

[CreateAssetMenu]
public class Cell : TileBase
{
    CellState cellState = CellState.CLOSED;

    // number that appears. -1 is a mine
    int cellValue = 0;
    Sprite cellSprite;
    Sprite tileClosed;
    Sprite tileFlagged;
    Sprite tileMine;
    Sprite tileRedMine;
    Sprite tileXMine;
    Sprite tile0;
    Sprite tile1;
    Sprite tile2;
    Sprite tile3;
    Sprite tile4;
    Sprite tile5;
    Sprite tile6;
    Sprite tile7;
    Sprite tile8;

    public ColliderType colliderType = ColliderType.Sprite;

    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
    {
        Sprite[] tilesAll = Resources.LoadAll<Sprite>("Sprites/TileSprites");

        foreach (Sprite png in tilesAll)
        {
            if (png.name == "TileSprites_0")
                tileClosed = png;
            else if (png.name == "TileSprites_1")
                tileFlagged = png;
            else if (png.name == "TileSprites_2")
                tileMine = png;
            else if (png.name == "TileSprites_3")
                tile0 = png;
            else if (png.name == "TileSprites_4")
                tile1 = png;
            else if (png.name == "TileSprites_5")
                tile2 = png;
            else if (png.name == "TileSprites_6")
                tile3 = png;
            else if (png.name == "TileSprites_7")
                tile4 = png;
            else if (png.name == "TileSprites_8")
                tile5 = png;
            else if (png.name == "TileSprites_9")
                tile6 = png;
            else if (png.name == "TileSprites_10")
                tile7 = png;
            else if (png.name == "TileSprites_11")
                tile8 = png;
        }
        tileRedMine = Resources.Load<Sprite>("Sprites/RedMine");
        tileXMine = Resources.Load<Sprite>("Sprites/XMine");

        return true;
    }

    public int getCellValue()
    {
        return cellValue;
    }

    public CellState getCellState()
    {
        return cellState;
    }

    // part of the reason I'm using getters and setters is to add functionality to setCellValue (also I just prefer the look of accessor methods compared to directly accessing a field)
    public void setCellValue(int val)
    {
        cellValue = val;
        switch (cellValue)
        {
            case 0:
                cellSprite = tile0;
                break;
            case 1:
                cellSprite = tile1;
                break;
            case 2:
                cellSprite = tile2;
                break;
            case 3:
                cellSprite = tile3;
                break;
            case 4:
                cellSprite = tile4;
                break;
            case 5:
                cellSprite = tile5;
                break;
            case 6:
                cellSprite = tile6;
                break;
            case 7:
                cellSprite = tile7;
                break;
            case 8:
                cellSprite = tile8;
                break;
            case -1:
                cellSprite = tileMine;
                break;
        }
    }

    public void setCellState(CellState state)
    {
        cellState = state;
    }

    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        // 
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {

        // Set cell values
        if (GameManager.gameState == GameState.STARTUP && cellValue != -1)
        {
            int mineCount = 0;
            Cell tempCell;
            for (int i = 0; i < 8; i++)
            {
                tempCell = GetNeighbor(i, position, tilemap.GetComponent<Tilemap>());
                if (tempCell != null)
                {
                    if (tempCell.getCellValue() == -1)
                        mineCount++;
                }
            }
            setCellValue(mineCount);
        }
        // zero spreading
        else if (
            GameManager.gameState == GameState.PLAY
            && cellValue == 0
            && cellState == CellState.OPENED
        )
        {
            cellState = CellState.UPDATED;
            Cell tempCell;
            for (int i = 0; i < 8; i++)
            {
                tempCell = GetNeighbor(i, position, tilemap.GetComponent<Tilemap>());
                if (tempCell != null && tempCell.getCellState() == CellState.CLOSED)
                {
                    GameManager.requestRefresh = true;
                    tempCell.setCellState(CellState.OPENED);
                }
            }
            // extra state UPDATED - cells that have already been zero-spreaded will not do so again - optimization
            GameManager.cellsRemaining--;
            // tilemap.GetComponent<Tilemap>().RefreshAllTiles();
        }
        // set cellState to UPDATED, decrement cellsRemaining value
        else if (
            GameManager.gameState == GameState.PLAY
            && cellValue != 0
            && cellValue != -1
            && cellState == CellState.OPENED
        )
        {
            // also using the UPDATED state to avoid double counting cells
            GameManager.cellsRemaining--;
            cellState = CellState.UPDATED;
        }
        // mark losing mine, enter loss state
        else if (
            GameManager.gameState == GameState.PLAY
            && cellValue == -1
            && cellState == CellState.OPENED
        )
        {
            cellState = CellState.DEAD;
            GameManager.gameState = GameState.LOSE;
        }

        // update cell sprite based on state
        if (cellState == CellState.CLOSED)
            tileData.sprite = tileClosed;
        else if (cellState == CellState.FLAGGGED)
            tileData.sprite = tileFlagged;
        else if (cellState == CellState.DEAD)
            tileData.sprite = tileRedMine;
        else if (cellState == CellState.WRONG)
            tileData.sprite = tileXMine;
        else
            tileData.sprite = cellSprite;

        tileData.colliderType = this.colliderType;
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

    private void UpdateNeighborCell(int dir, Vector3Int position, Tilemap tilemap)
    {
        switch (dir)
        {
            case 0: //W
                if (tilemap.HasTile(new Vector3Int(position.x - 1, position.y, position.z)))
                    tilemap.RefreshTile(new Vector3Int(position.x - 1, position.y, position.z));
                break;
            case 1: //NW
                if (tilemap.HasTile(new Vector3Int(position.x - 1, position.y + 1, position.z)))
                    tilemap.RefreshTile(new Vector3Int(position.x - 1, position.y + 1, position.z));
                break;
            case 2: //N
                if (tilemap.HasTile(new Vector3Int(position.x, position.y + 1, position.z)))
                    tilemap.RefreshTile(new Vector3Int(position.x, position.y + 1, position.z));
                break;
            case 3: //NE
                if (tilemap.HasTile(new Vector3Int(position.x + 1, position.y + 1, position.z)))
                    tilemap.RefreshTile(new Vector3Int(position.x + 1, position.y + 1, position.z));
                break;
            case 4: //E
                if (tilemap.HasTile(new Vector3Int(position.x + 1, position.y, position.z)))
                    tilemap.RefreshTile(new Vector3Int(position.x + 1, position.y, position.z));
                break;
            case 5: //SE
                if (tilemap.HasTile(new Vector3Int(position.x + 1, position.y - 1, position.z)))
                    tilemap.RefreshTile(new Vector3Int(position.x + 1, position.y - 1, position.z));
                break;
            case 6: //S
                if (tilemap.HasTile(new Vector3Int(position.x, position.y - 1, position.z)))
                    tilemap.RefreshTile(new Vector3Int(position.x, position.y - 1, position.z));
                break;
            case 7: //SW
                if (tilemap.HasTile(new Vector3Int(position.x - 1, position.y - 1, position.z)))
                    tilemap.RefreshTile(new Vector3Int(position.x - 1, position.y - 1, position.z));
                break;
        }
    }

    private Vector3Int GetNeighborCellLocation(int dir, Vector3Int position, Tilemap tilemap)
    {
        switch (dir)
        {
            case 0: //W
                if (tilemap.HasTile(new Vector3Int(position.x - 1, position.y, position.z)))
                    return new Vector3Int(position.x - 1, position.y, position.z);
                break;                    
            case 1: //NW
                if (tilemap.HasTile(new Vector3Int(position.x - 1, position.y + 1, position.z)))
                    return new Vector3Int(position.x - 1, position.y + 1, position.z);
                break;                    
            case 2: //N
                if (tilemap.HasTile(new Vector3Int(position.x, position.y + 1, position.z)))
                    return new Vector3Int(position.x, position.y + 1, position.z);
                break;                    
            case 3: //NE
                if (tilemap.HasTile(new Vector3Int(position.x + 1, position.y + 1, position.z)))
                    return new Vector3Int(position.x + 1, position.y + 1, position.z);
                break;                    
            case 4: //E
                if (tilemap.HasTile(new Vector3Int(position.x + 1, position.y, position.z)))
                    return new Vector3Int(position.x + 1, position.y, position.z);
                break;                    
            case 5: //SE
                if (tilemap.HasTile(new Vector3Int(position.x + 1, position.y - 1, position.z)))
                    return new Vector3Int(position.x + 1, position.y - 1, position.z);
                break;                    
            case 6: //S
                if (tilemap.HasTile(new Vector3Int(position.x, position.y - 1, position.z)))
                    return new Vector3Int(position.x, position.y - 1, position.z);
                break;                    
            case 7: //SW
                if (tilemap.HasTile(new Vector3Int(position.x - 1, position.y - 1, position.z)))
                    return new Vector3Int(position.x - 1, position.y - 1, position.z);
                break;                    
        }
        return new Vector3Int(-1,-1,-1);

    }
}
