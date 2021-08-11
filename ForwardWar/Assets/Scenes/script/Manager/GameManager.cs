using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using MyThread;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    static GameManager instance;
    public static GameManager Instance { get { Init(); return instance; } }

    public static bool StartGame = false;
    public static float playTime = 0f;

    GameData gameData = new GameData();
    Updater updater = new Updater();
    InputManager input = new InputManager();
    ScoreInfo score = new ScoreInfo();

    public static GameData Data { get { return Instance.gameData; } }
    public static Updater Updater { get { return Instance.updater; } }
    public static InputManager Input { get { return Instance.input; } }
    public static ScoreInfo Score { get { return Instance.score; } }


    //public static DebugThread thread;

    static GameObject clip;
    void Start()
    {
        Init();
        InitUserSetting();
    }

    static void Init()
    {
        if (instance == null)
        {
            GameObject go = GameObject.Find("GameManager");

            if(go == null)
            {
                go = new GameObject { name = "GameManager" };
                go.AddComponent<GameManager>();
            }
            clip = GameObject.Find("Clip");
            if(clip)
                DontDestroyOnLoad(clip);

            DontDestroyOnLoad(go);

            instance = go.GetComponent<GameManager>();
            instance.gameData.Init();
            //thread = new DebugThread();
            Score.InfoReset();
        }
    }

    //당장 유저 설정창이 생길지는 몰라도 설정해둠
    void InitUserSetting()
    {
        //
    }

    // Update is called once per frame
    void Update()
    {
        input.OnUpdate();

        if(StartGame)
        {
            playTime += Time.deltaTime;
        }
    }

    private void OnApplicationQuit()
    {
        //thread.Stop();
        //thread.Join();
    }


    public void SceneChange(int SceneNum)
    {
        SceneManager.LoadScene(SceneNum);
        Score.InfoReset();
        StartGame = true;
        playTime = 0f;
        if(SceneNum == 1)
        {
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = true;
        }
    }

    public void ExitApp()
    {
        Application.Quit();
    }

    public static void GameOver()
    {
        StartGame = false;
    }
}
