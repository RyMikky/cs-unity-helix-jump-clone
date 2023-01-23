using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MenuUIEngine : MonoBehaviour
{
    
    public GameObject _menuScreen;
    public GameObject _aboutScreen;
    public GameObject _bestScoreScreen;
    public GameObject _settingsScreen;
    public GameObject _aquaScreen;

    public GameObject _gameLevel;
    public GameObject _audioEngine;

    public enum Mode
    {
        menu, about, score, aqua, settings, game
    }

    public Mode _activeMode = Mode.menu;

    public MenuUIEngine SetActiveMode(Mode mode) { _activeMode = mode; return this; }
    public MenuUIEngine.Mode GetActiveMode() { return _activeMode; }

    private Mode _archiveMode = Mode.game;

    // Start is called before the first frame update
    void Start()
    {
        _activeMode = Mode.menu;
    }

    // Update is called once per frame
    void Update()
    {
        EnterMainMenu();

        switch (_activeMode)
        {
            case Mode.menu:
                ActivateMenuScreen();
                break;
            case Mode.about:
                ActivateAboutScreen();
                break;
            case Mode.score:
                ActivateBestScoreScreen();
                break;
            case Mode.aqua:
                ActivateAquaScreen();
                break;
            case Mode.game:
                CloseAllScreen();
                break;
        }
    }

    public void EnterMainMenu()
    {
        if (Input.GetButtonUp("Cancel"))
        {
            _audioEngine.GetComponent<AudioEngine>().PlayClick();
            
            if (_activeMode != Mode.menu)
            {
                _archiveMode = _activeMode;
                _activeMode = Mode.menu;
            }
            else
            {
                _activeMode = _archiveMode;
            }
        }
    }

    public void ActivateAboutScreen()
    {
        _activeMode = Mode.about;
        _menuScreen.SetActive(false);
        _aquaScreen.SetActive(false);
        _bestScoreScreen.SetActive(false);
        _settingsScreen.SetActive(false);
        _aboutScreen.SetActive(true);

        _gameLevel.GetComponent<GameLevelEngine>().SetTowerRotation(false);
        //_audioEngine.GetComponent<AudioEngine>().SetBackgroundVolume(0.8f);
    }

    public void ActivateSettingsScreen()
    {
        _activeMode = Mode.settings;
        _menuScreen.SetActive(false);
        _aquaScreen.SetActive(false);
        _bestScoreScreen.SetActive(false);
        _settingsScreen.SetActive(true);
        _aboutScreen.SetActive(false);

        _gameLevel.GetComponent<GameLevelEngine>().SetTowerRotation(false);
        //_audioEngine.GetComponent<AudioEngine>().SetBackgroundVolume(0.8f);
    }

    public void ActivateBestScoreScreen()
    {
        _activeMode = Mode.score;
        _menuScreen.SetActive(false);
        _aquaScreen.SetActive(false);
        _bestScoreScreen.SetActive(true);
        _settingsScreen.SetActive(false);
        _aboutScreen.SetActive(false);

        _gameLevel.GetComponent<GameLevelEngine>().SetTowerRotation(false);
        //_audioEngine.GetComponent<AudioEngine>().SetBackgroundVolume(0.8f);
    }

    public void ActivateMenuScreen()
    {
        _activeMode = Mode.menu;
        _aboutScreen.SetActive(false);
        _aquaScreen.SetActive(false);
        _bestScoreScreen.SetActive(false);
        _settingsScreen.SetActive(false);
        _menuScreen.SetActive(true);

        _gameLevel.GetComponent<GameLevelEngine>().SetTowerRotation(false);
        //_audioEngine.GetComponent<AudioEngine>().SetBackgroundVolume(0.8f);
    }

    public void ActivateAquaScreen()
    {
        _activeMode = Mode.aqua;
        _aboutScreen.SetActive(false);
        _aquaScreen.SetActive(true);
        _bestScoreScreen.SetActive(false);
        _settingsScreen.SetActive(false);
        _menuScreen.SetActive(false);

        _gameLevel.GetComponent<GameLevelEngine>().SetTowerRotation(false);
        //_audioEngine.GetComponent<AudioEngine>().SetBackgroundVolume(0.8f);
    }

    public void ActivateAquaLoseScreen()
    {
        _activeMode = Mode.aqua;
        _aboutScreen.SetActive(false);
        _menuScreen.SetActive(false);
        _bestScoreScreen.SetActive(false);
        _settingsScreen.SetActive(false);
        _aquaScreen.SetActive(true);

        _gameLevel.GetComponent<GameLevelEngine>().SetTowerRotation(false);
        _aquaScreen.GetComponent<AquaScreenEngine>().SetScreenMode(AquaScreenEngine.Mode.lose);
        //_audioEngine.GetComponent<AudioEngine>().SetBackgroundVolume(0.8f);
    }

    public void ActivateAquaWinScreen()
    {
        _activeMode = Mode.aqua;
        _aboutScreen.SetActive(false);
        _menuScreen.SetActive(false);
        _bestScoreScreen.SetActive(false);
        _settingsScreen.SetActive(false);
        _aquaScreen.SetActive(true);

        _gameLevel.GetComponent<GameLevelEngine>().SetTowerRotation(false);
        _aquaScreen.GetComponent<AquaScreenEngine>().SetScreenMode(AquaScreenEngine.Mode.win);
        //_audioEngine.GetComponent<AudioEngine>().SetBackgroundVolume(0.8f);
    }

    public void CloseAllScreen()
    {
        _activeMode = Mode.game;
        _aboutScreen.SetActive(false);
        _menuScreen.SetActive(false);
        _aquaScreen.SetActive(false);
        _bestScoreScreen.SetActive(false);
        _settingsScreen.SetActive(false);

        _gameLevel.GetComponent<GameLevelEngine>().SetTowerRotation(true);
        //_audioEngine.GetComponent<AudioEngine>().SetBackgroundVolume(0.6f);
    }

    public void ApplicationTurnOff()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
        }

        Application.Quit();
    }

    public void StartNewGame()
    {
        GetComponentInParent<GameEngine>().ConstructNewGameLevel();
    }

    public void StartNextGame()
    {
        GetComponentInParent<GameEngine>().ConstructNextGameLevel();
    }
}