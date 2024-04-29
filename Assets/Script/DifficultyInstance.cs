using Chess;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultyInstance : MonoBehaviour
{
    public static DifficultyInstance Instance { get; private set; }

    public enum Difficulty { Easy, Medium, Hard }
    public Difficulty gameDifficulty = Difficulty.Medium;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }   
    }

    public void SetGameDifficulty(int difficultyLevel)
    {

        switch (difficultyLevel)
        {
            case 1:
                gameDifficulty = Difficulty.Easy;
                LoadGameScene();
                break;

            case 2:
                gameDifficulty = Difficulty.Medium;
                LoadGameScene();
                break;

            case 3:
                gameDifficulty = Difficulty.Hard;
                LoadGameScene();
                break;
        }
    }

    void LoadGameScene()
    {
        Debug.Log("Loading");
        SceneManager.LoadScene("ChessGame");
    }
}
