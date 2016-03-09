using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ThugLib;
using ThugSimpleGame;
using System.IO;

public class LevelManagerScript : MonoBehaviour {

    public int levelWidth=200;
    public int levelHeight=200;

    public GameObject npcPrefab;

    private List<List<GameObject>> tileGrid;
    private List<List<GameObject>> subTileGrid;

    [HideInInspector]
    public MapRectangle fullMapBounds;

    [HideInInspector]
    public CellEntity[][] map;

    [HideInInspector]
    public MapManager mapman;

    private bool[][] visibility_map;

    [HideInInspector]
    public List<GameObject> npcs = new List<GameObject>();

    [HideInInspector]
    public bool active = false;

    [HideInInspector]
    public LevelEntity entity;

    [HideInInspector]
    public GameManagerScript gm;

    private bool IsVisibleAndBlocked(int x, int y) {
        return (x >= 0 && x < levelWidth && y >= 0 && y < levelHeight &&
            visibility_map[x][y] && !map[x][y].Passable());
    }

    [System.Serializable]
    public struct namedGameObject
    {
        public string name;
        public GameObject gameObject;
    }
    public List<namedGameObject> gameObjectTypes;
    public Dictionary<string, GameObject> gameObjects = new Dictionary<string, GameObject>();

    [System.Serializable]
    public struct spriteEntry
    {
        public string name;
        public Sprite neutral;
        public Sprite hostile;
    }
    public List<spriteEntry> spriteEntries;
    public Dictionary<string, Sprite[]> sprites = new Dictionary<string, Sprite[]>();

    void Awake ()
    {
        foreach (namedGameObject go in gameObjectTypes)
            gameObjects[go.name] = go.gameObject;

        foreach (spriteEntry go in spriteEntries)
            sprites[go.name] = new Sprite[] { go.neutral, go.hostile };

        GameObject g = GameObject.Find("GameManager");
        gm = g.GetComponent<GameManagerScript>();
    }

