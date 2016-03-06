using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ThugLib;
using ThugSimpleGame;

public class NPCScript : MonoBehaviour {
    public LevelManagerScript lm; // LevelManager
    public GameManagerScript gm;
    public ActorScript actor;

    public float timeUnitsPerTurn = 0.75f;

    public Path currentPath;
    public int currentPathStep;

/*
    public Dictionary<string,NPCBrain> brain;

    public void RunBrain()
    {
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

    void Awake ()
    {
        GameObject g = GameObject.Find("GameManager");
        gm = g.GetComponent<GameManagerScript>();
        lm = gm.lm;
        this.actor = GetComponent<ActorScript>();
    }

    void PlaceMeRandomly ()
    {
        // FIXME: replace this with something less stabby-in-the-dark
        int x = Random.Range(0,lm.levelWidth);
        int y = Random.Range(0,lm.levelHeight);
        while (!actor.CanMoveTo(x,y))
        {
            x = Random.Range(0,lm.levelWidth);
            y = Random.Range(0,lm.levelHeight);
        }
        actor.TeleportTo(x,y);
    }

    void Start () {
        PlaceMeRandomly();
        this.actor.entity.AddRefreshDelegate(delegate() {
            // TODO: have this do stuff
        });
    }

    public void Move ()
    {
        RunBrain();
    }
    */
}
