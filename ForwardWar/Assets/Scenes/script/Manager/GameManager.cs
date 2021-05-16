using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using MyThread;

public class GameManager : MonoBehaviour
{
    static GameManager instance;
    public static GameManager Instance { get { Init(); return instance; } }

    GameData gameData = new GameData();
    Updater updater = new Updater();
    InputManager input = new InputManager();

    public static GameData Data { get { return Instance.gameData; } }
    public static Updater Updater { get { return Instance.updater; } }
    public static InputManager Input { get { return Instance.input; } }

    public static DebugThread thread;

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
            thread = new DebugThread();
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

    }

    private void OnApplicationQuit()
    {
        thread.Stop();
        thread.Join();
    }

    public void SceneChange(string str)
    {
        SceneManager.LoadScene(1);
    }

    public void ExitApp()
    {
        Application.Quit();
    }
}
