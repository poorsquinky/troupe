using UnityEngine;
using System.Collections;
using ThugLib;

public class ActorScript : MonoBehaviour {

    public bool moving = false;
    private Vector3 movingTo   = new Vector3(0,0,0);

    public float smooth = 15;
    private GameManagerScript gm; // GameManager
    public LevelManagerScript lm; // LevelManager

    public ActorEntity entity = new ActorEntity();

    StatusSpriteScript statusSprite;

    void Awake()
    {
        GameObject g = GameObject.Find("GameManager");
        gm = g.GetComponent<GameManagerScript>();

        this.entity.index = gm.entity.RegisterEntity(this.entity);
    }

    void Start()
    {
        statusSprite = Instantiate(gm.statusSpritePrefab).GetComponent<StatusSpriteScript>();
        statusSprite.SetActor(this);
    }

    public void Attack(ActorEntity foe)
    {
        foe.GetHP();
        foe.stats["hp"]--;
        if (foe.stats["hp"] < 0)
            foe.stats["hp"] = 0;

        int x1 = this.entity.GetX();
        int y1 = this.entity.GetY();
        int x2 = foe.GetX();
        int y2 = foe.GetY();

        if (x1 == x2)
        {
            if (y1 < y2)
                statusSprite.Attack(0);
            else
                statusSprite.Attack(4);
        }
        else if (y1 == y2)
        {
            if (x1 < x2)
                statusSprite.Attack(2);
            else
                statusSprite.Attack(6);
        }
        else if (x1 < x2)
        {
            if (y1 < y2)
                statusSprite.Attack(1);
            else
                statusSprite.Attack(3);
        }
        else if (x1 > x2)
        {
            if (y1 < y2)
                statusSprite.Attack(7);
            else
                statusSprite.Attack(5);
        }


    }

    public void SetSprite(Sprite s)
    {
        GetComponent<SpriteRenderer>().sprite = s;
    }

    public int GetX()
    {
        return this.entity.GetX();
    }
    public int GetY()
    {
        return this.entity.GetY();
    }

    public bool CanMoveTo (int x, int y)
    {
        lm = gm.lm;
        if (lm)
        {
            if ((x < 0) || (x >= lm.levelWidth))
                return false;
            if ((y < 0) || (y >= lm.levelHeight))
                return false;
            CellEntity cell = lm.entity.GetCell(x,y);
            if (cell.GetHindrance() < 1f && cell.GetActor() == null)
                return true;

        }
        return false;
    }

    public void TeleportTo (Vector3 destination)
    {
        transform.position = destination;
        this.entity.TeleportTo((int)destination.x, (int)destination.y);
        moving = false;
        movingTo = destination;
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        lm = gm.lm;
        if ((lm != null) && (GetComponent<PlayerScript>() == null))
        {
            GetComponent<SpriteRenderer>().enabled = 
               lm.IsVisibleToPC(MathUtils.RoundToInt(transform.position.x),
               MathUtils.RoundToInt(transform.position.y));
        }
    }

    public void TeleportTo (int x, int y)
    {
        Vector3 pos = transform.position;
        pos.x = x;
        pos.y = y;
        TeleportTo(pos);
    }

    public void MoveTo (Vector3 destination)
    {
        if (this.entity.MoveTo((int)destination.x, (int)destination.y))
        {
            moving = true;
            movingTo = destination;
            UpdateVisibility();
        }

    }

    public void MoveTo (int x, int y)
    {
        Vector3 newDest = transform.position;
        newDest.x = x;
        newDest.y = y;
        this.MoveTo(newDest);
    }

    void FixedUpdate ()
    {
        if (entity.GetHP() < 1 && entity.isPlayer == false)
        {
            CellEntity cell           = this.entity.parent as CellEntity;
            GameObject corpse         = Instantiate(gm.itemPrefab) as GameObject;
            Vector3 pos               = new Vector3(cell.x,cell.y,0);
            ItemEntity corpseEntity   = corpse.GetComponent<ItemScript>().entity;
            corpse.transform.position = pos;

            corpseEntity.shortDescription = "corpse";
            corpseEntity.longDescription = "the corpse of a " + entity.shortDescription;

            cell.ActorForceExit();
            cell.AddItem(corpseEntity);
            Destroy(this.gameObject);
            return;
        }
        if (moving)
        {
            Vector3 newPos = transform.position;
            newPos.x = Mathf.Lerp(transform.position.x,movingTo.x,Time.deltaTime*this.smooth);
            newPos.y = Mathf.Lerp(transform.position.y,movingTo.y,Time.deltaTime*this.smooth);
            transform.position = newPos;
            float dist = Vector3.Distance(transform.position, movingTo);
            if (dist < 0.025)
            {
                transform.position = movingTo;
                moving = false;
            }
            UpdateVisibility();
        }
    }

}
