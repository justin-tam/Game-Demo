/*  
 *  Project: Phoenix Horizons
 *  Class: Actor
 *  Written By: Justin Tam
 *  Date: September 2016
 *  Summary: Character Information - Keep it simple
*/

using UnityEngine;
using System.Collections;

public class Actor : MonoBehaviour {

    [SerializeField]
    private string _name;
    [SerializeField]
    private int _health;
    [SerializeField]
    private int _wealth;
    [SerializeField]
    private int _mood;
    /*
        0 - Sad
        1 - Okay
        2 - Happy
        3 - Scared
        4 - Angry
        5 - Aggressive
     */
    [SerializeField]
    private int _personality;
    [SerializeField]
    private int _faction;
    /*
        0 - Neutral
        1 - Wildlife
        2 - Bandit
        3 - Player
        4 - Sword
        5 - Shield
        6 - Crown
        7 - Independence
        8 - Republic
     */

    // Use this for initialization
    void Start () {
	
	}

    void Update() { }

    public string charName
    {
        get { return _name; }
        set { _name = value; }
    }

    public int health
    {
        get { return _health; }
        set { _health = value; }
    }

    public int wealth
    {
        get { return _wealth; }
        set { _wealth = value; }
    }

    public int mood
    {
        get { return _mood; }
        set { _mood = value; }
    }

    public int personality
    {
        get { return _personality; }
        set { _personality = value; }
    }

    public int faction
    {
        get { return _faction; }
        set { _faction = value; }
    }
}