    void BuildVisibilityMap()
    {

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
                            y < levelHeight && map[x][y].Passable())
                        {
                            keepLooping = false;
                            visibility_map[i][j] = true;
                        }
                    }
                }
            }
        }
    }

    void CreateSprites()
    {
        tileGrid = new List<List<GameObject>>();
        subTileGrid = new List<List<GameObject>>();
        for (int i = 0; i < levelHeight; i++)
        {
            tileGrid.Add(new List<GameObject>());
            subTileGrid.Add(new List<GameObject>());
            for (int j = 0; j < levelWidth; j++)
            {

                TerrainEntity    terrain = map[j][i].GetTerrain();
                PropEntity          prop = map[j][i].GetProp();

                if (! gameObjects.ContainsKey(terrain.shortDescription))
                {
                    Debug.Log("TERRAIN NOT FOUND: " + terrain.shortDescription);
                    terrain.shortDescription = "wall";
                }
                GameObject terrainObject = gameObjects[terrain.shortDescription];

                GameObject propObject = null;
                if (prop != null)
                {
                    if (! gameObjects.ContainsKey(prop.shortDescription))
                    {
                        Debug.Log("PROP NOT FOUND: " + prop.shortDescription);
                        prop.shortDescription = "wall";
                    }
                    propObject = gameObjects[prop.shortDescription];
                }

                GameObject             o = Instantiate(terrainObject) as GameObject;
                o.transform.position     = new Vector3(j, i, 0);
                ShapeTerrainScript   sts = o.GetComponent<ShapeTerrainScript>();

                if (sts)
                {
                    CellEntity       cellEntity = map[j][i];
                    TerrainEntity terrainEntity = cellEntity.GetTerrain();
                    sts.SetCell(cellEntity);
                    /*
                    if (map[j][i].Passable())
                        sts.entity.hindrance = 0f;
                    else
                        sts.entity.hindrance = 1f;
                    */

                    // FIXME: all the shape terrain stuff should go in the ShapeTerrainScript and not here
                    if (sts.terrainShapeStyle == ShapeTerrainScript.ShapeTypes.SameSpaceType)
                    {
                        sts.SetSprite(0 |
                                ((i < levelHeight - 1 && terrainEntity.shortDescription == map[j  ][i+1].GetTerrain().shortDescription)? 1 : 0) |
                                ((j < levelWidth - 1  && terrainEntity.shortDescription == map[j+1][i  ].GetTerrain().shortDescription)? 2 : 0) |
                                ((i > 0               && terrainEntity.shortDescription == map[j  ][i-1].GetTerrain().shortDescription)? 4 : 0) |
                                ((j > 0               && terrainEntity.shortDescription == map[j-1][i  ].GetTerrain().shortDescription)? 8 : 0)
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

                if (propObject != null)
                {
                    GameObject         po = Instantiate(propObject) as GameObject;
                    po.transform.position = new Vector3(j, i, 0);
                    subTileGrid[i].Add(po);
                }
                else
                {
                    subTileGrid[i].Add(null);
                }
            }
        }
    }

    void DestroySprites()
    {

        while (tileGrid.Count > 0)
        {
            List<GameObject> l = tileGrid[0];
            while (l.Count > 0)
            {
                GameObject t = l[0];
                if (t != null)
                    Destroy(t);
                l.RemoveAt(0);
            }
            tileGrid.RemoveAt(0);
        }

        while (subTileGrid.Count > 0)
        {
            List<GameObject> l = subTileGrid[0];
            while (l.Count > 0)
            {
                GameObject t = l[0];
                if (t != null)
                    Destroy(t);
                l.RemoveAt(0);
            }
            subTileGrid.RemoveAt(0);
        }

    }

    public void Deactivate()
    {
        DestroySprites();
    }

    public void Activate()
    {
        // create all of the sprites
        CreateSprites();

        PlayerScript playerScript = gm.player.GetComponent<PlayerScript>();
        playerScript.lm = this;
        playerScript.ForceMoveTo(25,40);

        this.active = true;
    }

    void init_common()
    {
        init_common(25,40);
    }

    void init_common(int player_x, int player_y) {
        PlayerScript playerScript = gm.player.GetComponent<PlayerScript>();
        playerScript.lm = this;
        playerScript.ForceMoveTo(player_x,player_y);

        BuildVisibilityMap();

        // Add the player to the schedule
        ScheduleItem playersched = new ScheduleItem(gm.player.GetComponent<PlayerScript>().timeUnitsPerTurn, null);
        scheduleOutbox.Add(playersched);
    }

    void ProcessNPCs()
    {
        // FIXME should be able to do a tag search here instead
        for (int x = 0; x < levelWidth; x++)
        {
            for (int y = 0; y < levelHeight; y++)
            {
                CellEntity cell         = this.entity.GetCell(x,y);
                ActorEntity actorEntity = cell.GetActor();
                if (actorEntity != null)
                {
                    GameObject npc    = Instantiate(npcPrefab);
                    ActorScript actor = npc.GetComponent<ActorScript>();
                    actor.entity      = actorEntity;
                    actor.TeleportTo(x,y);
                    npcs.Add(npc);

                    Sprite sprite = sprites["person"][0];
                    if (actorEntity.attrs.ContainsKey("sprite"))
                    {
                        string spriteName = actorEntity.attrs["sprite"];
                        if (sprites.ContainsKey(spriteName))
                        {
                            if (actorEntity.attrs.ContainsKey("hostile") && actorEntity.attrs["hostile"] != "false")
                            {
                                sprite = sprites[actorEntity.attrs["sprite"]][1];
                            }
                            else
                            {
                                sprite = sprites[actorEntity.attrs["sprite"]][0];
                            }
                        }
                    }
                    actor.SetSprite(sprite);
//            monkey.attrs["brain"]   = "random";

                    ScheduleItem sched = new ScheduleItem(npc.GetComponent<NPCScript>().timeUnitsPerTurn, npc);
                    scheduleOutbox.Add(sched);
                }
            }
        }
    }

    public void init_overworld() {
        levelWidth=50;
        levelHeight=50;
        this.entity       = new LevelEntity(levelWidth, levelHeight, gm.entity);
        this.entity.index = gm.entity.RegisterEntity(this.entity);

        mapman = new TroupeOverworldMapManager(this);
        mapman.Generate();

        // uncomment all of this stuff in order to test serialize/deserialize
        /*
        this.entity.Serialize();
        string serialized = JsonUtility.ToJson(this.entity);
        File.WriteAllText("save-file.json", serialized);

        this.entity.Decommission(gm.entity);

        this.entity = null;
        mapman      = null;

        this.entity = JsonUtility.FromJson<LevelEntity>(serialized);
        this.entity.index = gm.entity.RegisterEntity(this.entity);
        this.entity.Deserialize();

        mapman = new TroupeOverworldMapManager(this);
        */

        ProcessNPCs();
        mapman.PostProcess();
        init_common(10,10);
    }

    public void init_office() {
        levelWidth=50;
        levelHeight=50;
        this.entity       = new LevelEntity(levelWidth, levelHeight, gm.entity);
        this.entity.index = gm.entity.RegisterEntity(this.entity);

        mapman = new TroupeOfficeMapManager(this);

        mapman.Generate();
        mapman.PostProcess();
        init_common();
    }

    public void init_circus() {
        levelWidth=50;
        levelHeight=50;
        this.entity       = new LevelEntity(levelWidth, levelHeight, gm.entity);
        this.entity.index = gm.entity.RegisterEntity(this.entity);

        mapman = new TroupeCircusMapManager(this);

        mapman.Generate();
        ProcessNPCs();
        mapman.PostProcess();
        init_common();
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
        return gm.player.GetComponent<PlayerScript>().actor.GetX();
    }
    public int GetPlayerY()
    {
        return gm.player.GetComponent<PlayerScript>().actor.GetY();
    }
    private bool[][] wasEverVisible = null;

    public void VisibilityUpdate() {
        isFirstUpdate = false;
        lastVisibilityUpdatePlayerPos = gm.player.transform.position;

        // generate a visibility map for the entire level

        int playerX = MathUtils.RoundToInt(lastVisibilityUpdatePlayerPos.x);
        int playerY = MathUtils.RoundToInt(lastVisibilityUpdatePlayerPos.y);
        PathUtils.CalculateBresenhamProductsToRectangle(
                fromX: playerX,
                fromY: playerY,
                map: map,
                rectangle: fullMapBounds,
                update: (previous, cell) => ((previous == 0 || !((CellEntity)cell).Visible()) ? 0 : 1),
                startingProduct: 1,
                includeFirstSquare: false,
                includeLastSquare: false,
                outputMap: latestVisibility);

        foreach (GameObject npc in npcs)
        {
            ActorScript actor = npc.GetComponent<ActorScript>();
            int x = actor.GetX();
            int y = actor.GetY();
//            npc.GetComponent<SpriteRenderer>().enabled = true;
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

    [HideInInspector]
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
                    item.speed = gm.player.GetComponent<PlayerScript>().timeUnitsPerTurn;
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
