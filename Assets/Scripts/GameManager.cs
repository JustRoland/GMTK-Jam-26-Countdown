using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private TextMeshProUGUI versionText;
    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private TextMeshProUGUI timeText;

    public readonly SimpleTimer Timer = new (TimerType.Countdown);
    private UnityAction _lostEvent;
    private bool _gameOver;
    
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        

        
        if (versionText) versionText.text = $"Számháború (Number Tag) : v{Application.version}";
    }


    private void OnEnable()
    {        
        Timer.SetTimer(0, 2, 0);
        _lostEvent = () => LoseGame(Team.Red);
        Timer.CountdownFinishedEvent.AddListener(_lostEvent);
        Timer.StartTimer();
    }

    private void OnDisable()
    {
        Timer.CountdownFinishedEvent.RemoveListener(_lostEvent);
        Timer.StopTimer();
    }

    private void Update()
    {
        Timer.UpdateTimer(Time.deltaTime);
        if (timeText) timeText.text = Timer.ReadTimeString(TimerDisplaySetting.MinSec);
    }

    public void WinGame(Team team)
    {
        if (_gameOver) return;
        print($"Team {team} wins!");
        if (!winText) return;
        winText.text = $"Team {team} wins!";
        winText.gameObject.SetActive(true);
        Timer.StopTimer();
    }

    public void LoseGame(Team team)
    {
        print($"Team {team} wins!");
        if (!winText) return;
        winText.text = $"Team {team} wins!";
        winText.gameObject.SetActive(true);
        _gameOver = true;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
