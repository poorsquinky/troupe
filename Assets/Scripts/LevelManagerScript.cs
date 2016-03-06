using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ThugLib;
using ThugSimpleGame;

public class LevelManagerScript : MonoBehaviour {

    public int levelWidth=200;
    public int levelHeight=200;

    public GameObject playerPrefab;

    public GameObject npcPrefab;

    public List<GameObject> tilePrefabs;

    private List<List<GameObject>> tileGrid = new List<List<GameObject>>();
    private List<List<GameObject>> subTileGrid = new List<List<GameObject>>();
    private GameObject player;

    public MapRectangle fullMapBounds;
    public MapSpaceType[][] map;
    public MapManager mapman;

    private bool[][] visibility_map;

    public List<GameObject> npcs = new List<GameObject>();

    public bool active = false;

    public LevelEntity entity;

    private GameManagerScript gm;

    private bool IsVisibleAndBlocked(int x, int y) {
        return (x >= 0 && x < levelWidth && y >= 0 && y < levelHeight &&
            visibility_map[x][y] && !map[x][y].passable);
    }

    void Awake ()
    {
        GameObject g = GameObject.Find("GameManager");
        gm = g.GetComponent<GameManagerScript>();
    }

    void go() {
        // Find a starting spot for the player
        // FIXME: this should be loaded from last location if applicable, otherwise set at map gen time
        int player_x = 25;
        int player_y = 38;

        fullMapBounds = new MapRectangle(0, 0, levelWidth, levelHeight);
        map = mapman.GetPatch(fullMapBounds);

        // build the visibility map for this level
        visibility_map = new bool[levelHeight][];
        for (int i = 0; i < levelWidth; i++)
        {
            visibility_map[i] = new bool[levelWidth];
            for (int j = 0; j < levelHeight; j++)
            {
                visibility_map[i][j] = false;
                bool keepLooping = true;
                for (int x = i-1; keepLooping && x <= i+1; x++)
                {
                    for (int y = j-1; keepLooping && y <= j+1; y++)
                    {
                        if (x >= 0 && x < levelWidth && y >= 0 &&
                            y < levelHeight && map[x][y].passable)
                        {
                            keepLooping = false;
                            visibility_map[i][j] = true;
                        }
                    }
                }
            }
        }

        // create all of the sprites
        for (int i = 0; i < levelHeight; i++)
        {
            tileGrid.Add(new List<GameObject>());
            subTileGrid.Add(new List<GameObject>());
            for (int j = 0; j < levelWidth; j++)
            {

                // XXX XXX XXX put the sprite objects under each cell as TerrainEntity objects


                // XXX this is profoundly dumb -- we're just assigning the player coords to the last passable square.  But it's temporary.
                /*
                if (map[j][i].passable)
                {
                    player_x = j;
                    player_y = i;
                }
                */

                subTileGrid[i].Add(null);

                var o = Instantiate(this.tilePrefabs[map[j][i].key]) as GameObject;
                o.transform.position = new Vector3(j, i, 0);
                var sts = o.GetComponent<ShapeTerrainScript>();
                if (sts)
                {
                    sts.SetCell(this.entity.GetCell(j,i));
                    if (map[j][i].passable)
                        sts.entity.hindrance = 0f;
                    else
                        sts.entity.hindrance = 1f;

                    // FIXME: all the shape terrain stuff should go in the ShapeTerrainScript and not here
                    if (sts.terrainShapeStyle == ShapeTerrainScript.ShapeTypes.SameSpaceType)
                    {
                        sts.SetSprite(0 |
                                ((i < levelHeight - 1 && map[j][i].key == map[j][i+1].key)? 1 : 0) |
                                ((j < levelWidth - 1  && map[j][i].key == map[j+1][i].key)? 2 : 0) |
                                ((i > 0               && map[j][i].key == map[j][i-1].key)? 4 : 0) |
                                ((j > 0               && map[j][i].key == map[j-1][i].key)? 8 : 0)
                                );
                    }
                    else if (sts.terrainShapeStyle == ShapeTerrainScript.ShapeTypes.VisibleAndImpassable)
                    {
                        sts.SetSprite(
                           (IsVisibleAndBlocked(j,     i + 1) ? 1 : 0) |
                           (IsVisibleAndBlocked(j + 1, i    ) ? 2 : 0) |
                           (IsVisibleAndBlocked(j,     i - 1) ? 4 : 0) |
                           (IsVisibleAndBlocked(j - 1, i    ) ? 8 : 0));
                        if (!visibility_map[j][i])
                            o.GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
                tileGrid[i].Add(o);
            }
        }

        // Create the player
        this.player = Instantiate(this.playerPrefab) as GameObject;

        PlayerScript playerScript = this.player.GetComponent<PlayerScript>();
        playerScript.ForceMoveTo(player_x, player_y);

        /*
        // Add some placeholder NPCs
        for (int i = 0; i < 10; i++)
        {
            GameObject npc = Instantiate(npcPrefab);
            npcs.Add(npc);

            ScheduleItem sched = new ScheduleItem(npc.GetComponent<NPCScript>().timeUnitsPerTurn, npc);
            scheduleOutbox.Add(sched);

            ActorScript actor = npc.GetComponent<ActorScript>();

            int x = Random.Range(0,levelWidth);
            int y = Random.Range(0,levelHeight);
            CellEntity npcCell = this.entity.GetCell(x,y);
            while (true)
            {
                if (
                        (npcCell != null)
                     && (npcCell.GetHindrance() < 1f)
                   )
                    break;
                x = Random.Range(0,levelWidth);
                y = Random.Range(0,levelHeight);
                npcCell = this.entity.GetCell(x,y);
            }

            npcCell.ActorForceEnter(actor.entity);
        }
        */

        // Create the camera
        GameObject cam = GameObject.Find("Camera");
        Vector3 pos = new Vector3(player_x, player_y, -10);
        cam.transform.position = pos;
        cam.GetComponent<CameraScript>().target = this.player.transform;

        // Add the player to the schedule
        ScheduleItem playersched = new ScheduleItem(player.GetComponent<PlayerScript>().timeUnitsPerTurn, null);
        scheduleOutbox.Add(playersched);

        this.active = true;
    }


    public void init_overworld() {
        this.entity       = new LevelEntity(levelWidth, levelHeight, gm.entity);
        this.entity.index = gm.entity.RegisterEntity(this.entity);

        mapman = new TroupeOverworldMapManager(this.entity);
        go();
    }
    public void init_circus() {
        levelWidth=50;
        levelHeight=50;
        this.entity       = new LevelEntity(levelWidth, levelHeight, gm.entity);
        this.entity.index = gm.entity.RegisterEntity(this.entity);

        mapman = new TroupeCircusMapManager(this.entity);
        go();
    }
    public void init_simple() {
        this.entity       = new LevelEntity(levelWidth, levelHeight, gm.entity);
        this.entity.index = gm.entity.RegisterEntity(this.entity);

        mapman = new SimpleMapManager(this.entity);
        go();
    }

    private Vector3 lastVisibilityUpdatePlayerPos;
    private bool isFirstUpdate = true;
    private int[][] latestVisibility = null;
    public bool IsVisibleToPC(int x, int y)
    {
        if (latestVisibility != null)
        {
            return (latestVisibility[x][y] == 1);
        }
        else
        {
            return false;
        }
    }
    public int GetPlayerX()
    {
        return player.GetComponent<PlayerScript>().actor.GetX();
    }
    public int GetPlayerY()
    {
        return player.GetComponent<PlayerScript>().actor.GetY();
    }
    private bool[][] wasEverVisible = null;

    public void VisibilityUpdate() {
        isFirstUpdate = false;
        lastVisibilityUpdatePlayerPos = this.player.transform.position;

        // generate a visibility map for the entire level

        int playerX = MathUtils.RoundToInt(lastVisibilityUpdatePlayerPos.x);
        int playerY = MathUtils.RoundToInt(lastVisibilityUpdatePlayerPos.y);
        PathUtils.CalculateBresenhamProductsToRectangle(playerX, playerY,
           map, fullMapBounds, (previous, tile) => 
// FIXME: ---ENTITY_UPDATE---
           ((previous == 0 || !((MapSpaceType)tile).transparent) ?
           0 : 1), 1, false, false, latestVisibility);

        foreach (GameObject npc in npcs)
        {
            ActorScript actor = npc.GetComponent<ActorScript>();
            int x = actor.GetX();
            int y = actor.GetY();
            npc.GetComponent<SpriteRenderer>().enabled = 
               (latestVisibility[x][y] == 1);
        }

        // load the current player visibility map into the tileGrid

        for (int i = 0; i < fullMapBounds.w; i++)
        {
            for (int j = 0; j < fullMapBounds.h; j++)
            {
                if (wasEverVisible[i][j])
                    tileGrid[j][i].GetComponent<SpriteRenderer>().color = new Color(0.25f,0.75f,1f,0.75f);
                else
                    tileGrid[j][i].GetComponent<SpriteRenderer>().color = new Color(1f,1f,1f,0f);
                if (subTileGrid[j][i] != null)
                {
                    if (wasEverVisible[i][j])
                        subTileGrid[j][i].GetComponent<SpriteRenderer>().color = new Color(0.25f,0.75f,1f,0.75f);
                    else
                        subTileGrid[j][i].GetComponent<SpriteRenderer>().color = new Color(1f,1f,1f,0f);
                }
            }
        }
        for (int i = 0; i < fullMapBounds.w; i++)
        {
            for (int j = 0; j < fullMapBounds.h; j++)
            {
                if (latestVisibility[i][j] == 1)
                {
                    for (int di = -1; di <= 1; di++) 
                    {
                        for (int dj = -1; dj <= 1; dj++) 
                        {
                            if (i + di >= 0 && j + dj >= 0 &&
                               j + dj < tileGrid.Count &&
                               i + di < tileGrid[0].Count)
                            {
                                wasEverVisible[i + di][j + dj] = true;
                                tileGrid[j + dj][i + di].GetComponent<SpriteRenderer>().color = new Color(1f,1f,1f,1f);
                                if (subTileGrid[j + dj][i + di] != null)
                                {
                                    subTileGrid[j + dj][i + di].GetComponent<SpriteRenderer>().color = new Color(1f,1f,1f,1f);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public bool playerTurn = true;

    struct ScheduleItem
    {
        public float speed;        // speed is expressed in Moves Per Arbitrary Time Unit
        public float timer;        // how much time is left before this item activates
        public GameObject npc;      // the NPC Script in question, or null if it's the player

        public ScheduleItem(float s, GameObject n)
        {
            speed = s;
            timer = s;
            npc   = n;
        }
    }

    private float timeDecrement = 0f;
    private List<ScheduleItem> scheduleOutbox = new List<ScheduleItem>();
    private List<ScheduleItem> scheduleInbox  = new List<ScheduleItem>();

    private void DoSchedule()
    {
        if (scheduleInbox.Count == 0)
        {
            scheduleInbox = scheduleOutbox;
            scheduleOutbox = new List<ScheduleItem>();
            timeDecrement = -1f;
            foreach (ScheduleItem item in scheduleInbox)
                if ((timeDecrement < 0f) || (timeDecrement > item.timer))
                    timeDecrement = item.timer;
            if (timeDecrement <= 0)
                timeDecrement = 0;
        } else {
            ScheduleItem item = scheduleInbox[0];
            scheduleInbox.RemoveAt(0);
            item.timer -= timeDecrement;
            if (item.timer <= 0f)
            {
                if (item.npc)
                {
                    item.npc.GetComponent<NPCScript>().Move();
                    item.speed = item.npc.GetComponent<NPCScript>().timeUnitsPerTurn;
                }
                else
                {
                    playerTurn = true;
                    item.speed = player.GetComponent<PlayerScript>().timeUnitsPerTurn;
                }
                item.timer += 1f / item.speed;
            }
            scheduleOutbox.Add(item);
        }
    }

    public GameObject GetTile(int x, int y)
    {
        if (x > 0 && x < levelWidth && y > 0 && y < levelHeight)
            return tileGrid[x][y];
        return null;
    }

    // Update is called once per frame
    void Update () {
        if (active == false) return;
        if (latestVisibility == null)
        {
            latestVisibility = new int[fullMapBounds.w][];
            wasEverVisible = new bool[fullMapBounds.w][];
            for (int i = 0; i < fullMapBounds.w; i++)
            {
                latestVisibility[i] = new int[fullMapBounds.h];
                wasEverVisible[i] = new bool[fullMapBounds.h];
            }
        }
        if (isFirstUpdate)
        {
            VisibilityUpdate();
        }

        if (! playerTurn)
            DoSchedule();
    }
}
