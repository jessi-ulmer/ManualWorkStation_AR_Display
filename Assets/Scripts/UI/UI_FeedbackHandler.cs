using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_FeedbackHandler : MonoBehaviour 
{
    public GameObject general_feedback_elements;
    public GameObject gamified_feedback_elements;
    public Sprite level_0_sprite;
    public Sprite level_1_sprite;
    public Sprite level_2_sprite;
    public Sprite level_3_sprite;
    public Sprite level_4_sprite;
    public List<GameObject> ui_notifications;
    public List<GameObject> uncompleted_steps;
    
    private float max_number_points;

    // Prefabs
    private GameObject prefab_popup;
    private GameObject prefab_bar;

    // Elements in scene
    private GameObject popup_parent;
    private GameObject progressBar;
    private GameObject point_display;
    private GameObject levelup;
    private GameObject perfect_run;
    private GameObject thank_you;
    private GameObject training_finished;
    private GameObject wrong_action;
    private GameObject correct_action;
    private GameObject error_plane;
    private GameObject current_level;
    private GameObject timer;
    private GameObject quality_display;
    private GameObject time_display;
    private GameObject max_points;
    private GameObject current_points;
    private GameObject levelname;
    private GameObject levelimage;


    void Awake()
    {
    }

	void Start () 
	{
        // Load prefabs
        prefab_bar = (GameObject)Resources.Load("Prefabs/UI/bar", typeof(GameObject));
        prefab_popup = Resources.Load("Prefabs/UI/Popup", typeof(GameObject)) as GameObject;
        
        // Find elements in scene
        point_display = GameObject.Find("PointDisplay");
        quality_display = GameObject.Find("QualityDisplay");
        progressBar = GameObject.Find("StepDisplay");
        current_level = GameObject.Find("CurrentLevel");
        popup_parent = GameObject.Find("PopupParent");
        levelup = GameObject.Find("LevelUp");
        perfect_run = GameObject.Find("PerfectRun");
        thank_you = GameObject.Find("ThankYou");
        training_finished = GameObject.Find("TrainingFinished");
        wrong_action = GameObject.Find("WrongAction");
        error_plane = GameObject.Find("ErrorPlane");
        correct_action = GameObject.Find("CorrectAction");
        timer = GameObject.Find("Timer");
        quality_display = GameObject.Find("QualityDisplay");
        time_display = GameObject.Find("TimeDisplay");

        max_points = point_display.transform.Find("MaxPoints").gameObject;
        current_points = point_display.transform.Find("CurrentPoints").gameObject;
        levelname = current_level.transform.Find("LevelName").gameObject;
        levelimage = current_level.transform.Find("LevelImage").gameObject;

        // Disable unnecessary elements
        ui_notifications.Add(error_plane);
        error_plane.SetActive(false);
    }

    public void SetMaxPoints(int points)
    {
        max_number_points = points;
        max_points.GetComponent<Text>().text = max_points.ToString();
    }

    public void ShowPoints(int points)
    {
        Text point_text = current_points.GetComponent<Text>();
        point_text.text = points.ToString();        
        float ratio = points / max_number_points;

        if (ratio > 0.8f)
        {
            point_text.color = Color.green;
        }
        else if (ratio > 0.4f)
        {
            point_text.color = Color.cyan;
        }
        else
        {
            point_text.color = Color.yellow;
        }
    }

    public void ShowLevel(int level)
    {        
        if (level == 0)
        {
            levelname.GetComponent<Text>().text = "Level 0 - Introduction";
            levelimage.GetComponent<Image>().sprite = level_0_sprite;
        }
        if (level == 1)
        {
            levelname.GetComponent<Text>().text = "Level 1 - Beginner";
            levelimage.GetComponent<Image>().sprite = level_1_sprite;
        }
        else if ( level == 2)
        {
            levelname.GetComponent<Text>().text = "Level 2 - Advanced";
            levelimage.GetComponent<Image>().sprite = level_2_sprite;
        }
        else if (level == 3)
        {
            levelname.GetComponent<Text>().text = "Level 3 - Very Advanced";
            levelimage.GetComponent<Image>().sprite = level_3_sprite;
        }
        else if (level == 4)
        {
            levelname.GetComponent<Text>().text = "Level 4 - Expert";
            levelimage.GetComponent<Image>().sprite = level_4_sprite;
        }
    }

	public void ShowNumberSteps(int number)
    {
        uncompleted_steps.Clear();
        GameObject new_bar;

        for (int i = 0; i < number; i++)
        {
            new_bar = Instantiate(prefab_bar, progressBar.transform);
            new_bar.name = "Bar_" + i;
            uncompleted_steps.Add(new_bar);
        }
    }

    public void ResetNumberSteps()
    {
        Debug.Log("Reset number of steps");
        foreach(Transform point in progressBar.transform)
        {
            if(point.name.Contains("Bar"))
            {
                Destroy(point.gameObject);
            }            
        }
    }

    public void FinishStep()
    {
        if(uncompleted_steps[0] != null)
        {
            uncompleted_steps[0].GetComponent<Image>().color = Color.green;
            uncompleted_steps.RemoveAt(0);
        }
        else
        {
            Debug.LogWarning("List for step display not found");
            Debug.Log(uncompleted_steps);
        }
    }

    public void StartTimer(int duration_seconds)
    {
        if(timer != null)
        {
            timer.GetComponent<UI_Timer>().StartTimer(duration_seconds);
        }
    }

    public void InitializeQualityRate(float threshold_yellow, float threshold_red)
    {
        quality_display.GetComponent<UI_CircularDisplay>().InitializeCircularDisplay(threshold_yellow, threshold_red);
        quality_display.GetComponent<UI_CircularDisplay>().UpdateCircularDisplay(1);
    }

    public void ShowQualityRate(float quality_rate)
    {
        quality_display.GetComponent<UI_CircularDisplay>().UpdateCircularDisplay(quality_rate);
    }

    public void InitializeTimeRate(float threshold_yellow, float threshold_red)
    {
        time_display.GetComponent<UI_CircularDisplay>().InitializeCircularDisplay(threshold_yellow, threshold_red);
        time_display.GetComponent<UI_CircularDisplay>().UpdateCircularDisplay(1);
    }

    public void ShowTimeRate(float quality_rate)
    {
        time_display.GetComponent<UI_CircularDisplay>().UpdateCircularDisplay(quality_rate);
    }

    public void DisplayPopup(string message, float color_r, float color_g, float color_b)
    {
        GameObject go = Instantiate(prefab_popup, popup_parent.transform.position, Quaternion.identity, popup_parent.transform);
        Color col = new Color(color_r, color_g, color_b);
        go.GetComponent<UI_Popup>().Setup(message, col);
    }

    public void DisplayLevelup()
    {
        levelup.GetComponent<UI_Confetti>().ShowConfetti();
    }

    public void DisplayPerfectRun()
    {
        perfect_run.GetComponent<UI_Confetti>().ShowConfetti();
    }

    public void DisplayThankYou()
    {
        thank_you.GetComponent<UI_Confetti>().ShowConfetti();
    }

    public void DisplayTrainingFinished()
    {
        training_finished.GetComponent<UI_Confetti>().ShowConfetti();
        
        GameObject.Find("Assemblies").SetActive(false);
    }

    public void NotifyCorrectAction()
    {
        correct_action.GetComponent<AudioSource>().Play();        
    }

    public void NotifyWrongAction()
    {
        wrong_action.GetComponent<AudioSource>().Play();
        error_plane.SetActive(true);
    }

    public void ResetNotifications()
    {
        foreach(GameObject notification in ui_notifications)
        {
            notification.SetActive(false);
        }
    }

    public void EnableGamification()
    {
        gamified_feedback_elements.SetActive(true);
        levelup.SetActive(true);
        perfect_run.SetActive(true);
        popup_parent.SetActive(true);
    }

    public void DisableGamification()
    {
        gamified_feedback_elements.SetActive(false); 
        levelup.SetActive(false);
        perfect_run.SetActive(false);
        popup_parent.SetActive(false);
    }

    public void ResetFeedbackElements()
    {
        ResetNotifications();
        ResetNumberSteps();
        max_points.GetComponent<Text>().text = "";
        current_points.GetComponent<Text>().text = "";
        levelname.GetComponent<Text>().text = "Beginner";
        levelimage.GetComponent<Image>().sprite = level_1_sprite;
        quality_display.GetComponent<UI_CircularDisplay>().UpdateCircularDisplay(1);
        time_display.GetComponent<UI_CircularDisplay>().UpdateCircularDisplay(1);
    }
}
