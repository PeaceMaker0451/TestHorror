using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    [SerializeField] private string _managerScene = "GameManagersScene";
    [SerializeField] private string _baseSceneToLoad = "GameScene";

    private void Start()
    {
        SceneManager.LoadSceneAsync(_managerScene, LoadSceneMode.Additive);
        StartCoroutine(LoadSceneAsync(_baseSceneToLoad));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            throw new Exception("Scene name is empty");

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
