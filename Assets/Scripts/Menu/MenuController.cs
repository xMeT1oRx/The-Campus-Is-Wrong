using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject SettingsPanel;
    // Start is called before the first frame update
    void Start()
    {
        SettingsPanel.SetActive(false);
    }

    public void Play()
    {
        SceneManager.LoadScene("SampleScene");
        //SceneManager.LoadScene(1);
    }

    public void Back()
    {
        SceneManager.LoadScene("MainMenu");
        //SceneManager.LoadScene(0);
    }

    public void Settings()
    {
        if (SettingsPanel.activeSelf == false)
        {
            SettingsPanel.SetActive(true);
        }
        else if (SettingsPanel.activeSelf == true)
        {
            SettingsPanel.SetActive(false);
        }
    }
    // Update is called once per frame
    public void Exit()
    {
        Application.Quit();
        Debug.Log("Вы выли из игры");
    }
}
