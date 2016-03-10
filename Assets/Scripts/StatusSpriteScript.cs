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

    void Start()
    {
        this.sprite.sprite = healthSprites[6];
        gm.RegisterBlinkDelegate(delegate(int d)
            {
                if (this == null)
                    return false;
                if (target == null)
                {
                    Destroy(gameObject);
                    return false;
                }
                if (d % 4 == 0 && target != null)
                {
                    ActorEntity e = target.GetComponent<ActorScript>().entity;
                    if (e.stats.ContainsKey("hp"))
                    {
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
                                this.sprite.sprite = healthSprites[idx];
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
                else
                    this.sprite.enabled = false;
                return true;
            }
        );
    }

}
