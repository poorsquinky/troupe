using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ThugLib;

public class StatusSpriteScript : MonoBehaviour {

    public Sprite       highlightSprite;
    public List<Sprite> healthSprites    = new List<Sprite>();
    public List<Sprite> attackSprites    = new List<Sprite>();
    public List<Sprite> directionSprites = new List<Sprite>();

    public SpriteRenderer sprite;

    public ActorScript target;

    public float timeout = 0f;

    GameManagerScript gm;

    void Awake()
    {
        gm          = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
        this.sprite = this.GetComponent<SpriteRenderer>();
        this.sprite.enabled = false;
    }

    public void SetActor(ActorScript a)
    {
        transform.position = a.transform.position;
        transform.parent   = a.transform;
        target = a;
    }

    public void FixedUpdate() {
        if (timeout > 0f)
        {
            if (gm.lm.GetPlayer().actor.entity.stats["hp"] > 0)
            {
                timeout -= Time.deltaTime;
                if (timeout <= 0f)
                {
                    timeout = 0f;
                    this.sprite.enabled = false;
                    transform.position = target.transform.position;
                }
            }
        }
    }

    // attack in eight cardinal directions: 0=up, 1=NE, etc.
    public void Attack(int direction)
    {
        Vector3 pos = target.transform.position;
        this.sprite.sprite = attackSprites[direction];
        this.sprite.enabled = true;
        this.timeout = 0.33f;
        switch(direction)
        {
            case 0:
                pos.y += 0.66f;
                break;
            case 1:
                pos.x += 0.66f;
                pos.y += 0.66f;
                break;
            case 2:
                pos.x += 0.66f;
                break;
            case 3:
                pos.x += 0.66f;
                pos.y -= 0.66f;
                break;
            case 4:
                pos.y -= 0.66f;
                break;
            case 5:
                pos.x -= 0.66f;
                pos.y -= 0.66f;
                break;
            case 6:
                pos.x -= 0.66f;
                break;
            case 7:
                pos.x -= 0.66f;
                pos.y += 0.66f;
                break;
            default:
                Debug.Log("bad direction!");
                break;
        }
        this.transform.position = pos;
    }

    // FIXME: this doesn't seem to point the way I was expecting it to
    void SetDirection(int x, int y)
    {
        Vector3 pos = target.transform.position;
        int fromx = (int)pos.x;
        int fromy = (int)pos.y;
        float theta = Mathf.Atan2(x - fromx,y - fromy);
        int cardinal = (int)(theta * 8 / (2 * Mathf.PI) + 8) % 8;
        this.sprite.sprite = directionSprites[cardinal];
        float roundedAngle = (float)cardinal / 8f * (2f * Mathf.PI);
        pos.x += 0.75f * Mathf.Sin(roundedAngle);
        pos.y += 0.75f * Mathf.Cos(roundedAngle);
        transform.position = pos;
    }

    void Start()
    {
        this.sprite.sprite = healthSprites[6];
        gm.RegisterBlinkDelegate(delegate(int d)
            {
                if (this.timeout > 0f)
                    return true;
                if (this == null)
                    return false;
                if (gm.lm.GetPlayer().actor.entity.stats["hp"] <= 0)
                {
                    this.sprite.enabled = false;
                    return false;
                }
                if (target == null)
                {
                    Destroy(gameObject);
                    return false;
                }
                if (target != null)
                {
                    Vector3 pos = target.transform.position;
                    transform.position = pos;
                    ActorEntity e = target.GetComponent<ActorScript>().entity;
                    if (e.stats.ContainsKey("hp") && e.stats["hp"] < 1)
                    {
                        pos.y += 0.25f;
                        transform.position = pos;
                        this.sprite.sprite  = healthSprites[0];
                        this.sprite.enabled = true;
                    }
                    else if (d % 4 == 0)
                    {
                        if (e.stats.ContainsKey("hp"))
                        {
                            pos.y += 0.125f;
                            transform.position = pos;
                            int hp  = e.stats["hp"];
                            if (e.stats.ContainsKey("mhp"))
                            {
                                int mhp = e.stats["mhp"];
                                if (hp >= mhp)
                                    this.sprite.enabled = false;
                                else
                                {
                                    this.sprite.enabled = true;
                                    int idx = (int)( (float)healthSprites.Count / (mhp > 1? (float)mhp - 1f: 1f) * (float)hp );
                                    if (idx < healthSprites.Count)
                                        this.sprite.sprite = healthSprites[idx];
                                    else
                                        this.sprite.sprite = healthSprites[healthSprites.Count - 1];
                                }
                            }
                            else
                            {
                                this.sprite.enabled = true;
                                this.sprite.sprite = healthSprites[(hp < healthSprites.Count? hp: healthSprites.Count - 1)];
                            }
                        }
                        else
                        {
                            this.sprite.enabled = false;
                        }
                    }
                    else if (e.isPlayer && gm.lm.isOverworld && (d % 4 <= 2))
                    {
                        int wx = gm.getOverworldQuestX();
                        int wy = gm.getOverworldQuestY();

                        SetDirection(wx,wy);
                        this.sprite.enabled = true;
                    }
                    else if (e.isPlayer && gm.lm.isOverworld && (d % 4 == 3) && target.moving == false)
                    {
                        this.sprite.enabled = false;
                        pos = target.transform.position;
                        pos.x = gm.getOverworldQuestX();
                        pos.y = gm.getOverworldQuestY();
                        transform.position = pos;

                        this.sprite.sprite = directionSprites[8];
                        this.sprite.enabled = true;
                    }
                    else
                        this.sprite.enabled = false;
                }
                else
                    this.sprite.enabled = false;
                return true;
            }
        );
    }

}
