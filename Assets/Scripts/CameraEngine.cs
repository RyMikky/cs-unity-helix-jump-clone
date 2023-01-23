using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEngine : MonoBehaviour
{
    //public GameObject _gameLevel;

    public bool _isEnable = false;                   // флаг вклчюение следования камеры
    public float _breakScaler = 1f;                  // ускоритель остановки камеры

    public CameraEngine SetBreakScaler(float scaler) { _breakScaler = scaler; return this; }
    public float GetBreakScaler() { return _breakScaler; }

    private bool _needStop = false;                  // флаг необходимости остановки камеры
    private Vector3 _camera_position;                // сохраненная позиция камеры

    private void Awake()
    {
        _camera_position = transform.position;
    }

    private void FixedUpdate()
    {
        // на постоянку отслеживается необходимость остановки "падения" камеры
        if (_needStop)
        {
            CameraStoper();
        }

        if (_camera_position != transform.position)
        {
            FindObjectOfType<ScoreScreenEngine>().ScrolScoreBall(-transform.position.y);
            _camera_position = transform.position;
        }
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