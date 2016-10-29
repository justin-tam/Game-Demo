/*  
 *  Project: Phoenix Horizons
 *  Class: CharacterMovement
 *  Written By: Justin Tam
 *  Date: September 2016
 *  Summary: Handle Scene Display
 *  TODO: Actually run LoadScene()
*/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadNewScene : MonoBehaviour
{
    string newScene; //name of scene to load | Feel free to add spaces. Spaces will be removed when loading scene.
    static Image titleBG;
    static Text title;
    Color titleBGColor;
    Color titleColor;

    void Start()
    {
        titleBG = GameObject.Find("Title_Background").GetComponent<Image>();
        title = GameObject.Find("Title").GetComponent<Text>();
        titleBGColor = titleBG.color;
        titleColor = title.color;
        newScene = gameObject.name;
    }

    public void DisplayScene()
    {
        //Set title of new scene
        title.text = newScene;

        //Display NewScene
        while (titleBGColor.a < 0.9f)
        {
            titleBGColor.a += 2.0f * Time.deltaTime;
            titleColor.a += 2.0f * Time.deltaTime;
            //Yield?
        }
        titleBGColor.a = (1.0f);
        titleColor.a = 1.0f;
        titleBG.color = titleBGColor;
        title.color = titleColor;
    }

    public void HideScene()
    {
        //Hide NewScene
        while (titleBGColor.a > 0.01f)
        {
            titleBGColor.a -= 2.0f * Time.deltaTime;
            titleColor.a -= 2.0f * Time.deltaTime;
            //Yield?
        }
        titleBGColor.a = 0.0f;
        titleColor.a = 0.0f;
        titleBG.color = titleBGColor;
        title.color = titleColor;
    }

    void LoadScene()
    {
        //Removes spaces from newScene text!
        string newString = newScene.Replace(" ", "");
        //Load the name of our new string
        SceneManager.LoadScene(newString);
    }
}