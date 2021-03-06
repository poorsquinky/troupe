﻿using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using ThugLib;

public class GameManagerScript : MonoBehaviour {

    public PlayerScript playerScript;
    [HideInInspector]
    public GameObject player;
    public GameObject playerPrefab;

    [HideInInspector]
    public LevelManagerScript lm;

    public Dictionary<string, LevelManagerScript> lmDict = new Dictionary<string, LevelManagerScript>();

    public GameObject dungeonLevelManagerPrefab;
    public GameObject circusLevelManagerPrefab;
    public GameObject overworldLevelManagerPrefab;
    public GameObject officeLevelManagerPrefab;

    public GameObject statusSpritePrefab;
    public GameObject itemPrefab;

    [HideInInspector]
    public bool menuActive = false;

    public int seed = 242;

    private GameObject ui_textoverlay;
    private GameObject ui_textcontent;

    private GameObject ui_actioncontent;

    private GameObject ui_messagecontent;
    private string     ui_messagebuffer;

    public float keyRepeatSpeed = 0.5f;
    public float currentKeyRepeat = 0f;

    [HideInInspector]
    public GameEntity entity = new GameEntity();

    [HideInInspector]
    public LevelEntity overworldEntity;

    //public int overworldX = 6;
    //public int overworldY = 39;
    [HideInInspector]
    public int overworldX = 10;
    [HideInInspector]
    public int overworldY = 10;

    public int turnCount = 0;

    public List<string> messageScrollback = new List<string>();

    [HideInInspector]
    public int questNumber = 0;

    private NameUtils nameUtils = new NameUtils();

    public int getOverworldQuestX() {
        return lm.entity.stats["quest_" + questNumber + "_x"];
    }
    public int getOverworldQuestY() {
        return lm.entity.stats["quest_" + questNumber + "_y"];
    }

    private const string helpText =
@"<b>MOVEMENT:</b>
 Arrows, gamepad and WASD (probably) work, but vi keys are recommended:
  <color=#cc99ffff>[A]</color> <color=#cc99ffff>[H]</color> - <color=#ffff66ff>Left</color>
  <color=#cc99ffff>[S]</color> <color=#cc99ffff>[J]</color> - <color=#ffff66ff>Down</color>
  <color=#cc99ffff>[W]</color> <color=#cc99ffff>[K]</color> - <color=#ffff66ff>Up</color>
  <color=#cc99ffff>[D]</color> <color=#cc99ffff>[L]</color> - <color=#ffff66ff>Right</color>
      <color=#cc99ffff>[Y]</color> - <color=#ffff66ff>Up/Left</color>
      <color=#cc99ffff>[U]</color> - <color=#ffff66ff>Up/Right</color>
      <color=#cc99ffff>[B]</color> - <color=#ffff66ff>Down/Left</color>
      <color=#cc99ffff>[N]</color> - <color=#ffff66ff>Down/Right</color>

<b>ACTIONS:</b>
      <color=#cc99ffff>[.]</color> - <color=#ffff66ff>Skip a move</color>
      <color=#cc99ffff>[/]</color> - <color=#ffff66ff>Toggle this help message (duh)</color>
<color=#cc99ffff>[SHIFT-S]</color> - <color=#ffff66ff>Save and quit (loading not implemented yet!) (KNOWN TO CRASH)</color>
<color=#cc99ffff>[SHIFT-Q]</color> - <color=#ffff66ff>Quit the game without saving (give up)</color>
";

    public int waypoint = -1;

    public bool IsOverworldActive() {
        if (lm.entity == overworldEntity)
            return true;
        return false;
    }

    public void UpdateOverworldCoords(int x, int y)
    {
        overworldX = x;
        overworldY = y;
    }

    int blinkDelta = 0;
    public delegate bool BlinkDelegate(int delta);
    public List<BlinkDelegate> blinkDelegates = new List<BlinkDelegate>();

    public void RegisterBlinkDelegate(BlinkDelegate d)
    {
        blinkDelegates.Add(d);
    }

