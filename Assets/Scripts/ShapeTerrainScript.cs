using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ThugLib;

public class ShapeTerrainScript : MonoBehaviour {

    public enum ShapeTypes{ VisibleAndImpassable, SameSpaceType };
    public ShapeTypes terrainShapeStyle = ShapeTypes.VisibleAndImpassable;

    public List<Sprite> spriteList;

    public TerrainEntity entity = new TerrainEntity();

    public void SetSprite(int spritenum)
    {
        if (spriteList.Count > spritenum)
        {
            GetComponent<SpriteRenderer>().sprite = spriteList[spritenum];
        }
    }

    public void SetCell(CellEntity cell)
    {
        this.entity.SetCell(cell);
        cell.SetTerrain(this.entity);
    }
}
