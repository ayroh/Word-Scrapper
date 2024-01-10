using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{


    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI Hint;
    [SerializeField] private TextMeshProUGUI endGameText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Settings")]
    [SerializeField] private TextMeshProUGUI vibrationText;
    [SerializeField] private TextMeshProUGUI soundText;
    private bool vibrationChoice = true;

    [Header("Buttons")]
    [SerializeField] private UnityEngine.UI.Button skipButton;
    [SerializeField] private UnityEngine.UI.Button hintButton;


    public void SetHint(string newHint) => Hint.text = newHint;

    public void SetEndGame(string newText) => endGameText.text = newText;


    #region Timer


    [Header("Timer")]
    [SerializeField] private float maxTimer = 30f;
    [SerializeField] private float correctAnswerTimerIncrease = 5f;
    [SerializeField] private float orderWrongAnswerTimerDecrease = .5f;
    [SerializeField] private float totalWrongAnswerTimerDecrease = 1f;
    IEnumerator timerCoroutine = null;
    private float timer;


    public void StopTimer()
    {
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
    }

    public void StartTimer()
    {
        if (timerCoroutine != null)
            StopTimer();
        
        StartCoroutine(timerCoroutine = Timer());
    }

    public void SetTimer(string newTime) => timerText.text = newTime;

    private Tween tw;
    public void EmphasizeTime()
    {
        if (tw == null || !tw.IsPlaying())
            (tw = DOTween.To(() => timerText.fontSize, x => timerText.fontSize = x, 110, .15f)).SetAutoKill(false).OnComplete(() => tw.PlayBackwards());
    }

    [SerializeField] private Canvas canvas;
    private float timeAnimationTime = 1f;


    private IEnumerator Timer()
    {
        PuzzleManager.instance.SetGameState(GameState.Started);

        while (0f < timer)
        {
            SetTimer(((int)timer + 1).ToString());
            timer -= Time.deltaTime;
            yield return null;
        }

        PuzzleManager.instance.SetGameState(GameState.Ended);
        
        SetEndGame("YOU LOST!");
        timerCoroutine = null;
        PuzzleManager.instance.BlowUpTower();
    }

    public void ChangeTimeAnimation(float DecrementTime)
    {
        TextMeshProUGUI timeAnimationTMP = PoolManager.instance.GetTimeAnimation();
        Vector3 newPosition;
        newPosition = timerText.rectTransform.position;
        Color newColor;
        if (DecrementTime < 0)
        {
            timeAnimationTMP.rectTransform.position = timerText.rectTransform.position;
            newPosition.x += 150;
            timeAnimationTMP.text = DecrementTime.ToString();
            newColor = Color.red;
            newColor.a = .4f;
            timeAnimationTMP.color = newColor;
            newColor.a = .9f;
        }
        else
        {
            newPosition.x -= 150;
            timeAnimationTMP.rectTransform.position = newPosition;
            newPosition.x += 150;
            timeAnimationTMP.text = "+" + DecrementTime.ToString();
            newColor = Color.green;
            timeAnimationTMP.color = newColor;
            newColor.a = .1f;
        }
        DOTween.To(() => timeAnimationTMP.rectTransform.position, x => timeAnimationTMP.rectTransform.position = x, newPosition, timeAnimationTime);
        DOTween.To(() => timeAnimationTMP.color, x => timeAnimationTMP.color = x, newColor, timeAnimationTime).OnComplete(() => PoolManager.instance.ReleaseTimeAnimation(timeAnimationTMP));
    }

    public float ChangeTime(TimeChange timeChange)
    {
        float changedTime = 0f;
        switch (timeChange)
        {
            case TimeChange.OrderWrong:
                changedTime = orderWrongAnswerTimerDecrease;
                timer -= orderWrongAnswerTimerDecrease;
                break;
            case TimeChange.TotalWrong:
                changedTime = totalWrongAnswerTimerDecrease;
                timer -= totalWrongAnswerTimerDecrease;
                break;
            case TimeChange.CorrectAnswer:
                changedTime = correctAnswerTimerIncrease;
                timer += correctAnswerTimerIncrease;
                break;
        }
        return changedTime;
    }

    public void ResetTimer()
    {
        timer = maxTimer;
    }

    #endregion

    #region Panel Buttons

    public void VibrationToggle()
    {
        vibrationChoice = !vibrationChoice;
        if (vibrationChoice)
            vibrationText.text = "Vibration On";
        else
            vibrationText.text = "Vibration Off";
    }

    public void SoundToggle()
    {
        if (SoundManager.instance.GetVolume(Source.Descend) == 0)
        {
            soundText.text = "Sound On";
            SoundManager.instance.SetVolume(Source.Descend, .5f);
            SoundManager.instance.SetVolume(Source.EndGame, .5f);
        }
        else
        {
            soundText.text = "Sound Off";
            SoundManager.instance.SetVolume(Source.Descend, 0f);
            SoundManager.instance.SetVolume(Source.EndGame, 0f);
        }
    }

    public async void CloseButtonForSeconds(InGameButton closeButton, float time)
    {
        Button button = null;

        switch (closeButton)
        {
            case InGameButton.Skip:
                button = skipButton;
                break;
            case InGameButton.Hint:
                button = hintButton;
                break;
        }

        button.enabled = false;
        await UniTask.WaitForSeconds(time);
        button.enabled = true;
    }

    #endregion
}

