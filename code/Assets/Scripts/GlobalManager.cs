using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance { get; private set; }

    // Singleton object, stores game presets and starts new levels

    public BotProfile passiveProfile = new BotProfile("Passive", 0.15f, 0f, 0.3f, 5, 3);
    public BotProfile aggresiveProfile = new BotProfile("Aggresive", 0f, 0f, 0.8f, 0, 7);
    public BotProfile tacticalProfile = new BotProfile("Tactical", 0.05f, 0.5f, 0.6f, 2, 5);
    public BotProfile testerProfile = new BotProfile("Tester", 0f, 0f, 0f, 0, 3);

    public Sprite greenSprite;
    public Sprite blueSprite;

    public GameObject humanPlayer1;
    public GameObject humanPlayer2;

    public GameObject AIPlayer;

    public Camera playerCamera;

    public float inputTimer;
    public bool debugMode;

    public (int, int, int, int) settings;

    public RenderTexture RT1;
    public RenderTexture RT2;

    public Color winColor;
    public Color loseColor;

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else
        {
            Destroy(gameObject);
        }

    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject gm = null;
        var objects = scene.GetRootGameObjects();
        foreach (GameObject obj in objects)
        {
            if (obj.name == "GameManager")
                gm = obj;
        }

        if (gm != null)
        {
            gm.GetComponent<GameManager>().SetVariables(settings.Item1, settings.Item2, settings.Item3, settings.Item4);
        }
    }

    // Update is called once per frame
    void Update()
    {
    
    }

    internal void StartLevel(int lvlId)
    {
        switch (lvlId)
        {
            case 1:
                settings = (0, 1, 1, 2);
                SceneManager.LoadScene("Level1");
                break;
            case 2:
                settings = (0, 2, 1, 2);
                SceneManager.LoadScene("Level2");
                break;
            case 3:
                settings = (0, 3, 1, 2);
                SceneManager.LoadScene("Level3");
                break;
            case 4:
                settings = (0, 1, 2, 3);
                SceneManager.LoadScene("Level4");
                break;
            case 5:
                settings = (0, 2, 2, 3);
                SceneManager.LoadScene("Level5");
                break;
            case 6:
                settings = (0, 3, 2, 3);
                SceneManager.LoadScene("Level6");
                break;
        }
    }

    internal void StartLevel(int lvlId, int profile1, int profile2)
    {
        settings = (profile1, profile2, 2, 2);
        switch (lvlId)
        {
            case 1:
                SceneManager.LoadScene("Level1");
                break;
            case 2:
                SceneManager.LoadScene("Level2");
                break;
            case 3:
                SceneManager.LoadScene("Level3");
                break;
        }
    }
}
