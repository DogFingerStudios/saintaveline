/// ------------------------------
/// Original Author: Matthew Vale
/// ------------------------------

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameTimeController : MonoBehaviour
{
    #region Instancing

    public static GameTimeController Instance { get; private set; }

    #endregion

    #region Private Properties

    private int daylightHours;
    private float gameMinutesInDaylightHours;
    private float gameMinutesInNightHours;

    private bool isTimeRunning = false;
    private ushort currentHour;
    private float currentMinute;
    private string AMPM;
    private string currentTimeString;
    // For normalizing the time of day 0 to 1 (used for curve-transitions)
    private float currentDaylightHoursTimeStep; // of 1440 (full day in mins)
    private float currentNightHoursTimeStep; // of 1440 (full day in mins)
    private float normalizedDaylightTimeStep;
    private float normalizedNightTimeStep;

    private byte currentDay = 1;
    private uint totalDaysRunning = 1;
    private uint currentWeek = 1;

    #endregion

    #region Public Properties

    [Header("--- TIME DETAILS")]
    [Tooltip("How many real-life seconds should it take for 1 minute of in-game time to pass? Clamped to reasonable values.")]
    [Range(0.05f, 60f)]
    public float realSecondsPerGameMinute = 1;
    [Tooltip("What time of day should we begin? (24 hour)")]
    public ushort startingHour = 6;
    [Tooltip("How many hours are in 1 in-game day?")]
    public ushort hoursPerDay = 24;
    [Range(4f, 10f)]
    public int sunriseHour = 5;
    [Range(12f, 22f)]
    public int sunsetHour = 17;

    [Header("DAYS / WEEKS")]
    public int daysInWeek = 7;
    //public Slider currentDaySlider;
    //public TextMeshProUGUI totalDaysText;
    //public TextMeshProUGUI currentWeekText;

    [Header("--- SUN OBJECTS")]
    public Transform sun;
    public Light sunLight;
    public Gradient sunLightGradient;
    public AnimationCurve sunIntensity;
    public AnimationCurve sunRotationCurve;

    [Header("--- MOON OBJECTS")]
    public Transform moon;
    public Light moonLight;
    public Gradient moonLightGradient;
    public AnimationCurve moonIntensity;
    public AnimationCurve moonRotationCurve;

    [Header("--- SKYBOX")]
    public Material skyboxMat;
    public Gradient groundGradient;
    public Gradient horizonGradient;
    public Gradient skyGradient;
    public string _groundColorString = "_GroundColor";
    public string _horizonColorString = "_HorizonColor";
    public string _skyColorString = "_SkyColor";

    [Header("--- FOG")]
    public Gradient fogGradient;
    public AnimationCurve linearFogIntensity;
    public AnimationCurve expFogIntensity;


    [Header("--- ENVIRONMENT LIGHTING")]
    public AnimationCurve environmentLightingIntensity;

    //[Header("--- UI ELEMENTS")]
    //public TextMeshProUGUI text_GameTime;

    #endregion

    #region Events

    public delegate void OnGameHourPassed();
    public static OnGameHourPassed gameHourPassed;
    public delegate void OnGameDayPassed();
    public static OnGameDayPassed gameDayPassed;
    public delegate void OnGameWeekPassed();
    public static OnGameWeekPassed gameWeekPassed;

    #endregion


    #region Unity Flow

    private void Awake()
    {
        Instance = this;

        InitListeners();
        Init();

        daylightHours = sunsetHour - sunriseHour;
        gameMinutesInDaylightHours = GetGameMinutesFromGameHours(daylightHours);
        gameMinutesInNightHours = GetGameMinutesFromGameHours(hoursPerDay - daylightHours);
        //currentDaySlider.maxValue = hoursPerDay * daysInWeek;
        UpdateCurrentDayOfWeekSlider(startingHour);
        UpdateDayText();
        UpdateWeekText();
    }

    private void Update()
    {
        if (isTimeRunning)
        {
            UpdateGameTime();
            UpdateSun();
            UpdateMoon();
            UpdateSkybox();
            UpdateFog();
            UpdateEnvironmentLighting();
        }
    }

    private void OnDestroy()
    {
        gameHourPassed -= HourTickDebug;
        gameDayPassed -= DayTickDebug;
        gameWeekPassed -= WeekTickDebug;

        // Reset all visual effects
        ResetEnvironmentLighting(0.5f);
        ResetFog(0.5f);
        ResetSkybox(0.5f);
        ResetSun(0.5f);
    }

    #endregion

    #region Private Methods

    private void InitListeners()
    {
        gameHourPassed += HourTickDebug;
        gameDayPassed += DayTickDebug;
        gameWeekPassed += WeekTickDebug;
    }

    private void Init()
    {
        currentHour = startingHour;
        currentMinute = 0;
        currentDaylightHoursTimeStep = 0;
        isTimeRunning = true;
    }

    private void UpdateGameTime()
    {
        currentMinute += Time.deltaTime / realSecondsPerGameMinute; // This works, don't change, issue's not here...

        // Check if new hour
        if (currentMinute >= 60)
        {
            HourPassed();
        }
        // Check if new day
        if (currentHour >= hoursPerDay)
        {
            DayPassed();
        }
        // Check day of week
        if (currentDay > daysInWeek)
        {
            WeekPassed();
        }

        // Display current time in game UI
        ConvertTimeToStringAndDisplay();

        // If we are in daylight hours, update sun brightness normalized value
        if (currentHour >= sunriseHour && currentHour < sunsetHour)
        {
            currentDaylightHoursTimeStep += Time.deltaTime / realSecondsPerGameMinute;
            normalizedDaylightTimeStep = Mathf.InverseLerp(0, gameMinutesInDaylightHours, currentDaylightHoursTimeStep);
            currentNightHoursTimeStep = 0;
        }
        else
        {
            currentNightHoursTimeStep += Time.deltaTime / realSecondsPerGameMinute;
            normalizedNightTimeStep = Mathf.InverseLerp(0, gameMinutesInNightHours, currentNightHoursTimeStep);
        }
    }

    private void HourPassed()
    {
        currentMinute = 0;
        currentHour++; // ...and increase the hour.

        //UpdateCurrentDayOfWeekSlider(currentDaySlider.value + 1);

        gameHourPassed(); // call event every hour
    }

    private void DayPassed()
    {
        currentHour = 0; // Reset current hour...
        currentDaylightHoursTimeStep = 0; // Reset current daylight hours step
        currentDay++; // Increment the current day count...
        totalDaysRunning++; // Increment total day count

        UpdateDayText();

        gameDayPassed(); // Call day event
    }

    private void WeekPassed()
    {
        currentWeek++;
        currentDay = 1;

        UpdateCurrentDayOfWeekSlider(0);
        UpdateWeekText();

        gameWeekPassed(); // Call week event
    }

    private void UpdateCurrentTimeAsString()
    {
        currentTimeString = currentHour.ToString("00") + ":" + currentMinute.ToString("00") + " " + AMPM;
    }

    private float GetGameMinutesFromGameHours(int hours)
    {
        return 60 * hours;
    }

    private void ConvertTimeToStringAndDisplay()
    {
        if (currentHour >= 0 && currentHour <= 11)
        {
            AMPM = " AM";
        }
        else
        {
            AMPM = " PM";
        }
        UpdateCurrentTimeAsString();
        //text_GameTime.text = currentTimeString;
    }

    private void UpdateCurrentDayOfWeekSlider(float value)
    {
        //currentDaySlider.value = value;
    }

    private void UpdateDayText()
    {
        //totalDaysText.text = "Day " + totalDaysRunning;
    }

    private void UpdateWeekText()
    {
        //currentWeekText.text = "Week " + currentWeek;
    }

    private void UpdateSun()
    {
        sun.localEulerAngles = new Vector3(0, sunRotationCurve.Evaluate(normalizedDaylightTimeStep), 0);
        sunLight.color = sunLightGradient.Evaluate(normalizedDaylightTimeStep);
        sunLight.intensity = sunIntensity.Evaluate(normalizedDaylightTimeStep);
    }

    private void ResetSun(float value)
    {
        sun.localEulerAngles = new Vector3(0, sunRotationCurve.Evaluate(value), 0);
        sunLight.color = sunLightGradient.Evaluate(value);
        sunLight.intensity = sunIntensity.Evaluate(value);
    }

    private void UpdateMoon()
    {
        moon.localEulerAngles = new Vector3(0, moonRotationCurve.Evaluate(normalizedNightTimeStep), 0);
        moonLight.color = moonLightGradient.Evaluate(normalizedNightTimeStep);
        moonLight.intensity = moonIntensity.Evaluate(normalizedNightTimeStep);
    }

    //private void ResetMoon(float value) {
    //    sun.localEulerAngles = new Vector3(0, sunRotationCurve.Evaluate(value), 0);
    //    sunLight.color = sunLightGradient.Evaluate(value);
    //    sunLight.intensity = sunIntensity.Evaluate(value);
    //}

    private void UpdateSkybox()
    {
        skyboxMat.SetColor(_groundColorString, groundGradient.Evaluate(normalizedDaylightTimeStep));
        skyboxMat.SetColor(_horizonColorString, horizonGradient.Evaluate(normalizedDaylightTimeStep));
        skyboxMat.SetColor(_skyColorString, skyGradient.Evaluate(normalizedDaylightTimeStep));
    }
    private void ResetSkybox(float value)
    {
        skyboxMat.SetColor(_groundColorString, groundGradient.Evaluate(value));
        skyboxMat.SetColor(_horizonColorString, horizonGradient.Evaluate(value));
        skyboxMat.SetColor(_skyColorString, skyGradient.Evaluate(value));
    }

    private void UpdateEnvironmentLighting()
    {
        RenderSettings.ambientIntensity = environmentLightingIntensity.Evaluate(normalizedDaylightTimeStep);
    }
    private void ResetEnvironmentLighting(float value)
    {
        RenderSettings.ambientIntensity = environmentLightingIntensity.Evaluate(value);
    }
    private void UpdateFog()
    {
        RenderSettings.fogColor = horizonGradient.Evaluate(normalizedDaylightTimeStep);
        RenderSettings.fogDensity = expFogIntensity.Evaluate(normalizedDaylightTimeStep);
    }
    private void ResetFog(float value)
    {
        RenderSettings.fogColor = horizonGradient.Evaluate(value);
        RenderSettings.fogDensity = expFogIntensity.Evaluate(value);
    }

    #endregion

    #region Public Methods

    public int GetCurrentHour()
    {
        return currentHour;
    }

    public float GetCurrentMinute()
    {
        return currentMinute;
    }

    public bool IsNightTime()
    {
        if (GetCurrentHour() >= sunsetHour || GetCurrentHour() <= sunriseHour)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion

    #region Debug

    private void HourTickDebug()
    {
        // One hour has passed
    }

    private void DayTickDebug()
    {
        // One day has passed
    }

    private void WeekTickDebug()
    {
        // One week has passed
    }

    #endregion

}