using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    public static VideoManager Instance { get; private set; }

    [SerializeField]
    private VideoPlayer player;

    [SerializeField]
    private RawImage rawImage;

    [SerializeField]
    private List<VideoClip> clips;

    private RawImage image;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnLoadScene;
        player.loopPointReached += OnVideoEnd;
    }

    private void OnLoadScene(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.name.Contains("House"))
        {
            LoadVideo(2);
        }
    }

    private void OnVideoEnd(VideoPlayer source)
    {
        player.Stop();
        SingleSceneManager.LoadNextScene();
    }

    public void LoadVideo(int index)
    {
        image = Instantiate(rawImage, FindAnyObjectByType<Canvas>().transform);
        player.clip = null;
        player.clip = clips[index];
        player.time = 0;
        player.Play();
        image.gameObject.SetActive(true);
    }

    public void SkipVidoe()
    {
        OnVideoEnd(player);
    }
}
