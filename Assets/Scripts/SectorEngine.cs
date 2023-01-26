using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;


[ExecuteInEditMode]
public class SectorEngine : MonoBehaviour
{

    public Transform _axisPoint;
    public Transform _edgePoint;

    public enum _sectorType
    {
        basic, empty, normal, alert, score, finish
    }

    public _sectorType _type;                            // тип механики сектора

    public Shader _vanishedShader;

    public List<Material> _sectorMaterials = new List<Material>();

    private _sectorType _currentType;                    // закрытое поле текущего типа сектора
    private GameObject _scoreFields;                     // поле объектов где будут подсчитываться очки
    private GameObject _menuUISystem;                    // поле обработки выходов в меню
    private GameObject _audioSystem;                     // поле обработки звуковых эффектов

    private bool IsEnable = true;                        // флаг работы метода триггеров
    private bool IsEmptyCalculate = false;               // флаг для того, чтобы очки плюсовались только один раз
    private bool IsVanishing = false;                    // флаг "растворения" элемента
    private float _vanishingTime = 0;                    // время за которое элемент должен раствориться
    private float _startVanishTime = 0;                  // время начала "растворения"

    public Transform GetAxisPoint() { return _axisPoint; }
    public Transform GetEdgePoint() { return _edgePoint; }
    public void SetEnable(bool enable) { IsEnable = enable; }

    void Update()
    {
        TypeUpdater();

        if (IsVanishing) 
            SectorVanishing();
    }

    public void SetVanishingTime(float time) 
    { 
        if (time > 0)
        {
            _vanishingTime = time;           // записываем время за корторое элемент должен исчезнуть
            _startVanishTime = Time.time;    // записываем время начала исчезания
            IsVanishing = true;              // поднимаем флаг необходимости растворения
        }
    }

    private void SectorVanishing()
    {
        float calc_time = Time.time - _startVanishTime;
        GetComponent<MeshRenderer>().material.SetFloat("_SmoothStep", 1 - (calc_time / _vanishingTime));
    }

    public void SetAlpha(float alpha)
    {
        GetComponent<MeshRenderer>().material.SetFloat(_vanishedShader.GetPropertyName(1), alpha);
    }

