using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ThugLib;

public class ShapeTerrainScript : MonoBehaviour {

    public enum ShapeTypes{ VisibleAndImpassable, SameSpaceType };
    public ShapeTypes terrainShapeStyle = ShapeTypes.VisibleAndImpassable;

    public List<Sprite> spriteList;

    public TerrainEntity entity = new TerrainEntity();

    private GameManagerScript gm;
    private LevelManagerScript lm;

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

    void Awake ()
    {
        GameObject g = GameObject.Find("GameManager");
        gm = g.GetComponent<GameManagerScript>();
        lm = gm.lm;
    }

    public void ExternalUpdate()
    {
        CellEntity c = this.entity.GetParent() as CellEntity;
        if (c != null)
        {
            int x = c.GetX();
            int y = c.GetY();
            int player_x = lm.GetPlayerX();
            int player_y = lm.GetPlayerY();
            if (
                    (Mathf.Abs(player_x - x) > 4)
                 || (Mathf.Abs(player_y - y) > 4)
               )
                GetComponent<SpriteRenderer>().enabled = false;
            else
                GetComponent<SpriteRenderer>().enabled = true;
        }
    }

}
