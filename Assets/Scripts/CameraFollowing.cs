using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowing : MonoBehaviour
{

    public bool _isEnable = false;                   // влаг вклчюение следования камеры
    public float _breakScaler = 1f;                  // ускоритель остановки камеры


    public GameObject _gameLevel;
    private float _towerHeight = 198;
    private float _downProcent = 0;

    private bool _needStop = false;
    private Vector3 _camera_position;

    public float GetDownProcent() { return _downProcent; }
    public CameraFollowing SetTowerHeight(float height) { _towerHeight = ((height - 5) *2) -_breakScaler; return this; }
    public float GetTowerHeight() { return _towerHeight; }

    private void Awake()
    {
        _camera_position = transform.position;
        // _towerHeight = ((_gameLevel.GetComponent<GameLevelEngine>().GetTowerHeight() - 5) * 2) -_breakScaler;
    }

    private void FixedUpdate()
    {
        // на постоянку отслеживается необходимость остановки "падения" камеры
        if (_needStop)
        {
            CameraStoper();
        }

        CalculateDownProcent();
        
        if (_camera_position != transform.position)
        {
            FindObjectOfType<ScoreScreenEngine>().ScrolScoreBall(-transform.position.y);
            _camera_position = transform.position;
        }
    }

    private void CalculateDownProcent() 
    {
        _downProcent = -transform.position.y / _towerHeight;
    }

    private void OnTriggerEnter(Collider other)
    {
        // когда на триггер попадает GameBall и отслеживание включено
        if (other.gameObject.tag == "GameBall" && _isEnable)
        {
            // смотрим скорость шара
            //Vector3 ball_speed = _gameBall.GetComponent<Rigidbody>().velocity;
            Vector3 ball_speed = other.gameObject.GetComponent<Rigidbody>().velocity;

            // если шар падает, а камера стоит на месте
            if (ball_speed.y < 0)
            {
                // запускаем камеру с той же скоростью, что и GameBall
                GetComponent<Rigidbody>().AddForce(ball_speed, ForceMode.VelocityChange);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // когда на триггер попадает GameBall и отслеживание включено
        if (other.gameObject.tag == "GameBall" && other.gameObject.GetComponent<BallEngine>().GetCombo() && _isEnable)
        {
            GetComponent<Rigidbody>().velocity = other.gameObject.GetComponent<Rigidbody>().velocity;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // когда GameBall сходит с триггера и отслеживание включено
        if (other.gameObject.tag == "GameBall" && _isEnable)
        {
            // если при этом скорость камеры не равна нулю
            if (GetComponent<Rigidbody>().velocity != Vector3.zero)
            {
                _needStop = true;     // задаем флаг того, что требуется остановка
            }
        }
    }

    private void CameraStoper()
    {
        // смотрим текущую скорость камеры
        Vector3 camera_speed = GetComponent<Rigidbody>().velocity;

        //Debug.Log($"Тормозим камеру");

        // если вертикальная скорость ниже нуля и требуется остановка
        if (camera_speed.y < 0 && _needStop)
        {
            // начинаем тормозить "падение" камеры
            GetComponent<Rigidbody>().AddForce(Vector3.up * _breakScaler, ForceMode.VelocityChange);
        }
        else
        {
            // как только перемахнули в плюс - стопаем перемещение
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            // снимаем флаг необходимости торможения
            _needStop = false;
        }
    }
}