    public void SectorCtor()
    {
        // для начала записываем тип объекта в поле текущего типа
        _currentType = _type;
        // берем рендер объекта
        MeshRenderer sectorMeshRenderer = GetComponent<MeshRenderer>();

        if (_menuUISystem == null)
            _menuUISystem = GameObject.FindGameObjectWithTag("MenuUISystem");

        if (_audioSystem == null)
            _audioSystem = GameObject.FindGameObjectWithTag("AudioSystem");

        switch (_currentType)
        {
            case _sectorType.basic:
                sectorMeshRenderer.material = _sectorMaterials[0];
                sectorMeshRenderer.enabled = true;
                break;
            case _sectorType.empty:
                // для пустого сектора цвета нет
                sectorMeshRenderer.enabled = false;
                break;
            case _sectorType.normal:
                // назначаем соответствующий цвет
                //Material vanished_sector = new Material(_vanishedShader);
                //sectorMeshRenderer.material = vanished_sector;

                sectorMeshRenderer.material = _sectorMaterials[4];
                sectorMeshRenderer.enabled = true;
                break;

            case _sectorType.alert:
                // назначаем соответствующий цвет
                sectorMeshRenderer.material = _sectorMaterials[2];
                sectorMeshRenderer.enabled = true;
                break;
            case _sectorType.score:
                // для сектора подсчёта очков также отключаем рендер
                sectorMeshRenderer.enabled = false;
                // делаем поиск окна подсчёта очков
                if (_scoreFields == null)
                    _scoreFields = GameObject.FindGameObjectWithTag("ScoreScreen");
                break;
            case _sectorType.finish:
                // назначаем соответствующий цвет
                sectorMeshRenderer.material = _sectorMaterials[3];
                sectorMeshRenderer.enabled = true;
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "GameBall" && IsEnable)
        {
            GameObject gameBall = other.gameObject;
            Rigidbody ballBody = other.GetComponent<Rigidbody>();

            switch (_currentType)
            {
                case _sectorType.normal:
                    // проверка на отрицательную скорость нужна для того, чтобы шарик не отскакивал дважды от рядомстоящий секторов
                    if (ballBody.velocity.y < 0)
                    {
                        // снимаем комбо-режим если он есть и устанавливаем дефолтное значение скорости
                        gameBall.GetComponent<BallEngine>().SetCombo(false).SetSpeedToDefault();

                        // останавливаем падение шара
                        ballBody.velocity = Vector3.zero;
                        // придаем вертикальной скорости обратной максимальной скорости GameBall со скалированием
                        ballBody.AddForce(
                            new Vector3(0, -other.GetComponent<BallEngine>()._currentYSpeed * 1.4f, 0), ForceMode.VelocityChange);
                        // проигрываем звук отскока шарика
                        _audioSystem.GetComponent<AudioEngine>().PlayBallBounce();
                        // сбрасываем повышение тона у звука пролета шарика
                        _audioSystem.GetComponent<AudioEngine>().SetPlatformBreakPitchDefault();
                    }
                    break;

                case _sectorType.alert:
                    // останавливаем ускорение шарика
                    gameBall.GetComponent<BallEngine>().SetEnable(false);
                    // останавливаем падение шара
                    ballBody.velocity = Vector3.zero;
                    // запрещаем поворот башни
                    GetComponentInParent<TowerEngine>().SetTowerRotation(false);
                    // включаем скрин с вредной аквой
                    _menuUISystem.GetComponent<MenuUIEngine>().ActivateAquaLoseScreen();

                    // проигрываем звук отскока шарика
                    _audioSystem.GetComponent<AudioEngine>().PlayBallBounce();
                    // сбрасываем повышение тона у звука пролета шарика
                    _audioSystem.GetComponent<AudioEngine>().SetPlatformBreakPitchDefault();
                    break;

                case _sectorType.score:
                    // если есть пул объектов где подсчитываются очки
                    if (_scoreFields != null && !IsEmptyCalculate)
                    {
                        // инкрементируем подсчёт
                        _scoreFields.GetComponent<ScoreScreenEngine>()
                            .ScoreIncrement(gameBall.GetComponent<BallEngine>().GetComboCount(), GetComponentInParent<RingEngine>().GetRingDifficulty());

                        IsEmptyCalculate = true;
                    }
                    // включаем набор комбо и ускоряем падение игрового шарика
                    gameBall.GetComponent<BallEngine>().SetCombo(true).IncrementBallSpeed();
                    // повышаем pitch звук пролета шарика
                    _audioSystem.GetComponent<AudioEngine>().IncrementPlatformBreakPitch();
                    break;

                case _sectorType.finish:
                    // останавливаем ускорение шарика
                    gameBall.GetComponent<BallEngine>().SetEnable(false);
                    // останавливаем падение шара
                    ballBody.velocity = Vector3.zero;
                    // запрещаем поворот башни
                    GetComponentInParent<TowerEngine>().SetTowerRotation(false);
                    // включаем скрин с вредной аквой
                    _menuUISystem.GetComponent<MenuUIEngine>().ActivateAquaWinScreen();

                    // проигрываем звук отскока шарика
                    _audioSystem.GetComponent<AudioEngine>().PlayBallBounce();
                    // сбрасываем повышение тона у звука пролета шарика
                    _audioSystem.GetComponent<AudioEngine>().SetPlatformBreakPitchDefault();
                    break;
            }
        }
    }

    public void SetSectorMode(_sectorType type)
    {
        _type = type;
        TypeUpdater();
    }

    public void SetUnenable()
    {
        IsEnable = false;
    }

    public bool GetScoreStatus()
    {
        return IsEmptyCalculate;
    }

    private void TypeUpdater()
    {
        if (_currentType != _type) SectorCtor();
    }
}