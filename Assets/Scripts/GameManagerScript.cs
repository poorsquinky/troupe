﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManagerScript : MonoBehaviour {

    private PlayerScript player;
    public LevelManagerScript lm;

    public GameObject dungeonLevelManager;
    public GameObject outdoorLevelManager;

    public bool menuActive = false;

    private GameObject ui_textoverlay;
    private GameObject ui_textcontent;

    public float keyRepeatSpeed = 0.5f;
    public float currentKeyRepeat = 0f;

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
<color=#cc99ffff>[SHIFT-Q]</color> - <color=#ffff66ff>Quit the game</color>
";

    void CheckRefs()
    {
        GameObject l = GameObject.Find("LevelManager");
        if (l)
            lm = l.GetComponent<LevelManagerScript>();
        GameObject p = GameObject.Find("player(Clone)");
        if (p)
            this.player = p.GetComponent<PlayerScript>();
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

    List<MenuCallback> menuCallbacks = new List<MenuCallback>();

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

    void Start()
    {

        CallbackMenu(
            "Select a map type!",
            new[] {
                new MenuCallback("Overworld", delegate() {
                        GameObject l = Instantiate(outdoorLevelManager) as GameObject;
                        lm = l.GetComponent<LevelManagerScript>();
                        lm.go_overworld();
                }),
                new MenuCallback("Dungeon", delegate() {
                        GameObject l = Instantiate(dungeonLevelManager) as GameObject;
                        lm = l.GetComponent<LevelManagerScript>();
                        lm.go_simple();
                })
            }
        );


        // set up our callback delegates
        // up
        RegisterCallback(new[] {"k","w","uparrow"}, delegate(string e)
        {
            CheckRefs();
            if (lm == null || lm.active == false) return;
            player.keyboardX = 0;
            player.keyboardY = 1;
        });
        // left
        RegisterCallback(new[] {"h","a","leftarrow"}, delegate(string e)
        {
            CheckRefs();
            if (lm == null || lm.active == false) return;
            player.keyboardX = -1;
            player.keyboardY = 0;
        });
        // down
        RegisterCallback(new[] {"j","s","downarrow"}, delegate(string e)
        {
            CheckRefs();
            if (lm == null || lm.active == false) return;
            player.keyboardX = 0;
            player.keyboardY = -1;
        });
        // right
        RegisterCallback(new[] {"l","d","rightarrow"}, delegate(string e)
        {
            CheckRefs();
            if (lm == null || lm.active == false) return;
            player.keyboardX = 1;
            player.keyboardY = 0;
        });
        // down/left
        RegisterCallback("b", delegate(string e)
        {
            CheckRefs();
            if (lm == null || lm.active == false) return;
            player.keyboardX = -1;
            player.keyboardY = -1;
        });
        // up/left
        RegisterCallback("y", delegate(string e)
        {
            CheckRefs();
            if (lm == null || lm.active == false) return;
            player.keyboardX = -1;
            player.keyboardY = 1;
        });
        // up/right
        RegisterCallback("u", delegate(string e)
        {
            CheckRefs();
            if (lm == null || lm.active == false) return;
            player.keyboardX = 1;
            player.keyboardY = 1;
        });
        // down/right
        RegisterCallback("n", delegate(string e)
        {
            CheckRefs();
            if (lm == null || lm.active == false) return;
            player.keyboardX = 1;
            player.keyboardY = -1;
        });

        // skip turn
        RegisterCallback("period", delegate(string e)
        {
            CheckRefs();
            if (lm == null || lm.active == false) return;
            if (lm)
                lm.playerTurn = false;
        });

        RegisterCallback("Q", delegate(string e)
        {
            CallbackMenu(
                "Are you sure you want to quit?",
                new[] {
                    new MenuCallback("Yes", delegate() { Application.Quit(); }),
                    new MenuCallback("No", delegate() { return; })
                }
            );
        });

        RegisterCallback("P", delegate(string e)
        {

            CallbackMenu(
                "This is the menu!",
                new[] {
                    new MenuCallback("Close", delegate() { return; })
                }
            );
        });
    }
}