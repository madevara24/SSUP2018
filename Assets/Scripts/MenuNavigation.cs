using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class MenuNavigation : MonoBehaviour {

    public enum ScheduleState { Closed, Talks, Works, Match }
    #region Variable
    private const int ENUM_STATUS_NOT_LOGGED_IN = 0;
    private const int ENUM_STATUS_LOGGED_IN = 1;

    [SerializeField] ScheduleState scheduleState = ScheduleState.Closed;

    [SerializeField] GameObject canvasHome, canvasInformation, canvasEvents, canvasSchedule;
    
    [SerializeField] Image imgScheduleContent;
    [SerializeField] Button btnStartTalk, btnStartWorks, btnStartMatch;

    [SerializeField] TouchDetector touchDetector;
    [SerializeField] List<Image> listOfImgSlider;
    [SerializeField] Image imgSliderCircle;
    [SerializeField] List<Sprite> listOfSliderCircleSprite;
    [SerializeField] List<Sprite> listOfScheduleContentSprite;


    private bool isGuest, isLoggedIn;
    private int bypassSignUpCounter;
    private bool isSwiping, isSliding = false;
    private int sliderImageId = 0;
    //public static int IdUI_AR;
    //public GameObject UIScore;
    #endregion
    // Use this for initialization
    void Start() {
        bypassSignUpCounter = 0;
        sliderImageId = 0;
        //DOTween.Init(true, true, LogBehaviour.Default);
    }

    // Update is called once per frame
    void Update() {/*
        if (!UIScore.active) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                IdUI_AR = 0;
                goBackToMainMenu();
            }
        }*/
        if (canvasHome.activeSelf)
            checkSwipe();
        //if (!canvasSplashScreen.activeSelf && !canvasSignUpOption.activeSelf && !canvasSignUp.activeSelf) {
            checkBackSwipe();
        //}
    }

    private void checkBackSwipe() {
        if (touchDetector.SwipeRight && touchDetector.StartTouch.x < Screen.width / 6) {/*
            if (canvasTreasureScore.activeSelf) {
                StartCoroutine(onBackSwipe(1));
            }
            else {
                StartCoroutine(onBackSwipe(0));
            }*/
            StartCoroutine(onBackSwipe(0));
        }
    }

    private IEnumerator onBackSwipe(int _phase) {
        yield return new WaitForSeconds(0.1f);
        switch (_phase) {
            case 0:
                goBackToMainMenu();
                break;
            case 1:
                //canvasTreasureScore.SetActive(false);
                break;
            default:
                goBackToMainMenu();
                break;
        }
    }

    public void deactivateScoreCanvas() {
        //canvasTreasureScore.SetActive(false);
    }

    public void goBackToMainMenu() {
        canvasHome.SetActive(true);
        canvasEvents.SetActive(false);
        canvasInformation.SetActive(false);
        canvasSchedule.SetActive(false);
        }

    private void deactivateAllMenuCanvas() {
        canvasHome.SetActive(false);
        canvasInformation.SetActive(false);
        canvasEvents.SetActive(false);
        canvasSchedule.SetActive(false);
    }

    private void initCanvas() {
    }

    private void initAr() {

        deactivateAllMenuCanvas();
    }

    
    #region Home Function
    private void initHomeMenu() {
        goBackToMainMenu();
    }

    private void checkSwipe() {
        if (!touchDetector.IsDraging)
            isSwiping = false;
        if (Input.touches.Length > 0 && Input.touches[0].position.y > Screen.height * 0.7) {
            if (touchDetector.SwipeRight && !isSwiping && !isSliding) {
                if (sliderImageId > 0) {
                    sliderImageId--;
                    slideImage(false);
                }
                isSwiping = true;
            }
            if (touchDetector.SwipeLeft && !isSwiping && !isSliding) {
                if (sliderImageId < listOfImgSlider.Count - 1) {
                    sliderImageId++;
                    slideImage(true);
                }
                isSwiping = true;
            }
        }
        //Debug.Log("Is Swiping " + isSwiping);
    }

    private void slideImage(bool _next) {
        string trace = "";
        Debug.Log("Slide " + _next);
        imgSliderCircle.sprite = listOfSliderCircleSprite[sliderImageId];
        isSliding = true;
        StartCoroutine(waitSlidingComplete());
        if (_next) {
            for (int i = 0; i < listOfImgSlider.Count; i++) {
                listOfImgSlider[i].rectTransform.DOLocalMoveX(listOfImgSlider[i].rectTransform.anchoredPosition.x - 664, 0.9f, true);
                trace += listOfImgSlider[i].rectTransform.anchoredPosition.x.ToString() + " -> " + (listOfImgSlider[i].rectTransform.anchoredPosition.x - 608).ToString() + "\n";
            }
        }
        else {
            for (int i = 0; i < listOfImgSlider.Count; i++) {
                listOfImgSlider[i].rectTransform.DOLocalMoveX(listOfImgSlider[i].rectTransform.anchoredPosition.x + 664, 0.9f, true);
                trace += listOfImgSlider[i].rectTransform.anchoredPosition.ToString() + " -> " + (listOfImgSlider[i].rectTransform.anchoredPosition.x + 608).ToString() + "\n";
            }
        }
        if (trace != "")
            Debug.Log(trace);
    }

    private IEnumerator waitSlidingComplete() {
        yield return new WaitForSeconds(1f);
        isSliding = false;
    }

    private bool checkSignUpField() {/*
        bool result = true;
        txtSignUpFeedback.text = "";
        for (int i = 0; i < listOfStarText.Count; i++) {
            listOfStarText[i].gameObject.SetActive(false);
        }

        if (inputSignUpName.text == "") {
            listOfStarText[0].gameObject.SetActive(true);
            txtSignUpFeedback.text = "Must fill required field(s)";
            result = false;
        }
        if (inputSignUpBirthday.text == "") {
            listOfStarText[1].gameObject.SetActive(true);
            txtSignUpFeedback.text = "Must fill required field(s)";
            result = false;
        }
        if (inputSignUpEmail.text == "" && !inputSignUpEmail.text.Contains("@")) {
            listOfStarText[2].gameObject.SetActive(true);
            txtSignUpFeedback.text = "Invalid Email";
            result = false;
        }
        if (inputSignUpPhoneNumber.text == "") {
            listOfStarText[3].gameObject.SetActive(true);
            txtSignUpFeedback.text = "Must fill required field(s)";
            result = false;
        }
        if (inputSignUpOcupation.text == "") {
            listOfStarText[4].gameObject.SetActive(true);
            txtSignUpFeedback.text = "Must fill required field(s)";
            result = false;
        }
        if (inputSignUpPassword.text == "") {
            listOfStarText[5].gameObject.SetActive(true);
            txtSignUpFeedback.text = "Must fill required field(s)";
            result = false;
        }
        if (inputSignUpConfirmPassword.text != inputSignUpPassword.text) {
            listOfStarText[6].gameObject.SetActive(true);
            txtSignUpFeedback.text = "Password didn't match";
            result = false;
        }
        return result;*/
        return false;
    }
    #endregion

    #region Button Function
    public void onPressBypassSignUp() {
        bypassSignUpCounter++;
        if (bypassSignUpCounter == 10) {/*
            StartCoroutine(RegisterBypassUser());
            playerSave();
            PlayerPrefs.SetInt("status", 1);
            PlayerPrefs.Save();
            PlayerPrefs.SetString("username", "bypass user");
            PlayerPrefs.Save();
            PlayerPrefs.SetString("password", "bypassuser");
            PlayerPrefs.Save();
            canvasSignUp.SetActive(false);*/
            canvasHome.SetActive(true);
            bypassSignUpCounter = 0;
        }
    }

    public void onPressLoginButton() {
        //LOGIN
        canvasHome.SetActive(true);
        //canvasLogIn.SetActive(false);
        isLoggedIn = true;

    }
    public void onPressSignUpGuestButton() {
        //LOGIN GUEST
        isGuest = true;
        isLoggedIn = true;
        PlayerPrefs.SetInt("status", 0);
        PlayerPrefs.Save();
        canvasHome.SetActive(true);
        //canvasSignUpOption.SetActive(false);
    }
    public void onPressSignUpButton() {/*
        canvasSignUp.SetActive(true);
        canvasLogIn.SetActive(false);*/
    }
    public void onPressSignUpGoogleButton() {/*
        canvasSignUp.SetActive(true);
        canvasSignUpOption.SetActive(false);*/
    }
    public void onPressSignUpFacebookButton() {/*
        canvasSignUp.SetActive(true);
        canvasSignUpOption.SetActive(false);*/
    }
    public void onPressSignUpEmailButton() {/*
        canvasSignUp.SetActive(true);
        canvasSignUpOption.SetActive(false);*/
    }
    public void onPressSubmitSignUpButton() {/*
        if (checkSignUpField()) {
            StartCoroutine(RegisterUser());
            playerSave();
            PlayerPrefs.SetInt("status", 1);
            PlayerPrefs.Save();
            PlayerPrefs.SetString("username", inputSignUpName.text);
            PlayerPrefs.Save();
            PlayerPrefs.SetString("password", inputSignUpPassword.text);
            PlayerPrefs.Save();
            canvasSignUp.SetActive(false);
        }*/
        canvasHome.SetActive(true);
    }


    public void onPressEventsButton() {
        canvasEvents.SetActive(true);
        canvasHome.SetActive(false);
    }
    public void onPressScheduleButton() {
        canvasSchedule.SetActive(true);
        canvasHome.SetActive(false);
    }
    public void onPressArExperienceButton() {
        if (!isGuest) {
            initAr();
            //IdUI_AR = 1;
        }
    }
    public void onPressInformationButton() {
        canvasInformation.SetActive(true);
        canvasHome.SetActive(false);
    }
    public void onPressBackButton() {
        goBackToMainMenu();
    }
    public void onPressArMapButton() {
        initAr();
        //IdUI_AR = 1;
    }
    public void onPressArEventInformationButton() {
        initAr();
        //IdUI_AR = 1;
    }
    public void onPressTreasureHuntButton() {
        if (!isGuest) {
            initAr();
            //IdUI_AR = 2;
        }

    }
    public void onPressStartTalkButton() {
        if (scheduleState == ScheduleState.Talks)
            changeScheduleState(ScheduleState.Closed);
        else
            changeScheduleState(ScheduleState.Talks);
    }
    public void onPressStartWorksButton() {
        if (scheduleState == ScheduleState.Works)
            changeScheduleState(ScheduleState.Closed);
        else
            changeScheduleState(ScheduleState.Works);
    }
    public void onPressStartMatchButton() {
        if (scheduleState == ScheduleState.Match)
            changeScheduleState(ScheduleState.Closed);
        else
            changeScheduleState(ScheduleState.Match);
    }
    #endregion
    private void changeScheduleState(ScheduleState _state) {
        scheduleState = _state;
        arrangeScheduleMenu();
    }

    private void arrangeScheduleMenu() {
        switch (scheduleState) {
            case ScheduleState.Closed:
                imgScheduleContent.sprite = listOfScheduleContentSprite[0];
                btnStartTalk.GetComponent<RectTransform>().anchoredPosition = new Vector3(-40f, -230f);
                btnStartWorks.GetComponent<RectTransform>().anchoredPosition = new Vector3(-40f, -329f);
                btnStartMatch.GetComponent<RectTransform>().anchoredPosition = new Vector3(-40f, -427f);
                break;
            case ScheduleState.Talks:
                imgScheduleContent.sprite = listOfScheduleContentSprite[1];
                btnStartTalk.GetComponent<RectTransform>().anchoredPosition = new Vector3(-40f, -230f);
                btnStartWorks.GetComponent<RectTransform>().anchoredPosition = new Vector3(-40f, -1124f);
                btnStartMatch.GetComponent<RectTransform>().anchoredPosition = new Vector3(-40f, -1219f);
                break;
            case ScheduleState.Works:
                imgScheduleContent.sprite = listOfScheduleContentSprite[2];
                btnStartTalk.GetComponent<RectTransform>().anchoredPosition = new Vector3(-40f, -230f);
                btnStartWorks.GetComponent<RectTransform>().anchoredPosition = new Vector3(-40f, -329f);
                btnStartMatch.GetComponent<RectTransform>().anchoredPosition = new Vector3(-40f, -803f);
                break;
            case ScheduleState.Match:
                imgScheduleContent.sprite = listOfScheduleContentSprite[3];
                btnStartTalk.GetComponent<RectTransform>().anchoredPosition = new Vector3(-40f, -230f);
                btnStartWorks.GetComponent<RectTransform>().anchoredPosition = new Vector3(-40f, -329f);
                btnStartMatch.GetComponent<RectTransform>().anchoredPosition = new Vector3(-40f, -427f);
                break;
            default:
                break;
        }
    }
}
