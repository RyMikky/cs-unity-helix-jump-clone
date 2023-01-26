using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;

//[ExecuteInEditMode]
public class RingEngine : MonoBehaviour
{
    public enum Mode
    {
        empty, simple, procedure, finish
    }

    public Int32 _difficult = 0;                                      // сложность для процедурной генерации
    public float _score_down_offset = 1.5f;                           // отступ объекта подсчёта очков вниз по оси Y

    public Mode _execution_mode;                                      // режим работы обработки объекта
    public string _console_command;                                   // поле ввода команд
    public List<GameObject> _sectors = new List<GameObject>();        // лист с объектами
    public List<Material> _materials = new List<Material>();          // лист материалов

    private Mode _current_mode;
    private List<GameObject> _children = new List<GameObject>();      // лист наследников-секторов
    private bool _is_explised = true;                                 // флаг готовности к разлету
    private GameObject _audioSystem;                                  // поле обработки звуковых эффектов

    // базовая константная таблица  индексов элементов и их размеров в градусах
    private Dictionary<Int32, Int32> _SECTORS_GRAD_
        = new Dictionary<Int32, Int32>() { { 0, 12}, { 1, 24}, { 2, 36}, { 3, 48}, { 4, 60} };

    public RingEngine SetExecutionMode(Mode mode) { _execution_mode = mode; return this; }
    public RingEngine SetRingDifficulty(Int32 diff) { _difficult = diff; return this; }
    public Int32 GetRingDifficulty() { return _difficult; }

    void Start()
    {
        RingCTor();

        if (_audioSystem == null)
            _audioSystem = GameObject.FindGameObjectWithTag("AudioSystem");
    }

    void Update()
    {
        ModeChecker();
        ExploseChildren();
    }

    private void ChildsClear()
    {
        foreach (GameObject child in _children)
        {
            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }
        _children.Clear();
    }

    private void ModeChecker()
    {
        if (_current_mode != _execution_mode) RingCTor();
    }

    private void RingCTor()
    {
        _current_mode = _execution_mode;
        switch (_execution_mode)
        {
            case Mode.empty:
                ChildsClear();
                break;

            case Mode.simple:
                SimpleCtor();
                break;
            case Mode.procedure:
                ProcedureCtor(_difficult);
                break;
            case Mode.finish:
                FinishCtor();
                break;
        }
    }

