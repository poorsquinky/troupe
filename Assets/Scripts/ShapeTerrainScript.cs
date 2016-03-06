using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ThugLib;

public class ShapeTerrainScript : MonoBehaviour {

    public enum ShapeTypes{ VisibleAndImpassable, SameSpaceType, EntitySeedRandom };
    public ShapeTypes terrainShapeStyle = ShapeTypes.VisibleAndImpassable;

    public List<Sprite> spriteList;

    public TerrainEntity entity;

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
        this.entity = cell.GetTerrain();
//        this.entity.SetCell(cell);
//        cell.SetTerrain(this.entity);
        if (terrainShapeStyle == ShapeTypes.EntitySeedRandom && spriteList.Count > 1)
        {
            SetSprite(Mathf.Abs(this.entity.entitySeed % spriteList.Count));
        }
    }

    void Awake ()
    {
        GameObject g = GameObject.Find("GameManager");
        gm = g.GetComponent<GameManagerScript>();
        lm = gm.lm;
//        this.entity.index = gm.entity.RegisterEntity(this.entity);
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
                    // FIXME: don't use hardcoded values here
                    (Mathf.Abs(player_x - x) > 24)
                 || (Mathf.Abs(player_y - y) > 24)
               )
                GetComponent<SpriteRenderer>().enabled = false;
            else
                GetComponent<SpriteRenderer>().enabled = true;
        }
    }

}
