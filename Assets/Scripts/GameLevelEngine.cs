using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLevelEngine : MonoBehaviour
{

    public GameObject _gameBall;
    public float _defaultBallSpeed;
    public float _maximumBallSpeed;

    public float GetDefaultBallSpeed() { return _defaultBallSpeed; }
    public GameLevelEngine SetDefaultBallSpeed(float d_speed) { _defaultBallSpeed = d_speed; return this; }
    public float GetMaximumBallSpeed() { return _maximumBallSpeed; }
    public GameLevelEngine SetMaximumBallSpeed(float m_speed) { _maximumBallSpeed = m_speed; return this; }


    public GameObject _gameTower;
    public Int32 _towerHeight;
    public Int32 _ringOffset;
    public Int32 _minDifficulty;
    public Int32 _maxDifficulty;
    public TowerEngine.Mode _towerMode;

    public Int32 GetTowerHeight() { return _towerHeight; }
    public GameLevelEngine SetTowerHeight(Int32 height) { _towerHeight = height; return this; }
    public Int32 GetRingOffset() { return _ringOffset; }
    public GameLevelEngine SetRingOffset(Int32 offset) { _ringOffset = offset; return this; }
    public Int32 GetMinDifficulty() { return _minDifficulty; }
    public GameLevelEngine SetMinDifficulty(Int32 min) { _minDifficulty = min; return this; }
    public Int32 GetMaxDifficulty() { return _maxDifficulty; }
    public GameLevelEngine SetMaxDifficulty(Int32 max) { _maxDifficulty = max; return this; }
    public TowerEngine.Mode GetTowerMode() { return _towerMode; }
    public GameLevelEngine SetTowerMode(TowerEngine.Mode mode) { _towerMode = mode; return this; }

    // включение или отключение поворота башни
    public GameLevelEngine SetTowerRotation(bool flag) { _childGameTower.GetComponent<TowerEngine>().SetTowerRotation(flag); return this; }

    public GameObject _childGameBall = null;
    public GameObject _childGameTower = null;

    public GameLevelEngine ConstructGameLevel()
    {
        MakeGameTower(_towerHeight, _ringOffset, _minDifficulty, _maxDifficulty, _towerMode);
        MakeGameBall(_defaultBallSpeed, _maximumBallSpeed);
        return this;
    }

    public GameLevelEngine ReconstructGameLevel() 
    {
        if (_childGameBall != null)
        {
            _childGameBall.transform.SetParent(null);
            DestroyImmediate(_childGameBall);
        }
        

        if (_childGameTower != null)
        {
            _childGameTower.transform.SetParent(null);
            _childGameTower.GetComponent<TowerEngine>().DestroyTower();
            DestroyImmediate(_childGameTower);
        }

        return ConstructGameLevel();
    }

    public GameLevelEngine MakeGameBall()
    {
        return MakeGameBall(20, 30);
    }
    public GameLevelEngine MakeGameBall(float d_speed, float m_speed)
    {
        // создаем объект - игровой шарик
        _childGameBall = Instantiate(_gameBall, transform) as GameObject;

        _childGameBall.GetComponent<BallEngine>()
            .SetEnable(true)                      // подлючаем обработку шарика
            .SetDefaultBallSpeed(-d_speed)        // задаём базовую скорость падения
            .SetMaximalBallSpeed(-m_speed);       // задаём максимальну скорость падения

        _childGameBall.SetActive(true);

        return this;
    }

    public GameLevelEngine MakeGameTower()
    {
        return MakeGameTower(205, 10, 0, 0, TowerEngine.Mode.procedure);
    }

    public GameLevelEngine MakeGameTower(Int32 t_height, Int32 r_offset, Int32 min_dif, Int32 max_dif, TowerEngine.Mode mode)
    {
        // создаём объект - башня уровня
        _childGameTower = Instantiate(_gameTower, transform) as GameObject;

        _childGameTower.GetComponent<TowerEngine>()
            .SetTowerHeight(t_height)           // задаём высоту башни
            .SetRingsOffset(r_offset)           // задаём расстояние между кольцами
            .SetMinDifficulty(min_dif)          // задаём минимальную сложность
            .SetMaxDifficulty(max_dif)          // задаём максимальную сложность
            .SetExecutionMode(mode)             // задаём режим генерации
            .SetTowerRotation(true)             // включаем вращение
            .TowerConstructor();                // перестраиваем башню по заданым параметрам

        _childGameTower.SetActive(true);

        return this;
    }
}