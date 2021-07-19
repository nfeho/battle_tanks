using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour
{
    public Canvas FirstMenuCanvas;
    public Canvas ChallengePickCanvas;
    public Canvas SinglePlayerCanvas;
    public Canvas MultiPlayerCanvas;

    public Canvas SPControls;
    public Canvas MPControls;

    private int playerParty;

    public Canvas DebugMenuCanvas;

    public int debugModeTeam1Profile;
    public int debugModeTeam2Profile;
    public int debugModeLevelId;

    public GameObject levelDropdownMenu;
    public GameObject profile1DropdownMenu;
    public GameObject profile2DropdownMenu;


    // This class handles main menu UI elements

    // Start is called before the first frame update
    void Start()
    {
        FirstMenuCanvas.enabled = true;
        ChallengePickCanvas.enabled = false;
        SinglePlayerCanvas.enabled = false;
        MultiPlayerCanvas.enabled = false;
        DebugMenuCanvas.enabled = false;
        SPControls.enabled = false;
        MPControls.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnOnePlayerClicked()
    {
        SinglePlayerCanvas.enabled = true;
        ChallengePickCanvas.enabled = true;
        playerParty = 0;
        FirstMenuCanvas.enabled = false;
    }

    public void OnTwoPlayerClicked()
    {
        MultiPlayerCanvas.enabled = true;
        ChallengePickCanvas.enabled = true;
        playerParty = 1;
        FirstMenuCanvas.enabled = false;
    }

    public void OnBackButtonClicked()
    {
        FirstMenuCanvas.enabled = false;
        MultiPlayerCanvas.enabled = false;
        FirstMenuCanvas.enabled = true;
        ChallengePickCanvas.enabled = false;
    }

    public void ToggleMPControls(bool isOn)
    {
        MPControls.enabled = isOn;
    }

    public void ToggleSPControls(bool isOn)
    {
        SPControls.enabled = isOn;
    }

    public void StartLevel(int choice)
    {
        if (playerParty == 0)
        {
            // SINGLE PLAYER
            if (choice == 0)
            {
                GlobalManager.Instance.StartLevel(1);
                // Level1
            } else if (choice == 1)
            {
                GlobalManager.Instance.StartLevel(2);
                // Level2
            } else
            {
                GlobalManager.Instance.StartLevel(3);
                // Level3
            }
        }
        else if (playerParty == 1)
        {
            // MP
            if (choice == 0)
            {
                GlobalManager.Instance.StartLevel(4);
                // Level4
            }
            else if (choice == 1)
            {
                GlobalManager.Instance.StartLevel(5);
                // Level5
            }
            else
            {
                GlobalManager.Instance.StartLevel(6);
                // Level6
            }
        }
    }

    public void ChangeDebugModeValue(int id)
    {
        if (id == 0)
        {
            debugModeLevelId = levelDropdownMenu.GetComponent<Dropdown>().value + 1;
        }
        if (id == 1)
        {
            debugModeTeam1Profile = profile1DropdownMenu.GetComponent<Dropdown>().value + 1;
        }
        if (id == 2)
        {
            debugModeTeam2Profile = profile2DropdownMenu.GetComponent<Dropdown>().value + 1;
        }
    }

    public void StartDebugLevel()
    {
        GlobalManager.Instance.StartLevel(debugModeLevelId, debugModeTeam1Profile, debugModeTeam2Profile);
    }

    public void OnDebugMenuEnable()
    {
        GlobalManager.Instance.debugMode = true;
        FirstMenuCanvas.enabled = false;
        DebugMenuCanvas.enabled = true;
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
