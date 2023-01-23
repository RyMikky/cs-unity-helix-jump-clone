using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TowerEngine : MonoBehaviour
{

    public float _mouseSence = 2f;                              // чувствительность мыши
    public bool _inverseRotation = false;                       // инверсировать вращение
    public bool _enableRotation = false;                        // флаг включения

    public GameObject _towerRoot;                               // объект для создания стержня
    public GameObject _sectorRing;                              // объект кольцевого модуля

    public enum Mode
    {
        basic, test, procedure
    }

    public Mode _executionMode;                                 // режим построения
    public Int32 _towerHeight = 105;                            // высота создаваемой башни
    public Int32 _ringOffset = 10;                              // расстояние между кольцами секций

    public Int32 _minDifficulty = 0;                            // минимальный уровень сложности
    public Int32 _maxDifficulty = 9;                            // максимальный уровень сложности

    private List<Int32> _difficultyLine = new List<Int32>();    // список сложностей по нарастанию
    //private bool _IsConstructed = false;                        // флаг того, что объект построен
    private Vector3 _lastMousePosition;                         // поле предыдущей позиции мыши
    private List<GameObject> _tower = new List<GameObject>();   // лист созданных компонентов

    public TowerEngine SetTowerHeight(Int32 height) { _towerHeight = height; return this; }
    public Int32 GetTowerHeight() { return _towerHeight; }
    public TowerEngine SetRingsOffset(Int32 offset) { _ringOffset = offset; return this; }
    public TowerEngine SetMinDifficulty(Int32 min) { _minDifficulty = min; return this; }
    public TowerEngine SetMaxDifficulty(Int32 max) { _maxDifficulty = max; return this; }
    public TowerEngine SetExecutionMode(Mode mode) { _executionMode = mode; return this; }
    public TowerEngine SetTowerRotation(bool enable) { _enableRotation = enable; return this; }

    public TowerEngine DestroyTower()
    {
        foreach(var item in _tower)
        {
            // отвязываемся от наследования
            item.transform.SetParent(null);
            // удаляем компонент
            DestroyImmediate(item);
        }

        _tower.Clear();

        return this;
    }

    public TowerEngine ReconstructTower()
    {
        DestroyTower();
        TowerConstructor();
        return this;
    }

    private void Update()
    {
        if (_enableRotation)   // если вращение разрешено
        {
            TowerRotation2();  // поворачиваем мышкой
        }
    }

    public void TowerConstructor()
    {
        switch (_executionMode)
        {
            case Mode.basic:
                BasicModeCtor();
                break;
            case Mode.test:
                TestModeCtor();
                break;
            case Mode.procedure:
                ProcedureCtor();
                break;
        }

        //_IsConstructed = true;
    }

    private void BasicModeCtor()
    {
        MakeTowerRoot(25);                                               // создаём ствол высотой 25
        MakeTowerRings(
            GetRingsCountByHeight(25), 
            -_ringOffset, RingEngine.Mode.simple);                       // создаём 5 базовых колец
    }

    private void TestModeCtor()
    {
        MakeTowerRoot(55);                                               // создаём ствол высотой 55
        MakeTowerRings(
            GetRingsCountByHeight(55),
            -_ringOffset, 0, 9, RingEngine.Mode.procedure);              // создаём 10 базовых колец
    }

    private void ProcedureCtor()
    {
        // создаём ствол высотой по готовым параметрам из публичных полей
        ProcedureCtor(_towerHeight, _ringOffset, _minDifficulty, _maxDifficulty);
    }
    private void ProcedureCtor(Int32 hight, Int32 offset, Int32 min, Int32 max)
    {
        // создаём ствол высотой по переданным параметрам
        MakeTowerRoot(hight);
        MakeTowerRings(
            GetRingsCountByHeight(hight),
            -offset, min, max, RingEngine.Mode.procedure);
    }

    // создаёт ствол уровня согласно переданной высоте уровня
    private void MakeTowerRoot(Int32 height)
    {
        // создаем объект - стержень уровня
        GameObject new_tower = Instantiate(_towerRoot, transform) as GameObject;
        // скалируем его под требуемый размер
        new_tower.transform.localScale = new Vector3(1, height, 1);
        // добавляем созданный рут в лист элементов
        _tower.Add(new_tower);
    }

    // перегрузка для вызова с базисными значениями из полей
    private void MakeTowerRings(Int32 count, Int32 position, RingEngine.Mode mode)
    {
        MakeTowerRings(count, position, _minDifficulty, _maxDifficulty, mode);
    }
    // создаёт кольца для уровня, принимает на вход количество колец, стартовую позицию и режим работы колец
    private void MakeTowerRings(Int32 count, Int32 position, Int32 min_dif, Int32 max_dif, RingEngine.Mode mode)
    {
        // если передается процедурный режим то количество уменьшаем на единицу
        // так как расчитывается с крайним элементом конца уровня - finish
        if (mode == RingEngine.Mode.procedure)
        {
            --count; // декремируем и запускаем расчёт листа сложностей
            DifficultyLineCalculation(min_dif, max_dif, count);
        }
            

        System.Random rnd = new System.Random();
        // запускаем создание колец по переданному количеству
        for (Int32 i = 0; i < count; ++i)
        {
            // создаём объект - кольцо с лепестками
            GameObject new_ring = Instantiate(_sectorRing, transform) as GameObject;
            // задаём положение и случайный поворот кольца
            new_ring.transform.SetLocalPositionAndRotation
                (new Vector3(0, position, 0), Quaternion.Euler(new Vector3(0, rnd.Next(0, 359), 0)));

            if (mode == RingEngine.Mode.procedure)
                // если режим процедурный, то задаём сложность секции из подготовленного списка
                new_ring.GetComponent<RingEngine>().SetRingDifficulty(_difficultyLine[i]);

            // задаём режим работы кольца
            new_ring.GetComponent<RingEngine>().SetExecutionMode(mode);
            // увеличиваем отступ
            position -= _ringOffset;

            // добавляем созданное кольцо в лист элементов
            _tower.Add(new_ring);
        }

        // если передается процедурный режим, то в конце добавляем финишный модуль рекурсивным вызовом
        if (mode == RingEngine.Mode.procedure) MakeTowerRings(1, position, RingEngine.Mode.finish);
    }

    // возвращаем количество колец по переданной высоте стволя уровня
    private Int32 GetRingsCountByHeight(Int32 height)
    {
        // возвращает количество колец по высоте столба и промежутку между кольцами
        return height / (_ringOffset / 2);
    }

    // подготавливает лист сложностей для полученных значений минимальной, максимальной сложностей и количества колец
    private void DifficultyLineCalculation(Int32 min, Int32 max, Int32 count)
    {
        // считаем количество сложностей для будущего рассчёта
        // например для границы 2:8 будет 8 - 2 + 1  = 7 итого сложности 2, 3, 4, 5, 6, 7, 8
        Int32 diff_count = max - min + 1;

        _difficultyLine.Clear();            // обнуляем список на всякий пожарный

        // далее две ситуации - если колец меньше или равно количеству сложностей
        if (count <= diff_count)
        {
            for (Int32 i = 0; i < count; ++i) 
            {
                _difficultyLine.Add(min++); // просто добавляем в список по порядку
            }
        }
        // иначе предстоит расчёт точного количества и сложности
        else
        {
            // для начала определяем во сколько раз количество колец больше сложностей
            Int32 multiple = (Int32) (count / diff_count);   // даёт "высоту" базовой секции сложностей
            Int32 d_multi = multiple + 1;                    // даёт "высоту" сложностей плюс 1
            Int32 delta = (Int32) (count % diff_count);      // даёт увеличенных "столбцов" сложностей

            // для понимания возьмём минимум 2 и максимум 8, итого 7 сложностей
            // необходимо построить 12 колец секций, как будет происходить рассчёт

            // сложности        2       3       4       5       6       7       8

            // номера           1       3       5       7       9       11      12
            // секций           2       4       6       8       10 

            // базисно "высота" секций multiple = 1, но delta = 5, соответственно пять сложностей
            // а именно сложности 2, 3, 4, 5, 6 будут иметь по две секции колец - "высота" d_multy

            // также запускаем цикл набора сложностей, но с большими отличиями
            Int32 cuttent_d_multy = 0;                                 // текущая "высота" секции сложностей
            Int32 current_delta = 0;                                   // текущая дельта по высотности
            Int32 current_diff = min;                                  // текущий уровень сложности
            for (Int32 i = 0; i < count; ++i)
            {
                if (cuttent_d_multy < d_multi && current_delta < delta)
                {
                    // если текущая "высота" ниже увеличенной и текущая дельта позиция меньше максимальный дельты
                    _difficultyLine.Add(current_diff);                 // просто заносим сложность в лист
                    ++cuttent_d_multy;                                 // инкрементируем показатель текущей "высоты"
                }
                else
                {
                    // далее смотрим на возможную ситуацию текущая дельта может быть как меньше, так и равна полученной выше
                    if (current_delta < delta)
                    {
                        ++current_delta;                                   // инкрементируем текущую дельту
                        ++current_diff;                                    // инкрементируем сложность
                        cuttent_d_multy = 1;                               // переназначаем текущую высоту
                        _difficultyLine.Add(current_diff);                 // заносим сложность в лист
                    }
                    // если текущая дельта равна полученной, но текущая "высота" ниже расчётной базовой
                    else if (current_delta >= delta && cuttent_d_multy < multiple)
                    {
                        ++cuttent_d_multy;                                 // инкрементируем текущую высоту
                        _difficultyLine.Add(current_diff);                 // заносим сложность в лист
                    }
                    // если и текущая дельта равна полученной и "высота" сравнялась с базовой
                    else
                    {
                        ++current_diff;                                    // инкрементируем сложность
                        cuttent_d_multy = 1;                               // переназначаем текущую высоту
                        _difficultyLine.Add(current_diff);                 // заносим сложность в лист
                    }
                }
            }
        }
    }

    private void OnMouseDrag()
    {
        if (_enableRotation)   // если вращение разрешено
        {
            //TowerRotation();   // поворачиваем мышкой
        }
    }
    // базовая функция вращения башни уровня
    private void TowerRotation()
    {
        float horizontal = Input.GetAxis("Mouse X");            // берем компоненту мышки по Х
        if (_inverseRotation) horizontal = -horizontal;         // инверсируем вращение, если надо

        // вращаем с помощью перемещения курсора
        transform.RotateAround(transform.position, Vector3.up, -horizontal * _mouseSence * 300 * Time.deltaTime);
    }

    private void TowerRotation2()
    {
        if (Input.GetMouseButton(0)) 
        {
            float horizontal = (Input.mousePosition - _lastMousePosition).x;  // берем компоненту мышки по Х
            if (_inverseRotation) horizontal = -horizontal;                   // инверсируем вращение, если надо

            // вращаем с помощью перемещения курсора
            transform.Rotate(Vector3.up, -horizontal * _mouseSence);
            
        }

        _lastMousePosition = Input.mousePosition;
    }
}