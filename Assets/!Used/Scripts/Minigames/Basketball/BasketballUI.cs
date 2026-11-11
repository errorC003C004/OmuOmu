using System;
using UnityEngine;
using TMPro;

public class BasketballUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreCounter;
    [SerializeField] private TextMeshPro scoreCounter3d;
    [SerializeField] private int score;
    [SerializeField] private int targetScore = 15;
    public bool isTimerRunning;

    public float startingTime = 60f;

    private float timer;

    void Start()
    {
        ScoreTextConvert();
        ConvertTimeToClock();
    }

    public void StartTimer()
    {
        timer = startingTime;
        isTimerRunning = true;
        ConvertTimeToClock();
    }

    public void UpdateTimer()
    {
        if (!isTimerRunning) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            timer = 0f;
            StopTimer();
            //
        }

        ConvertTimeToClock();
    }

    public void ResetTimer()
    {
        timer = startingTime;
        ConvertTimeToClock();
    }

    public void StopTimer()
    {
        isTimerRunning = false;
        score = 0;
        ScoreTextConvert();
        ConvertTimeToClock();
    }

    private void Update()
    {
        if (isTimerRunning)
            UpdateTimer();
    }

    public void AddScore(int points)
    {
        score += points;
        ScoreTextConvert();
        if (score >= targetScore)
        {
            StopTimer();
        }
    }

    private void ScoreTextConvert()
    {
        if (scoreCounter != null)
            scoreCounter.text = $"{score}/{targetScore}";
        if (scoreCounter3d != null)
            scoreCounter3d.text = $"{score}/{targetScore}";
    }

    private void ConvertTimeToClock()
    {
        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}