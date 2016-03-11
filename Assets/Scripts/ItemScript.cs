using UnityEngine;
using System.Collections;
using ThugLib;

public class ItemScript : MonoBehaviour {

    public ItemEntity entity;

    void Awake ()
    {
        this.entity = new ItemEntity();
    }

//    public ItemScript (ItemEntity e)
//    {
//        this.entity = e;
//    }

    // Use this for initialization
    void Start () {
    
    }
    
    // Update is called once per frame
    void Update () {
    
    }
}
