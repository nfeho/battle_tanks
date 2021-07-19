using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public List<GameObject> team1 = new List<GameObject>();
    public List<GameObject> team2 = new List<GameObject>();

    public List<Vector3> spawnPositionsTeam1;
    public List<Vector3> spawnPositionsTeam2;

    public int team1type;
    public int team2type;
    public int team1NumberOfMembers;
    public int team2NumberOfMembers;

    private int Team1Wins;
    private int Team2Wins;

    public Canvas GameOverCanvas;
    public Canvas Cameras;
    public Canvas ExitMenu;

    public Canvas SPCamera;
    public Canvas MPCamera;

    public GameObject gameOverText;

    public bool paused;

    // Start is called before the first frame update
    void Start()
    {
        Team1Wins = 0;
        Team2Wins = 0;
        ExitMenu.enabled = false;
        GameOverCanvas.enabled = false;
        if (GlobalManager.Instance.debugMode)
            Cameras.enabled = false;
        paused = false;
    }

    public void SetVariables(int greenTeamType, int blueTeamType, int greenAmountOfMembers, int blueAmountOfMembers)
    {
        // Sets the variables of game on lentering the Scene
        team1type = greenTeamType;
        team2type = blueTeamType;
        team1NumberOfMembers = greenAmountOfMembers;
        team2NumberOfMembers = blueAmountOfMembers;
        StartGame();
    }

    public void StartGame()
    {
        // Spawns tanks, sets their attributes and UI according to manager variables
        if (team1type == 0)
        {
            if (team1NumberOfMembers == 2)
            {
                var player1 = GameObject.Instantiate(GlobalManager.Instance.humanPlayer1);
                var player2 = GameObject.Instantiate(GlobalManager.Instance.humanPlayer2);
                // SET ROTATION, SET DIRECTION, SET TEAMID, SET COLOR, SET TEAMMATES
                var random = Random.Range(0, spawnPositionsTeam1.Count);
                player1.transform.position = spawnPositionsTeam1[random];
                player2.transform.position = spawnPositionsTeam1[(random+1)%spawnPositionsTeam1.Count];
                player1.GetComponent<AbstractController>().SetDirection(Direction.Right);
                player2.GetComponent<AbstractController>().SetDirection(Direction.Right);
                player1.GetComponent<AbstractController>().team = 0;
                player2.GetComponent<AbstractController>().team = 0;
                player1.GetComponent<AbstractController>().SetSprite(0);
                player2.GetComponent<AbstractController>().SetSprite(0);
                var camera1 = GameObject.Instantiate(GlobalManager.Instance.playerCamera);
                var camera2 = GameObject.Instantiate(GlobalManager.Instance.playerCamera);
                camera1.GetComponent<PlayerCamera>().player = player1;
                camera2.GetComponent<PlayerCamera>().player = player2;
                camera1.GetComponent<Camera>().targetTexture = GlobalManager.Instance.RT1;
                camera2.GetComponent<Camera>().targetTexture = GlobalManager.Instance.RT2;
                SPCamera.enabled = false;
                MPCamera.enabled = true;
                team1.Add(player1);
                team1.Add(player2);
            } else
            {
                var player = GameObject.Instantiate(GlobalManager.Instance.humanPlayer1);
                var random = Random.Range(0, spawnPositionsTeam1.Count);
                player.transform.position = spawnPositionsTeam1[random];
                player.GetComponent<AbstractController>().SetDirection(Direction.Right);
                player.GetComponent<AbstractController>().team = 0;
                player.GetComponent<AbstractController>().SetSprite(0);
                var camera = GameObject.Instantiate(GlobalManager.Instance.playerCamera);
                camera.GetComponent<PlayerCamera>().player = player;
                camera.GetComponent<Camera>().targetTexture = GlobalManager.Instance.RT1;
                SPCamera.enabled = true;
                MPCamera.enabled = false;
                team1.Add(player);
            }
        } else
        {
            var player1 = GameObject.Instantiate(GlobalManager.Instance.AIPlayer);
            var player2 = GameObject.Instantiate(GlobalManager.Instance.AIPlayer);
            // SWITCH, SET THE PROFILES
            var random = Random.Range(0, spawnPositionsTeam1.Count);
            player1.transform.position = spawnPositionsTeam1[random];
            player2.transform.position = spawnPositionsTeam1[(random + 1) % spawnPositionsTeam1.Count];
            player1.GetComponent<AbstractController>().SetDirection(Direction.Right);
            player2.GetComponent<AbstractController>().SetDirection(Direction.Right);
            player1.GetComponent<AbstractController>().team = 0;
            player2.GetComponent<AbstractController>().team = 0;
            player1.GetComponent<AbstractController>().SetSprite(0);
            player2.GetComponent<AbstractController>().SetSprite(0);
            player1.GetComponent<AIController>().SetProfile(team1type);
            player2.GetComponent<AIController>().SetProfile(team1type);
            team1.Add(player1);
            team1.Add(player2);
            player1.GetComponent<AIController>().SetMates(team1);
            player2.GetComponent<AIController>().SetMates(team1);
        }

        // ALWAYS AI
        var random2 = Random.Range(0, spawnPositionsTeam2.Count);
        for (int i = 0; i < team2NumberOfMembers; i++)
        {
            var player = GameObject.Instantiate(GlobalManager.Instance.AIPlayer);
            player.transform.position = spawnPositionsTeam2[(random2 + i) % spawnPositionsTeam2.Count];
            player.GetComponent<AbstractController>().SetDirection(Direction.Left);
            player.GetComponent<AbstractController>().team = 1;
            player.GetComponent<AbstractController>().SetSprite(1);
            player.GetComponent<AIController>().SetProfile(team2type);
            team2.Add(player);
        }
        foreach (GameObject member in team2)
        {
            member.GetComponent<AIController>().SetMates(team2);
        }

        paused = false;
        Time.timeScale = 1;
    }

    public void RestartGame()
    {
        // Cleans the scene to default state and then starts a new game
        for (int i = 0; i < team1.Count; i++)
        {
            team1.Remove(team1[i]);
        }
        for (int i = 0; i < team2.Count; i++)
        {
            team2.Remove(team2[i]);
        }

        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            GameObject.Destroy(player);
        }

        var bullets = GameObject.FindGameObjectsWithTag("Bullet");
        foreach (GameObject bullet in bullets)
        {
            GameObject.Destroy(bullet);
        }

        var cameras = GameObject.FindGameObjectsWithTag("Camera");
        foreach (GameObject camera in cameras)
        {
            GameObject.Destroy(camera);
        }
        GameOverCanvas.enabled = false;
        StartGame();
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void Continue()
    {
        ExitMenu.enabled = false;
    }

    public void Quit()
    {
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {
        // Detects if end game conditions are met, after that, end game UI is shown and time is paused
        // In debug mode active, the game is immediately restarted to let bots play again
        if (!paused)
        {
            for (int i = 0; i < team1.Count; i++)
            {
                if (team1[i] == null)
                {
                    team1.Remove(team1[i]);
                }
            }

            for (int i = 0; i < team2.Count; i++)
            {
                if (team2[i] == null)
                {
                    team2.Remove(team2[i]);
                }
            }

            if (team1.Count == 0)
            {
                if (GlobalManager.Instance.debugMode)
                {
                    Team2Wins++;
                    Time.timeScale = 0;
                    paused = true;
                    if (Team1Wins + Team2Wins == 50)
                    {
                        GameOverCanvas.enabled = true;
                        Debug.Log("LEVEL: " + SceneManager.GetActiveScene().name + " |AI1: " + team1type + " | SC: " + Team1Wins + " |AI2: " + team2type + " | SC: " + Team2Wins);
                    } else
                    {
                        PauseBetweenMatches();
                        RestartGame();
                    }
                } else
                {
                    gameOverText.GetComponent<Text>().text = "You LOST";
                    gameOverText.GetComponent<Text>().color = GlobalManager.Instance.loseColor;
                    Debug.Log("LEVEL: " + SceneManager.GetActiveScene().name + ", AI WIN");
                    GameOverCanvas.enabled = true;
                    Time.timeScale = 0;
                    paused = true;
                }

            }
            if (team2.Count == 0)
            {
                if (GlobalManager.Instance.debugMode)
                {
                    Team1Wins++;
                    Time.timeScale = 0;
                    paused = true;
                    if (Team1Wins + Team2Wins == 50)
                    {
                        GameOverCanvas.enabled = true;
                        Debug.Log("LEVEL: " + SceneManager.GetActiveScene().name + " |AI1: " + team1type + " | SC: " + Team1Wins + " |AI2: " + team2type + " | SC: " + Team2Wins);
                    }
                    else
                    {
                        PauseBetweenMatches();
                        RestartGame();
                    }
                }
                else
                {
                    gameOverText.GetComponent<Text>().text = "You WON";
                    gameOverText.GetComponent<Text>().color = GlobalManager.Instance.winColor;
                    Debug.Log("LEVEL: " + SceneManager.GetActiveScene().name + ", HUMAN WIN");
                    GameOverCanvas.enabled = true;
                    Time.timeScale = 0;
                    paused = true;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitMenu.enabled = true;
        }

    }

    public IEnumerator PauseBetweenMatches()
    {
        yield return new WaitForSecondsRealtime(1);
    }
}