    private void SimpleCtor() 
    {
        if (_children.Count == 0)
        {
            Int32 angle = 0;

            for (Int32 i = 0; i < 4; i++)
            {
                GameObject new_sector = Instantiate(_sectors[4], transform) as GameObject;
                new_sector.GetComponent<SectorEngine>().SetSectorMode(SectorEngine._sectorType.normal);

                new_sector.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(new Vector3(0, angle, 0)));
                _children.Add(new_sector);

                angle += 90;
            }
            MakeScoreObject();            // досоздаём коллайдер для подсчёта очков
        }
    }

    private void ProcedureCtor(Int32 hard_scaler)
    {

        if (_children.Count != 0) return;     // генерация работает только если нет наследников

        // пытаемся получить карты построения уровня из заготовленных константных мап
        if (_DIFF_PARAM_.TryGetValue(hard_scaler, out DiffucultyMap diff_map) 
            && _SECTOR_CALC_.TryGetValue(hard_scaler, out SectorsCalculate s_cals))
        {
            Int32 last_size = 0;                          // запоминаем предыдущий созданный размер
            Int32 angle = 0;                              // переменная подсчёта углаповорота
            bool IsFirst = true;                          // флаг создания первого модуля

            // начинаем генерацию согласно количеству указанному в мапе
            for (Int32 i = 0; i != diff_map._total_sectors_count; i++)
            {
                SectorEngine._sectorType type;            // заготовка под создаваемвый тип
                Int32 size;                               // заготовка под создаваемый размер

                bool InProcess = true;                    // флаг процесса подбора параметров сектора
                while (InProcess)                         // цикл подбора параметров для создания сектора
                {
                    type = GetRandomSectorType();         // получаем тип создаваемого сектора
                    size = GetRandomSectorSize();         // получаем размер создаваемого сектора

                    // проверяем возможность создания данного типа сектора
                    if (!SectorTypeIsApprove(type, ref diff_map)) continue;
                    // проверяем возможность создания сектора данного размера
                    if (!SectorSizeIsApprove(size, ref s_cals)) continue;

                    InProcess = false;                    // закрываем флаг процесса подбора

                    // создаем сектор указанного размера в базовой точке
                    GameObject new_sector = Instantiate(_sectors[size], transform) as GameObject;
                    // присваиваем сектору режим выполнения согласно полученного типа
                    new_sector.GetComponent<SectorEngine>().SetSectorMode(type);

                    if (IsFirst)
                    {
                        // если элемент первый, то ставим по углу 0
                        IsFirst = false; // закрываем флаг первого элемента
                    }
                    else
                    {
                        // прибавляем угол по полученному размеру текущего и предыдущего элементов
                        angle += (_SECTORS_GRAD_[size] / 2) + (_SECTORS_GRAD_[last_size] / 2);
                    }

                    // поворачиваем объект согласно имеющегося угла положения следующего сектора
                    new_sector.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(new Vector3(0, angle, 0)));
                    // добавляем полученный элемент в список "детей" родительского объекта
                    _children.Add(new_sector);

                    // запоминаем созданный размер
                    last_size = size;

                    // актуализируем текущую карту сложности
                    ActualizeDiffucultyMap(type, ref diff_map);
                    // актуализируем текущую калькуляцию секторов
                    ActualizeSectorsCalculate(size, ref s_cals);
                }
            }
            MakeScoreObject();            // досоздаём коллайдер для подсчёта очков
        }
        RandomRotation();                 // рандомно разворачиваем линию вдоль вертикальной оси
    }

    private void FinishCtor()
    {
        if (_children.Count == 0)
        {
            // создаем крайний объект - коллайдер подсчёта очков
            GameObject score_sector = Instantiate(_sectors[5], transform) as GameObject;
            // присваиваем сектору режим выполнения согласно полученного типа
            score_sector.GetComponent<SectorEngine>().SetSectorMode(SectorEngine._sectorType.finish);
            // поворачиваем объект согласно имеющегося угла положения следующего сектора
            score_sector.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(Vector3.zero));
            // добавляем полученный элемент в список "детей" родительского объекта
            _children.Add(score_sector);
        }
    }

    private void MakeScoreObject()
    {
        // создаем крайний объект - коллайдер подсчёта очков
        GameObject score_sector = Instantiate(_sectors[5], transform) as GameObject;
        // присваиваем сектору режим выполнения согласно полученного типа
        score_sector.GetComponent<SectorEngine>().SetSectorMode(SectorEngine._sectorType.score);
        // поворачиваем объект согласно имеющегося угла положения следующего сектора
        score_sector.transform.SetLocalPositionAndRotation(new Vector3(0, -_score_down_offset, 0), Quaternion.Euler(Vector3.zero));
        // добавляем полученный элемент в список "детей" родительского объекта
        _children.Add(score_sector);
    }

    private void RandomRotation()
    {
        System.Random rnd = new System.Random();
        transform.Rotate(Vector3.up, (float)rnd.Next(0, 360));
    }

    private void ExploseChildren()
    {
        if (_is_explised && _children[_children.Count - 1].GetComponent<SectorEngine>().GetScoreStatus())
        {
            // проигрываем звук пролёта шарика
            _audioSystem.GetComponent<AudioEngine>().PlayPlatformBreak();

            _is_explised = false; // закрываем флаш взрыва лепестков
            for (int i = 0; i < _children.Count - 1; ++i)
            {
                // выключаем работу триггеров
                _children[i].GetComponent<SectorEngine>().SetEnable(false);
                // переносим на слой IgnoreRayCast
                _children[i].layer = 2;
                // отвязываем трансформ от родителя
                _children[i].transform.SetParent(null);
                // создаём рижитбади чтобы воздействовать силой
                _children[i].AddComponent<Rigidbody>();
                // задаём массу объекта
                _children[i].GetComponent<Rigidbody>().mass = 5;
                
                // берем компоненты опорных точек секции
                Vector3 from = _children[i].GetComponent<SectorEngine>().GetAxisPoint().position;
                Vector3 to = _children[i].GetComponent<SectorEngine>().GetEdgePoint().position;

                System.Random rnd = new System.Random();
                 _children[i].GetComponent<Rigidbody>()   // пинаем его силой
                    .AddForce(((to - from).normalized * rnd.Next(35, 65)) + new Vector3(0, rnd.Next(-65, 65), 0), ForceMode.Impulse);
            }

            DelayedDestroy();
        }
    }

    private void DelayedDestroy()
    {
        System.Random rnd = new System.Random();
        for (int i = 0; i < _children.Count - 1; ++i)
        {
            float time = rnd.Next(2, 7);

            Destroy(_children[i], time);

            if (_children[i].tag == "TowerSector")
            {
                _children[i].GetComponent<SectorEngine>().SetVanishingTime(time - 1);
            }
        }
    }

    private struct DiffucultyMap
    {
        public DiffucultyMap(Int32 total, Int32 normals, Int32 alerts, Int32 spaces, Int32 s_size, Int32 a_size)
        {
            _total_sectors_count = total;
            _normals_count = normals;
            _alerts_count = alerts;
            _spaces_count = spaces;
            _max_spaces_size = s_size;
            _max_alert_size = a_size;
        }

        public Int32 _total_sectors_count;
        public Int32 _normals_count;
        public Int32 _alerts_count;
        public Int32 _spaces_count;
        public Int32 _max_spaces_size;
        public Int32 _max_alert_size;
    }

    private Dictionary<Int32, DiffucultyMap> _DIFF_PARAM_
        = new Dictionary<Int32, DiffucultyMap>()
        {
            { 0, new DiffucultyMap( 6, 3, 0, 3, 4, 0) },
            { 1, new DiffucultyMap( 8, 5, 0, 3, 4, 0) },
            { 2, new DiffucultyMap( 10, 6, 1, 3, 3, 0) },
            { 3, new DiffucultyMap( 10, 6, 1, 3, 3, 1) },
            { 4, new DiffucultyMap( 10, 6, 2, 2, 2, 1) },
            { 5, new DiffucultyMap( 10, 6, 2, 2, 2, 2) },
            { 6, new DiffucultyMap( 12, 7, 3, 2, 1, 2) },
            { 7, new DiffucultyMap( 12, 7, 3, 2, 1, 2) },
            { 8, new DiffucultyMap( 12, 7, 4, 1, 0, 3) },
            { 9, new DiffucultyMap( 14, 8, 5, 1, 0, 4) }
        };

    private struct SectorsCalculate
    {
        public SectorsCalculate(Int32 s0, Int32 s1, Int32 s2, Int32 s3, Int32 s4)
        {
            _s0_count = s0;
            _s1_count = s1;
            _s2_count = s2;
            _s3_count = s3;
            _s4_count = s4;
        }

        public Int32 _s0_count;
        public Int32 _s1_count;
        public Int32 _s2_count;
        public Int32 _s3_count;
        public Int32 _s4_count;
    }

    private Dictionary<Int32, SectorsCalculate> _SECTOR_CALC_
        = new Dictionary<Int32, SectorsCalculate>()
        {
            { 0, new SectorsCalculate( 0, 0, 0, 0, 6) },
            { 1, new SectorsCalculate( 0, 2, 2, 0, 4) },
            { 2, new SectorsCalculate( 2, 2, 2, 2, 2) },
            { 3, new SectorsCalculate( 2, 1, 3, 3, 1) },
            { 4, new SectorsCalculate( 1, 2, 4, 2, 1) },
            { 5, new SectorsCalculate( 1, 2, 3, 4, 0) },
            { 6, new SectorsCalculate( 3, 3, 3, 3, 0) },
            { 7, new SectorsCalculate( 2, 4, 4, 2, 0) },
            { 8, new SectorsCalculate( 4, 1, 4, 3, 0) },
            { 9, new SectorsCalculate( 6, 2, 4, 2, 0) }
        };

    private SectorEngine._sectorType GetRandomSectorType()
    {
        System.Random rnd = new System.Random();
        Int32 count = rnd.Next(1, 4);

        switch (count)
        {
            case 0:
                return SectorEngine._sectorType.basic;
            case 1:
                return SectorEngine._sectorType.empty;
            case 2:
                return SectorEngine._sectorType.normal;
            case 3:
                return SectorEngine._sectorType.alert;
            default:
                return SectorEngine._sectorType.basic;

        }
    }

    private Int32 GetRandomSectorSize()
    {
        System.Random rnd = new System.Random();
        Int32 count = rnd.Next(0, 5);

        switch (count)
        {
            case 0:
                return 0;
            case 1:
                return 1;
            case 2:
                return 2;
            case 3:
                return 3;
            case 4:
                return 4;
            default:
                return 0;
        }
    }

    private bool SectorTypeIsApprove(SectorEngine._sectorType type, ref DiffucultyMap map)
    {
        switch(type)
        {
            case SectorEngine._sectorType.empty:
                return map._spaces_count > 0;
            case SectorEngine._sectorType.normal:
                return map._normals_count > 0;
            case SectorEngine._sectorType.alert:
                return map._alerts_count > 0;
            default:
                return false;
        }
    }

    private bool SectorSizeIsApprove(Int32 size, ref SectorsCalculate calc)
    {
        switch (size) 
        {
            case 0:
                return calc._s0_count > 0;
            case 1:
                return calc._s1_count > 0;
            case 2: 
                return calc._s2_count > 0;
            case 3:
                return calc._s3_count > 0;
            case 4:
                return calc._s4_count > 0;
            default: 
                return false;
        }
    }

    private void ActualizeDiffucultyMap(SectorEngine._sectorType type, ref DiffucultyMap map)
    {
        switch (type)
        {
            case SectorEngine._sectorType.empty:
                --map._spaces_count;
                break;
            case SectorEngine._sectorType.normal:
                --map._normals_count;
                break;
            case SectorEngine._sectorType.alert:
                --map._alerts_count;
                break;
            default:
                break;
        }
    }

    private void ActualizeSectorsCalculate(Int32 size, ref SectorsCalculate calc)
    {
        switch (size)
        {
            case 0:
                --calc._s0_count;
                break;
            case 1:
                --calc._s1_count;
                break;
            case 2:
                --calc._s2_count;
                break;
            case 3:
                --calc._s3_count;
                break;
            case 4:
                --calc._s4_count;
                break;
            default:
                break;
        }
    }
}