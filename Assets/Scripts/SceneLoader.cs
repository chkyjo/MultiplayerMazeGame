using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour {

    public GameObject loadingScreen;
    public Slider loadingBar;
    public Text percentageText;

	public void LoadLevel(int sceneIndex) {
        StartCoroutine(LoadAsynchronously(sceneIndex));
        
    }

    IEnumerator LoadAsynchronously(int sceneIndex) {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        loadingScreen.SetActive(true);
        float progress;

        while (!operation.isDone) {
            progress = Mathf.Clamp01(operation.progress / 0.9f);
            loadingBar.value = progress;
            percentageText.text = progress * 100f + "%";

            yield return null;
        }
    }

    public void QuitGame() {
        Application.Quit();
    }
}
