using UnityEngine;
using System.Collections;
using ThugLib;

public class ActorScript : MonoBehaviour {

    public bool moving = false;
    private Vector3 movingTo   = new Vector3(0,0,0);

    public float smooth = 15;
    private GameManagerScript gm; // GameManager
    private LevelManagerScript lm; // LevelManager

    public ActorEntity entity = new ActorEntity();

    void Awake()
    {
        GameObject g = GameObject.Find("GameManager");
        gm = g.GetComponent<GameManagerScript>();
        lm = gm.lm;

        this.entity.index = gm.entity.RegisterEntity(this.entity);
    }

    public int GetX()
    {
        return this.entity.GetX();
    }
    public int GetY()
    {
        return this.entity.GetY();
    }

    // XXX delete
    public bool CanMoveTo (int x, int y)
    {
        if (lm)
        {
            if ((x < 0) || (x >= lm.levelWidth))
                return false;
            if ((y < 0) || (y >= lm.levelHeight))
                return false;
            if (lm.entity.GetCell(x,y).GetHindrance() < 1f)
                return true;
        }
        return false;
    }

    public void TeleportTo (Vector3 destination)
    {
        transform.position = destination;
        this.entity.MoveTo((int)destination.x, (int)destination.y);
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
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