    public void DoBlinkDelegates()
    {
        List<BlinkDelegate> newDelegates = new List<BlinkDelegate>();
        blinkDelta++;
        if (blinkDelta > 7)
            blinkDelta = 0;
        foreach (BlinkDelegate d in blinkDelegates)
            if (d(blinkDelta))
                newDelegates.Add(d);
        blinkDelegates = newDelegates;
    }

    void CheckRefs()
    {
        GameObject l = GameObject.Find("LevelManager");
        if (l)
            lm = l.GetComponent<LevelManagerScript>();
        if (playerScript == null)
        {
            GameObject p = GameObject.Find("player(Clone)");
            if (p)
            {
                this.playerScript    = p.GetComponent<PlayerScript>();
            }
        }
    }

    void Awake()
    {
        ui_actioncontent    = GameObject.Find("Text-top-left");
        ui_messagecontent = GameObject.Find("Text-bottom");

        InvokeRepeating("DoBlinkDelegates", 1, 0.125f);
    }

    public void Message(string msg)
    {
        messageScrollback.Add(msg);
        List<string> l = new List<string>(messageScrollback);
        l.Reverse();
        ui_messagecontent.GetComponent<Text>().text = System.String.Join("\n", l.ToArray());
    }

    void ToggleHelpMenu()
    {
        if (ui_textoverlay == null)
            ui_textoverlay = GameObject.Find("UITextOverlay");
        if (ui_textcontent == null)
            ui_textcontent = GameObject.Find("UITextContent");
        if (menuActive)
        {
            ui_textoverlay.GetComponent<CanvasGroup>().alpha = 0f;
            menuActive = false;
        }
        else
        {
            ui_textcontent.GetComponent<Text>().text = helpText;
            ui_textoverlay.GetComponent<CanvasGroup>().alpha = 1f;
            menuActive = true;
        }
    }

    // FIXME: menus only support numeric selection.  We really need to support pre-specified alpha options
    //        too, or maybe exclusively

    public delegate void MenuDelegate();

    public struct MenuCallback
    {
        public string menuText;
        public MenuDelegate menuDelegate;
        public MenuCallback(string s, MenuDelegate d)
        {
            menuText = s;
            menuDelegate = d;
        }
    }

    public List<string> actionCallbackText    = new List<string>();
    public List<Entity> actionCallbackTargets = new List<Entity>();

    public List<Entity.CallbackDelegate> actionCallbacks = new List<Entity.CallbackDelegate>();

    public void ClearActionCallbacks()
    {
        actionCallbackTargets.Clear();
        actionCallbackText.Clear();
        actionCallbacks.Clear();
    }
    public void AddActionCallback(string txt, Entity.CallbackDelegate d, Entity e)
    {
        actionCallbackTargets.Add(e);
        actionCallbackText.Add(txt);
        actionCallbacks.Add(d);
    }

    List<MenuCallback> menuCallbacks = new List<MenuCallback>();

    public void UpdateHelpDisplay()
    {

        if (playerScript != null)
        {
            if (IsOverworldActive())
            {
                ui_actioncontent.GetComponent<Text>().text = "Food: " + playerScript.actor.entity.stats["food"] + "  Performers: " + playerScript.CountPerformers();
            }
            else
            {
                if (actionCallbacks.Count == 0)
                {
                    ui_actioncontent.GetComponent<Text>().text = lm.entity.parent.longDescription;
                }
                else if (actionCallbacks.Count == 1)
                {
                    ui_actioncontent.GetComponent<Text>().text = "<color=#cc99ffff>[Space]</color><color=#ffff66ff> - " + actionCallbackText[0] + "</color>";
                }
                else
                {
                    ui_actioncontent.GetComponent<Text>().text = "<color=#cc99ffff>[Space]</color><color=#ffff66ff> - " + actionCallbackText.Count + " actions available</color>";
                }
            }
        }
    }

    public void DoAction()
    {
        if (actionCallbacks.Count == 0)
            return;
        else if (actionCallbacks.Count == 1)
            actionCallbacks[0](playerScript.actor.entity,actionCallbackTargets[0]);
    }

