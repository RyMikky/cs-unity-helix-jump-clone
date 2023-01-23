using OpenCover.Framework.Model;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AquaScreenEngine : MonoBehaviour
{

    public enum Mode
    {
        none, win, lose
    }
    public Mode _mode = Mode.none;

    public RawImage _patheticAqua;                   // мордашка вредной аквы
    public TextMeshProUGUI _aquaSarcasm;             // издёвка вредной аквы
    public List<Texture> _aquaVariant;               // варианты мордашки вредной аквы

    public Button _nextButton;                       // кнопка перехода на след уровень
    public Button _restartButton;                    // кнопка перезапуска уровня
    public Button _exitButton;                       // кнопка выхода

    public void SetScreenMode(AquaScreenEngine.Mode mode) { _mode = mode; }

    private List<string> _aquasReplys = new List<string>()
    { "Ой, ну я даже и не знаю...\nКонечно, ты случайно ошибся\nВпрочем, что еще ожидать от Хиккинита?\n\nПопробуешь еще разок?" ,
        "Ты серьезно?!\nТак мало очков?!\nХочешь попытаться еще раз?!\nНе смеши меня Хиккинит...\n\nВпрочем, почему бы и нет?",
        "Будем считать, что справился\nОчков маловато, но пойдёт\nMожет быть дальше повезет?\n\n\nCледующий уровень?",
        "Не может быть?!\nPазве такое возможно?!\nMаксимум очков?!\nТы победил!\n\nCледующий уровень?"};

    void Update()
    {
        Engine();
    }

    private void Engine()
    {
        switch (_mode)
        {
            case Mode.none:
                if (_nextButton.gameObject.activeSelf) _nextButton.gameObject.SetActive(false);
                if (_restartButton.gameObject.activeSelf) _restartButton.gameObject.SetActive(false);
                if (_exitButton.gameObject.activeSelf) _exitButton.gameObject.SetActive(false);

                break;
            case Mode.win:
                if (_patheticAqua.texture != _aquaVariant[2]) _patheticAqua.texture = _aquaVariant[2];
                if (_aquaSarcasm.text != _aquasReplys[2]) _aquaSarcasm.text = _aquasReplys[2];

                if (!_nextButton.gameObject.activeSelf) _nextButton.gameObject.SetActive(true);
                if (!_restartButton.gameObject.activeSelf) _restartButton.gameObject.SetActive(true);
                if (!_exitButton.gameObject.activeSelf) _exitButton.gameObject.SetActive(true);

                break;
            case Mode.lose:
                if (_patheticAqua.texture != _aquaVariant[0]) _patheticAqua.texture = _aquaVariant[0];
                if (_aquaSarcasm.text != _aquasReplys[0]) _aquaSarcasm.text = _aquasReplys[0];

                if (_nextButton.gameObject.activeSelf) _nextButton.gameObject.SetActive(false);
                if (!_restartButton.gameObject.activeSelf) _restartButton.gameObject.SetActive(true);
                if (!_exitButton.gameObject.activeSelf) _exitButton.gameObject.SetActive(true);

                break;
        }
    }
}