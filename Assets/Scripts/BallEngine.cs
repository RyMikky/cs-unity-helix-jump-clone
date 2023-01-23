using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallEngine : MonoBehaviour
{
    public float _currentYSpeed = -20f;
    public float _defaultYSpeed = -20f;
    public float _maximalYSpeed = -30f;

    private Int32 _comboCount = 1;
    private bool _enable = true;
    private bool _combo = false;

    public BallEngine SetCurrentBallSpeed(float speed) { _currentYSpeed = speed; return this;}
    public float GetCurrentBallSpeed() { return _currentYSpeed; }
    public BallEngine SetDefaultBallSpeed(float speed) { _defaultYSpeed = speed; return this; }
    public float GetDefaultBallSpeed() { return _defaultYSpeed; }
    public BallEngine SetMaximalBallSpeed(float speed) { _maximalYSpeed = speed; return this; }
    public float GetMaximalBallSpeed() { return _maximalYSpeed; }
    public BallEngine SetEnable(bool enable) { _enable = enable; return this; }
    public bool GetEnable() { return _enable; }
    public BallEngine SetCombo(bool enable) 
    { 
        if (!enable) 
        {
            _comboCount = 1;
        }
        else
        {
            _comboCount++;
        }

        _combo = enable;
        return this;
    }
    public bool GetCombo() { return _combo; }
    public Int32 GetComboCount() { return _comboCount;}
    public void IncrementBallSpeed() { if (_currentYSpeed > _maximalYSpeed) --_currentYSpeed; }
    public BallEngine SetSpeedToDefault() { _currentYSpeed = _defaultYSpeed; return this; }

    void FixedUpdate()
    {
        if (_enable)
        {
            // если текущая скорость падения GameBall превышает максимум
            if (GetComponent<Rigidbody>().velocity.y > _currentYSpeed)
            {
                // ускоряемся каждый кадр на десятую часть максимума
                GetComponent<Rigidbody>().AddForce(new Vector3(0, _currentYSpeed / 10, 0), ForceMode.VelocityChange);
            }
        }
    }
    public void StopTheBall()
    {
        if (_enable) _enable = false;
    }
}