    public void CallbackMenu(string header, MenuCallback[] callbacks)
    {
        if (menuActive)
            return;

        string menuContent = header + "\n\n";
        menuCallbacks.Clear();
        int menuIdx = 1;
        foreach (MenuCallback mcb in callbacks)
        {
            menuCallbacks.Add(mcb);
            menuContent += "\n<color=#cc99ffff>[" + menuIdx + "]</color> "
                         + "<color=#ffff66ff>" + mcb.menuText + "</color>";
            menuIdx += 1;
        }

        if (ui_textoverlay == null)
            ui_textoverlay = GameObject.Find("UITextOverlay");
        if (ui_textcontent == null)
            ui_textcontent = GameObject.Find("UITextContent");

        ui_textcontent.GetComponent<Text>().text = menuContent;
        ui_textoverlay.GetComponent<CanvasGroup>().alpha = 1f;

        menuActive = true;
    }

    public delegate void InputDelegate(string e);

    private Dictionary<string, List<InputDelegate>> callbacks = new Dictionary<string, List<InputDelegate>>();

    public void RegisterCallback(string s, InputDelegate d)
    {
        if (callbacks.ContainsKey(s))
            callbacks[s].Add(d);
        else
            callbacks.Add(s, new List<InputDelegate> {d});
    }
    public void RegisterCallback(string[] s, InputDelegate d)
    {
        foreach (string i in s)
            RegisterCallback(i, d);
    }

    public void DeregisterCallback(string s, InputDelegate d)
    {
        if (callbacks.ContainsKey(s))
            foreach (InputDelegate this_d in callbacks[s])
                if (d == this_d)
                    callbacks[s].Remove(this_d);
    }
    public void DeregisterCallback(string[] s, InputDelegate d)
    {
        foreach (string i in s)
            DeregisterCallback(i, d);
    }

    private void CheckCallbacks(string s)
    {
        if (callbacks.ContainsKey(s))
            foreach (InputDelegate this_d in callbacks[s])
                this_d(s);
    }
    private void CheckCallbacks(char s)
    {
        CheckCallbacks(s.ToString());
    }

    private bool keyIsUp = true;
    void OnGUI()
    {

        CheckRefs();
        // FIXME: we need support for x/y axis input when controller is pressed

        bool shifted = false;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            shifted = true;
        if (!keyIsUp)
        {
            currentKeyRepeat -= Time.deltaTime;
            if (currentKeyRepeat <= 0f)
                keyIsUp = true;
        }
        if (Event.current.type == EventType.KeyUp)
            keyIsUp = true;
        else if ((keyIsUp) && (Event.current.type == EventType.KeyDown))
        {
            currentKeyRepeat = keyRepeatSpeed;
            keyIsUp = false;
            if (Event.current.keyCode == KeyCode.Slash)
                ToggleHelpMenu();
            if (Event.current.isKey)
            {
                string s = Event.current.keyCode.ToString();
                if (shifted)
                    s = s.ToUpper();
                else
                    s = s.ToLower();
                if (!menuActive)
                    CheckCallbacks(shifted? s.ToUpper() : s.ToLower());
                else
                {
                    // FIXME: it's silly to use a switch for this, and completely sucks that we're stuck
                    //        with 1-9 only
                    int i = -1;
                    switch (s)
                    {
                        case "alpha1":
                        case "keypad1":
                            i = 1;
                            break;
                        case "alpha2":
                        case "keypad2":
                            i = 2;
                            break;
                        case "alpha3":
                        case "keypad3":
                            i = 3;
                            break;
                        case "alpha4":
                        case "keypad4":
                            i = 4;
                            break;
                        case "alpha5":
                        case "keypad5":
                            i = 5;
                            break;
                        case "alpha6":
                        case "keypad6":
                            i = 6;
                            break;
                        case "alpha7":
                        case "keypad7":
                            i = 7;
                            break;
                        case "alpha8":
                        case "keypad8":
                            i = 8;
                            break;
                        case "alpha9":
                        case "keypad9":
                            i = 9;
                            break;
                    }
                    if (i > 0 && i <= menuCallbacks.Count)
                    {
                        ui_textoverlay.GetComponent<CanvasGroup>().alpha = 0f;
                        menuActive = false;
                        menuCallbacks[i - 1].menuDelegate();
                    }
                }
            }
        }
    }

