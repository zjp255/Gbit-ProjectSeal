using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameManager : Singleton<GameManager>
{
    [HideInInspector]
    public CharacterStats playerStats;
    private CinemachineVirtualCamera camera;
    [HideInInspector]
    public int curLevel;
    private List<Vector3> cameraPos = new List<Vector3>
    {
        new Vector3(5,8,0),

    };
    private List<Quaternion> cameraRotation = new List<Quaternion>
    {
        new Quaternion(0.164301902f,-0.792195201f,0.243271828f,0.535023868f)
    };

    List<IEndGameObserver> endGameObservers = new List<IEndGameObserver>();

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    public void RigisterPlayer(CharacterStats player)
    {
        playerStats = player;
        camera = FindObjectOfType<CinemachineVirtualCamera>();
        //camera.transform.SetPositionAndRotation(cameraPos[curLevel - 1], cameraRotation[curLevel - 1]);
        if(camera != null)
        {
            camera.Follow = playerStats.transform.GetChild(1);
            //camera.LookAt = playerStats.transform.GetChild(2);
        }
    }

    public void AddObserver(IEndGameObserver observer)
    {
        endGameObservers.Add(observer);
    }

    public void RemoveObserver(IEndGameObserver observer)
    {
        endGameObservers.Remove(observer);
    }

    public void NotifyObservers()
    {
        foreach(var observer in endGameObservers)
        {
            observer.EndNotify();
        }
    }

    public Transform GetEntrance()
    {
        foreach (var item in FindObjectsOfType<TransitionDestination>())
        {
            if (item.destinationTag == TransitionDestination.DestinationTag.ENTER)
                return item.transform;
        }
        return null;
    }
}
