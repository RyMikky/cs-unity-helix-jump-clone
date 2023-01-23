using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameEngine : MonoBehaviour
{

    public Camera _mainCamera;
    public GameObject _gameLevel;
    public GameObject _menuUISystem;
    public GameObject _scoreScreen;

    public float _cameraBreakScaler;

    public float _defaultBallSpeed;
    public float _maximumBallSpeed;

    public Int32 _towerHeight;
    public Int32 _ringOffset;
    public Int32 _minDifficulty;
    public Int32 _maxDifficulty;
    public TowerEngine.Mode _towerMode;

    public Int32 _screenScoreScaler;

    private Int32 _gameLevelIndex;

    private Int32 _lastGameScore;
    private Int32 _totalGameScore;

    void Start()
    {
        ConstructTestLevel();
        EnterMenuScreen();
        ActivateScoreScreen();
    }

    // Update is called once per frame
    void Update()
    {
        _totalGameScore = _scoreScreen.GetComponent<ScoreScreenEngine>().GetScorePoints();
    }

    public void GameExit()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
        }
        Application.Quit(0);
    }

    public GameEngine EnterMenuScreen()
    {
        _menuUISystem.SetActive(true);
        _menuUISystem.GetComponent<MenuUIEngine>().SetActiveMode(MenuUIEngine.Mode.menu);
        return this;
    }
    public GameEngine ActivateScoreScreen()
    {
        _scoreScreen.SetActive(true);
        return this;
    }

    public GameEngine ConstructTestLevel()
    {
        _mainCamera.GetComponent<CameraEngine>()
            .SetBreakScaler(2);

        _scoreScreen.GetComponent<ScoreScreenEngine>()
            .SetLevelIndex(1)
            .SetLevelHeight((55 - (10 / 2)) * 2)
            .SetScoreScaler(100);

        _gameLevel.GetComponent<GameLevelEngine>()
            .SetDefaultBallSpeed(20)
            .SetMaximumBallSpeed(30)
            .SetTowerHeight(55)
            .SetRingOffset(10)
            .SetMinDifficulty(0)
            .SetMaxDifficulty(9)
            .SetTowerMode(TowerEngine.Mode.test)
            .ConstructGameLevel();

        return this;
    }

    public void ConstructNewGameLevel()
    {
        ResetCameraPosition();

        _gameLevel.GetComponent<GameLevelEngine>()
            .SetDefaultBallSpeed(20)
            .SetMaximumBallSpeed(30)
            .SetTowerHeight(205)
            .SetRingOffset(10)
            .SetMinDifficulty(0)
            .SetMaxDifficulty(3)
            .SetTowerMode(TowerEngine.Mode.procedure)
            .ReconstructGameLevel();

        _scoreScreen.GetComponent<ScoreScreenEngine>()
            .SetScorePoints(0)
            .SetLevelIndex(1)
            .SetLevelHeight((205 - (10 / 2)) * 2)
            .SetScoreScaler(100);

        _mainCamera.GetComponent<CameraEngine>()
            .SetBreakScaler(2);
    }

    public void ConstructNextGameLevel()
    {
        ResetCameraPosition();

        _gameLevel.GetComponent<GameLevelEngine>()
            .SetDefaultBallSpeed(20)
            .SetMaximumBallSpeed(30)
            .SetTowerHeight(205)
            .SetRingOffset(10)
            .SetMinDifficulty(0)
            .SetMaxDifficulty(3)
            .SetTowerMode(TowerEngine.Mode.procedure)
            .ReconstructGameLevel();

        _scoreScreen.GetComponent<ScoreScreenEngine>()
            .SetScorePoints(_totalGameScore)
            .SetLevelIndex(1)
            .SetLevelHeight((205 - (10 / 2)) * 2)
            .SetScoreScaler(100);

        _mainCamera.GetComponent<CameraEngine>()
            .SetBreakScaler(2);
    }

    public void ResetCameraPosition()
    {
        // сброс базовой позиции камеры
        _mainCamera.transform.position = new Vector3(0, -1.5f, 0);
        _mainCamera.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }
}