    void SetupBasicInputCallbacks()
    {
        // set up our callback delegates
        // up
        RegisterCallback(new[] {"k","w","uparrow"}, delegate(string e)
        {
            CheckRefs();
            if (lm == null || lm.active == false) return;
            playerScript.keyboardX = 0;
            playerScript.keyboardY = 1;
        });
        // left
        RegisterCallback(new[] {"h","a","leftarrow"}, delegate(string e)
        {
            CheckRefs();
            if (lm == null || lm.active == false) return;
            playerScript.keyboardX = -1;
            playerScript.keyboardY = 0;
        });
        // down
        RegisterCallback(new[] {"j","s","downarrow"}, delegate(string e)
        {
            CheckRefs();
            if (lm == null || lm.active == false) return;
            playerScript.keyboardX = 0;
            playerScript.keyboardY = -1;
        });
        // right
        RegisterCallback(new[] {"l","d","rightarrow"}, delegate(string e)
        {
            CheckRefs();
            if (lm == null || lm.active == false) return;
            playerScript.keyboardX = 1;
            playerScript.keyboardY = 0;
        });
        // down/left
        RegisterCallback("b", delegate(string e)
        {
            CheckRefs();
            if (lm == null || lm.active == false) return;
            playerScript.keyboardX = -1;
            playerScript.keyboardY = -1;
        });
        // up/left
        RegisterCallback("y", delegate(string e)
        {
            CheckRefs();
            if (lm == null || lm.active == false) return;
            playerScript.keyboardX = -1;
            playerScript.keyboardY = 1;
        });
        // up/right
        RegisterCallback("u", delegate(string e)
        {
            CheckRefs();
            if (lm == null || lm.active == false) return;
            playerScript.keyboardX = 1;
            playerScript.keyboardY = 1;
        });
        // down/right
        RegisterCallback("n", delegate(string e)
        {
            CheckRefs();
            if (lm == null || lm.active == false) return;
            playerScript.keyboardX = 1;
            playerScript.keyboardY = -1;
        });

        // skip turn
        RegisterCallback("period", delegate(string e)
        {
            CheckRefs();
            if (lm == null || lm.active == false) return;
            if (lm)
                lm.playerTurn = false;
        });

        // action menu
        RegisterCallback("space", delegate(string e)
        {
            DoAction();
        });

        // quit
        RegisterCallback("Q", delegate(string e)
        {
            CallbackMenu(
                "Are you sure you want to quit without saving?",
                new[] {
                    new MenuCallback("Yes", delegate() { Application.Quit(); }),
                    new MenuCallback("No", delegate() { return; })
                }
            );
        });

        /*
        RegisterCallback("S", delegate(string e)
        {
            CallbackMenu(
                "Are you sure you want to save and quit?",
                new[] {
                    new MenuCallback("Yes", delegate() {
                        Debug.Log(Application.persistentDataPath + @"/save-file.json");
//                        File.WriteAllText(Application.persistentDataPath + @"/save-file.json", this.entity.Serialize());
                    }),
                    new MenuCallback("No", delegate() { return; })
                }
            );
        });
        */
    }

    void RefreshTiles()
    {
        for (int i = 0; i < lm.levelWidth; i++)
        {
            for (int j = 0; j < lm.levelHeight; j++)
            {
                GameObject tile = lm.GetTile(i,j);
                if (tile != null)
                    tile.GetComponent<ShapeTerrainScript>().ExternalUpdate();
            }
        }
    }

