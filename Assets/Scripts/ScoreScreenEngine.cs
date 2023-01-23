using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ScoreScreenEngine : MonoBehaviour
{
    public TextMeshProUGUI _scorePoints;
    public TextMeshProUGUI _levelIndex;
    public UnityEngine.UI.Image _levelRangeImage;
    public GameObject _scoreBall;
    public Camera _gameCamera;

    public Int32 _scoreScaler = 100;
    public ScoreScreenEngine SetScoreScaler (Int32 scaler) { _scoreScaler = scaler; return this; }
    public Int32 GetScoreScaler() { return _scoreScaler; }

    private Int32 _level = 1;
    private Int32 _points = 0;
    public ScoreScreenEngine SetLevelIndex(Int32 level) { _level = level; return this; }
    public Int32 GetLevelIndex() { return _level; }
    public ScoreScreenEngine SetScorePoints(Int32 points) { _points = points; return this; }
    public Int32 GetScorePoints() { return _points; }

    private float _levelHeight = 198;
    private float _scoreBallRange = 350;
    public ScoreScreenEngine SetLevelHeight(float height) { _levelHeight = height; return this; }
    public float GetLevelHeight() { return _levelHeight; }
    public ScoreScreenEngine SetScoreBallRange(float range) { _scoreBallRange = range; return this; }
    public float ScoreBallRange() { return _scoreBallRange; }

    private void Awake()
    {
        //_levelHeight = _gameCamera.GetComponent<CameraFollowing>().GetTowerHeight();
        //Debug.Log("Получена высота башни - " + _levelHeight);
    }

    private void FixedUpdate()
    {
        // отображаем количество очков
        _scorePoints.text = _points.ToString();
        _levelIndex.text = _level.ToString();
    }

    public ScoreScreenEngine SetScoreBallDefaultPosition()
    {
        _scoreBall.transform.localPosition = new Vector3(50, 50, 0);
        return this;
    }

    public void ScrolScoreBall(float x)
    {
        Debug.Log("Двигаем шарик");
        x *= _scoreBallRange;
        x /= _levelHeight;

        _scoreBall.transform.localPosition = new Vector3(x + 50, 50, 0);

        _levelRangeImage.fillAmount = x / _scoreBallRange;
    }

    public void ScoreIncrement(Int32 combo)
    {
        // плюсуем очки в зависимости от счётчика комбо
        _points += _scoreScaler * combo;
    }

    public void ScoreIncrement(Int32 combo, Int32 difficulty)
    {
        // плюсуем очки в зависимости от счётчика комбо
        _points += _scoreScaler * combo * (difficulty + 1);
    }
}