using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManger : Singleton<GameManger>
{
    public bool isGameOver;
    public bool isClear;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GameOver()
    {
        //if(PlayerController.instance.Hp <= 0)
            isGameOver = true;
        SceneManager.LoadScene("GameOverScene");
    }
}