    public void ActivateOffice(PlaceEntity place, int level)
    {
        if (lm)
            lm.Deactivate();
        Destroy(lm.gameObject);
        lm = null;
        GameObject l = Instantiate(officeLevelManagerPrefab) as GameObject;
        lm = l.GetComponent<LevelManagerScript>();
        lmDict["office"] = lm;
        if (place.levels.Count <= level)
        {
            lm.init_office(place);
        }
        else
        {
            lm.load_office(place.levels[level]);
        }
        lm.Activate();
        RefreshTiles();
    }

    public void ActivateCircus(PlaceEntity place)
    {

        

        // we're short-circuiting the circus activation because of time

        /*
        // create the level manager if needed
        if (lm)
            lm.Deactivate();
        GameObject l = Instantiate(circusLevelManagerPrefab) as GameObject;
        lm = l.GetComponent<LevelManagerScript>();
        lmDict["circus"] = lm;

        // create the level entities if needed
        if (place.levels.Count == 0)
        {
            lm.init_circus(place);
        }
        else
        {
            lm.load_circus(place.levels[0]);
        }
        
        lm.Activate();
        RefreshTiles();

        if (playerScript != null)
        {
            if (playerScript.actor.entity.GetHP() < playerScript.actor.entity.GetMHP())
            {
                playerScript.actor.entity.SetHP(playerScript.actor.entity.GetMHP());
                Message("You drink some snake oil to restore your health.");
            }
        }
        */

        if (playerScript != null)
        {
            if (playerScript.actor.entity.GetHP() < playerScript.actor.entity.GetMHP())
            {
                playerScript.actor.entity.SetHP(playerScript.actor.entity.GetMHP());
                Message("You drink some snake oil to restore your health.");
            }
        }

        if (place.subPlaces.Count < 1)
        {
            PlaceEntity office      = new PlaceEntity();
            office.shortDescription = "office";
            office.longDescription  = "the " + nameUtils.RandomCompanyName() + " building";
            office.placeType        = "office";
            office.index            = this.entity.RegisterEntity(office);
            place.AddSubPlace(office);
        }

        List<MenuCallback> menuItems = new List<MenuCallback>();
//                        Debug.Log(place);
        foreach (PlaceEntity subPlace in place.subPlaces)
        {
            menuItems.Add(new GameManagerScript.MenuCallback("Visit " + subPlace.longDescription, delegate() { lm.gm.ActivateOffice(subPlace,0); }));
        }
        menuItems.Add(new MenuCallback("Return to world map", ActivateOverworld ));
        string menuText = "Welcome to " + place.shortDescription;

        if (waypoint < 0)
        {
            menuText = "You arrive outside of " + place.shortDescription + ".";
            // FIXME TBD
            waypoint++;
        }

        lm.gm.CallbackMenu(
            menuText,
            menuItems.ToArray()
        );


    }

    public void ActivateOverworld()
    {
        if (lm)
        {
            lm.Deactivate();
            Destroy(lm.gameObject);
        }
        GameObject l = Instantiate(overworldLevelManagerPrefab) as GameObject;
        lm = l.GetComponent<LevelManagerScript>();
        lm.init_overworld(overworldEntity);
        this.overworldEntity = lm.entity;
        lmDict["overworld"] = lm;
        lm.Activate();
        RefreshTiles();
        if (waypoint < 0)
            ActivateCircus(lm.entity.GetCell(overworldX,overworldY).place);
//        Debug.Log(lm.entity.parent);
//        playerScript.ForceMoveTo(overworldX,overworldY);
    }

    /*
    void SwitchToLevel(string levelName)
    {
        lm.Deactivate();
        // FIXME: stop being stupid here
        if (levelName == "overworld")
        {
            ActivateOverworld();
        }
        else if (levelName == "circus")
        {
            ActivateCircus();
        }
    }
    */

    void Start()
    {
        this.player = Instantiate(this.playerPrefab) as GameObject;

        // Create the camera
        GameObject cam = GameObject.Find("Camera");
        Vector3 pos = this.player.transform.position;
        pos.z = -10;
        cam.transform.position = pos;
        cam.GetComponent<CameraScript>().target = player.transform;

        //ActivateCircus();
        ActivateOverworld();

        SetupBasicInputCallbacks();

    }
}
