using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ThugLib;
using ThugSimpleGame;

public class NPCScript : MonoBehaviour {
    [HideInInspector]
    public LevelManagerScript lm; // LevelManager
    [HideInInspector]
    public GameManagerScript gm;
    [HideInInspector]
    public ActorScript actor;

    public float timeUnitsPerTurn = 0.75f;

    public Path currentPath;
    public int currentPathStep;

    void Awake ()
    {
        GameObject g = GameObject.Find("GameManager");
        gm = g.GetComponent<GameManagerScript>();
        lm = gm.lm;

        this.actor = GetComponent<ActorScript>();
    }

    public Dictionary<string,NPCBrain> brain;

    public void RunBrain()
    {
        if (actor.entity.stats.ContainsKey("hp") && actor.entity.stats["hp"] < 1)
            return;
        if (brain == null)
        {
            brain = new Dictionary<string,NPCBrain>();
            int brainType = Random.Range(0, 3);
            switch (brainType)
            {
                case 0: brain["patrol"] = new NPCPatrolBrain(this); break;
                case 1: brain["chase"] = new NPCChaseBrain(this); 
                   brain["random"] = new NPCRandomBrain(this);
                   brain["random"].active = false;
                   break;
                case 2: brain["random"] = new NPCRandomBrain(this); break;
            }
        }
        foreach (string s in brain.Keys)
        {
            if (brain[s].active) brain[s].Run();
        }
    }

    /*
    void Start () {
        this.actor.entity.AddRefreshDelegate(delegate() {
            // TODO: have this do stuff
        });
    }
    */

    public void Move ()
    {
        RunBrain();
    }
}
