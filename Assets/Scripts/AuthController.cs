using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class AuthController : MonoBehaviour {

    #region Variables
    [SerializeField] InputField inputSignUpName, inputSignUpBirthday, inputSignUpEmail, inputSignUpPhoneNumber, inputSignUpOcupation, inputSignUpPassword, inputSignUpConfirmPassword;
    [SerializeField] List<Text> listOfStarText;
    [SerializeField] Text txtSignUpFeedback;

    [SerializeField] GameObject canvasSplashScreen, canvasSignUpOption, canvasSignUp, canvasHome;

    private int bypassSignUpCounter;
    #endregion

    #region Local Data Enum
    private const string KEY_LOCAL_DATA_EMAIL = "KEY_LOCAL_DATA_EMAIL";
    private const string KEY_LOCAL_DATA_PASSWORD = "KEY_LOCAL_DATA_PASSWORD";
    private const string KEY_LOCAL_DATA_NAME = "KEY_LOCAL_DATA_NAME";
    private const string KEY_LOCAL_DATA_LOGIN_STATUS = "KEY_LOCAL_DATA_LOGIN_STATUS";

    private const string KEY_DATABASE = "UserData";

    private const string KEY_DATABASE_NAME = "KEY_DATABASE_NAME";
    private const string KEY_DATABASE_EMAIL = "KEY_DATABASE_EMAIL";
    private const string KEY_DATABASE_PASSWORD = "KEY_DATABASE_PASSWORD";
    private const string KEY_DATABASE_OCCUPATION = "KEY_DATABASE_OCCUPATION";
    private const string KEY_DATABASE_BIRTHDAY = "KEY_DATABASE_BIRTHDAY";
    private const string KEY_DATABASE_PHONE_NUMBER = "KEY_DATABASE_PHONE_NUMBER";
    private const string KEY_DATABASE_SCORE = "KEY_DATABASE_SCORE";

    private const int ENUM_STATUS_NOT_LOGGED_IN = 0;
    private const int ENUM_STATUS_LOGGED_IN = 1;
    #endregion

    #region Firebase Auth Variables
    protected Firebase.Auth.FirebaseAuth auth;
    private Firebase.Auth.FirebaseAuth otherAuth;
    protected Dictionary<string, Firebase.Auth.FirebaseUser> userByAuth =
      new Dictionary<string, Firebase.Auth.FirebaseUser>();
    private string logText = "";/*
    public Text emailText;
    public Text passwordText;
    public Text ConfirmPassText;*/
    protected string email = "";
    protected string password = "";
    protected string confirmpassword = "";
    protected string displayName = "";
    private bool fetchingToken = false;

    const int kMaxLogSize = 16382;
    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
    #endregion

    #region Firebase Database Variables
    ArrayList leaderBoard;

    private const int MaxScores = 5;

    private string name = "";
    private string birthday = "";
    private string occupation = "";
    private string phoneNumber = "";
    private int score = 0;

    private bool addScorePressed;
    #endregion

    // Use this for initialization
    void Start () {
        initCanvas();
        StartCoroutine(initSplashScreen());
        startFirebase();
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
        updateFirebaseAuth();
        updateFirebaseDatabase();
    }

    void OnDestroy() {
        onDestroyFirebaseAuth();
    }
    private void startFirebase() {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available) {
                InitializeFirebaseAuth();
                InitializeFirebaseDatabase();
            }
            else {
                Debug.LogError(
                  "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }
    #region Firebase Auth Functions

    private void onDestroyFirebaseAuth() {
        auth.StateChanged -= AuthStateChanged;
        auth.IdTokenChanged -= IdTokenChanged;
        auth = null;
    }
    private void updateFirebaseAuth() {
        email = inputSignUpEmail.text;
        password = inputSignUpPassword.text;
        confirmpassword = inputSignUpConfirmPassword.text;
    }
    void InitializeFirebaseAuth() {
        DebugLog("Setting up Firebase Auth");
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        auth.IdTokenChanged += IdTokenChanged;
        AuthStateChanged(this, null);
    }

    // Output text to the debug log text field, as well as the console.
    public void DebugLog(string s) {
        Debug.Log(s);
        logText += s + "\n";

        while (logText.Length > kMaxLogSize) {
            int index = logText.IndexOf("\n");
            logText = logText.Substring(index + 1);
        }
    }

    // Display user information.
    void DisplayUserInfo(Firebase.Auth.IUserInfo userInfo, int indentLevel) {
        string indent = new String(' ', indentLevel * 2);
        var userProperties = new Dictionary<string, string> {
      {"Display Name", userInfo.DisplayName},
      {"Email", userInfo.Email},
      {"Photo URL", userInfo.PhotoUrl != null ? userInfo.PhotoUrl.ToString() : null},
      {"Provider ID", userInfo.ProviderId},
      {"User ID", userInfo.UserId}
    };
        foreach (var property in userProperties) {
            if (!String.IsNullOrEmpty(property.Value)) {
                DebugLog(String.Format("{0}{1}: {2}", indent, property.Key, property.Value));
            }
        }
    }

    // Display a more detailed view of a FirebaseUser.
    void DisplayDetailedUserInfo(Firebase.Auth.FirebaseUser user, int indentLevel) {
        DisplayUserInfo(user, indentLevel);
        DebugLog("  Anonymous: " + user.IsAnonymous);
        DebugLog("  Email Verified: " + user.IsEmailVerified);
        var providerDataList = new List<Firebase.Auth.IUserInfo>(user.ProviderData);
        if (providerDataList.Count > 0) {
            DebugLog("  Provider Data:");
            foreach (var providerData in user.ProviderData) {
                DisplayUserInfo(providerData, indentLevel + 1);
            }
        }
    }

    // Track state changes of the auth object.
    void AuthStateChanged(object sender, System.EventArgs eventArgs) {
        Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
        Firebase.Auth.FirebaseUser user = null;
        if (senderAuth != null) userByAuth.TryGetValue(senderAuth.App.Name, out user);
        if (senderAuth == auth && senderAuth.CurrentUser != user) {
            bool signedIn = user != senderAuth.CurrentUser && senderAuth.CurrentUser != null;
            if (!signedIn && user != null) {
                DebugLog("Signed out " + user.UserId);
                //user is logged out, load login screen 
                //SceneManager.LoadSceneAsync("scene_01");
            }
            user = senderAuth.CurrentUser;
            userByAuth[senderAuth.App.Name] = user;
            if (signedIn) {
                DebugLog("Signed in " + user.UserId);
                displayName = user.DisplayName ?? "";
                DisplayDetailedUserInfo(user, 1);
            }
        }
    }

    // Track ID token changes.
    void IdTokenChanged(object sender, System.EventArgs eventArgs) {
        Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
        if (senderAuth == auth && senderAuth.CurrentUser != null && !fetchingToken) {
            senderAuth.CurrentUser.TokenAsync(false).ContinueWith(
              task => DebugLog(String.Format("Token[0:8] = {0}", task.Result.Substring(0, 8))));
        }
    }

    // Log the result of the specified task, returning true if the task
    // completed successfully, false otherwise.
    public bool LogTaskCompletion(Task task, string operation) {
        bool complete = false;
        if (task.IsCanceled) {
            DebugLog(operation + " canceled.");
        }
        else if (task.IsFaulted) {
            DebugLog(operation + " encounted an error.");
            foreach (Exception exception in task.Exception.Flatten().InnerExceptions) {

                string authErrorCode = "";
                Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;

                if (firebaseEx != null) {

                    authErrorCode = String.Format("AuthError.{0}: ",
                    ((Firebase.Auth.AuthError)firebaseEx.ErrorCode).ToString());
                }
                DebugLog(authErrorCode + exception.ToString());
            }
        }
        else if (task.IsCompleted) {
            DebugLog(operation + " completed");
            complete = true;
        }
        return complete;
    }
    public void CreateUserAsync() {
        if (password == confirmpassword) {
            DebugLog(String.Format("Attempting to create user {0}...", email));

            // This passes the current displayName through to HandleCreateUserAsync
            // so that it can be passed to UpdateUserProfile().  displayName will be
            // reset by AuthStateChanged() when the new user is created and signed in.
            string newDisplayName = displayName;
            auth.CreateUserWithEmailAndPasswordAsync(email, password)
              .ContinueWith((task) => {
                  return HandleCreateUserAsync(task, newDisplayName: newDisplayName);
              }).Unwrap();
        }

        else {
            DebugLog("Wrong Password");
        }
    }

    Task HandleCreateUserAsync(Task<Firebase.Auth.FirebaseUser> authTask,
                               string newDisplayName = null) {
        if (LogTaskCompletion(authTask, "User Creation")) {
            if (auth.CurrentUser != null) {
                DebugLog(String.Format("User Info: {0}  {1}", auth.CurrentUser.Email,
                                       auth.CurrentUser.ProviderId));
                return UpdateUserProfileAsync(newDisplayName: newDisplayName);
            }
        }
        // Nothing to update, so just return a completed Task.
        return Task.FromResult(0);
    }

    // Update the user's display name with the currently selected display name.
    public Task UpdateUserProfileAsync(string newDisplayName = null) {
        if (auth.CurrentUser == null) {
            DebugLog("Not signed in, unable to update user profile");
            return Task.FromResult(0);
        }
        displayName = newDisplayName ?? displayName;
        DebugLog("Updating user profile");
        return auth.CurrentUser.UpdateUserProfileAsync(new Firebase.Auth.UserProfile {
            DisplayName = displayName,
            PhotoUrl = auth.CurrentUser.PhotoUrl,
        }).ContinueWith(HandleUpdateUserProfile);
    }

    void HandleUpdateUserProfile(Task authTask) {
        if (LogTaskCompletion(authTask, "User profile")) {
            DisplayDetailedUserInfo(auth.CurrentUser, 1);
        }
    }

    public void SigninAsync() {
        DebugLog(String.Format("Attempting to sign in as {0}...", email));
        auth.SignInWithEmailAndPasswordAsync(email, password)
          .ContinueWith(HandleSigninResult);
    }

    void HandleSigninResult(Task<Firebase.Auth.FirebaseUser> authTask) {
        LogTaskCompletion(authTask, "Sign-in");
        SceneManager.LoadSceneAsync("scene_02");
    }

    public void ReloadUser() {
        if (auth.CurrentUser == null) {
            DebugLog("Not signed in, unable to reload user.");
            return;
        }
        DebugLog("Reload User Data");
        auth.CurrentUser.ReloadAsync().ContinueWith(HandleReloadUser);
    }

    void HandleReloadUser(Task authTask) {
        if (LogTaskCompletion(authTask, "Reload")) {
            DisplayDetailedUserInfo(auth.CurrentUser, 1);
        }
    }

    public void GetUserToken() {
        if (auth.CurrentUser == null) {
            DebugLog("Not signed in, unable to get token.");
            return;
        }
        DebugLog("Fetching user token");
        fetchingToken = true;
        auth.CurrentUser.TokenAsync(false).ContinueWith(HandleGetUserToken);
    }

    void HandleGetUserToken(Task<string> authTask) {
        fetchingToken = false;
        if (LogTaskCompletion(authTask, "User token fetch")) {
            DebugLog("Token = " + authTask.Result);
        }
    }

    void GetUserInfo() {
        if (auth.CurrentUser == null) {
            DebugLog("Not signed in, unable to get info.");
        }
        else {
            DebugLog("Current user info:");
            DisplayDetailedUserInfo(auth.CurrentUser, 1);
        }
    }

    public void SignOut() {
        DebugLog("Signing out.");
        auth.SignOut();
    }

    // Show the providers for the current email address.
    public void DisplayProvidersForEmail() {
        auth.FetchProvidersForEmailAsync(email).ContinueWith((authTask) => {
            if (LogTaskCompletion(authTask, "Fetch Providers")) {
                DebugLog(String.Format("Email Providers for '{0}':", email));
                foreach (string provider in authTask.Result) {
                    DebugLog(provider);
                }
            }
        });
    }
    #endregion

    #region Firebase Database Functions
    // Initialize the Firebase database:
    protected virtual void InitializeFirebaseDatabase() {
        FirebaseApp app = FirebaseApp.DefaultInstance;
        // NOTE: You'll need to replace this url with your Firebase App's database
        // path in order for the database connection to work correctly in editor.
        app.SetEditorDatabaseUrl("https://ssup2018-12aab.firebaseio.com/");
        if (app.Options.DatabaseUrl != null) app.SetEditorDatabaseUrl(app.Options.DatabaseUrl);
        StartListener();
    }

    private void updateFirebaseDatabase() {
        name = inputSignUpName.text;
        email = inputSignUpEmail.text;
        password = inputSignUpPassword.text;
        occupation = inputSignUpOcupation.text;
        birthday = inputSignUpBirthday.text;
        phoneNumber = inputSignUpPhoneNumber.text;
    }

    protected void StartListener() {
        FirebaseDatabase.DefaultInstance
          .GetReference(KEY_DATABASE).OrderByChild(KEY_DATABASE_SCORE)
          .ValueChanged += (object sender2, ValueChangedEventArgs e2) => {
              if (e2.DatabaseError != null) {
                  Debug.LogError(e2.DatabaseError.Message);
                  return;
              }
              Debug.Log("Received values for Leaders.");
              string title = leaderBoard[0].ToString();
              leaderBoard.Clear();
              leaderBoard.Add(title);
              if (e2.Snapshot != null && e2.Snapshot.ChildrenCount > 0) {
                  foreach (var childSnapshot in e2.Snapshot.Children) {
                      if (childSnapshot.Child("score") == null
                    || childSnapshot.Child("score").Value == null) {
                          Debug.LogError("Bad data in sample.  Did you forget to call SetEditorDatabaseUrl with your project id?");
                          break;
                      }
                      else {
                          Debug.Log("Leaders entry : " +
                        childSnapshot.Child("email").Value.ToString() + " - " +
                        childSnapshot.Child("score").Value.ToString());
                          leaderBoard.Insert(1, childSnapshot.Child("score").Value.ToString()
                        + "  " + childSnapshot.Child("email").Value.ToString());
                          
                          foreach (string item in leaderBoard) {

                          }
                      }
                  }
              }
          };
    }

    TransactionResult addNewUserDataTransaction(MutableData mutableData) {
        List<object> userData = mutableData.Value as List<object>;

        if (userData == null) {
            userData = new List<object>();
        }

        // Now we add the new score as a new entry that contains the email address and score.
        Dictionary<string, object> newUserDataMap = new Dictionary<string, object>();
        newUserDataMap[KEY_DATABASE_NAME] = name;
        newUserDataMap[KEY_DATABASE_EMAIL] = email;
        newUserDataMap[KEY_DATABASE_PASSWORD] = password;
        newUserDataMap[KEY_DATABASE_BIRTHDAY] = birthday;
        newUserDataMap[KEY_DATABASE_OCCUPATION] = occupation;
        newUserDataMap[KEY_DATABASE_PHONE_NUMBER] = phoneNumber;
        newUserDataMap[KEY_DATABASE_SCORE] = 0;
        userData.Add(newUserDataMap);

        // You must set the Value to indicate data at that location has changed.
        mutableData.Value = userData;
        //return and log success
        return TransactionResult.Success(mutableData);
    }

    TransactionResult AddScoreTransaction(MutableData mutableData) {
        List<object> leaders = mutableData.Value as List<object>;

        if (leaders == null) {
            leaders = new List<object>();
        }
        else if (mutableData.ChildrenCount >= MaxScores) {
            // If the current list of scores is greater or equal to our maximum allowed number,
            // we see if the new score should be added and remove the lowest existing score.
            long minScore = long.MaxValue;
            object minVal = null;
            foreach (var child in leaders) {
                if (!(child is Dictionary<string, object>))
                    continue;
                long childScore = (long)((Dictionary<string, object>)child)["score"];
                if (childScore < minScore) {
                    minScore = childScore;
                    minVal = child;
                }
            }
            // If the new score is lower than the current minimum, we abort.
            if (minScore > score) {
                return TransactionResult.Abort();
            }
            // Otherwise, we remove the current lowest to be replaced with the new score.
            leaders.Remove(minVal);
        }

        // Now we add the new score as a new entry that contains the email address and score.
        Dictionary<string, object> newScoreMap = new Dictionary<string, object>();
        newScoreMap["score"] = score;
        newScoreMap["email"] = name;
        leaders.Add(newScoreMap);

        // You must set the Value to indicate data at that location has changed.
        mutableData.Value = leaders;
        //return and log success
        return TransactionResult.Success(mutableData);
    }

    public void AddScore() {

        if (score == 0 || string.IsNullOrEmpty(name)) {
            DebugLog("invalid score or email.");
            return;
        }
        DebugLog(String.Format("Attempting to add score {0} {1}",
          name, score.ToString()));

        DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("Leaders");

        DebugLog("Running Transaction...");
        // Use a transaction to ensure that we do not encounter issues with
        // simultaneous updates that otherwise might create more than MaxScores top scores.
        reference.RunTransaction(AddScoreTransaction)
          .ContinueWith(task => {
              if (task.Exception != null) {
                  DebugLog(task.Exception.ToString());
              }
              else if (task.IsCompleted) {
                  DebugLog("Transaction complete.");
              }
          });
        //update UI
        addScorePressed = true;
    }
    private void createNewUserData() {
        DebugLog("Attempting to create new user data");
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference(KEY_DATABASE);

        DebugLog("Running Transaction...");
        // Use a transaction to ensure that we do not encounter issues with
        // simultaneous updates that otherwise might create more than MaxScores top scores.
        reference.RunTransaction(addNewUserDataTransaction)
          .ContinueWith(task => {
              if (task.Exception != null) {
                  DebugLog(task.Exception.ToString());
              }
              else if (task.IsCompleted) {
                  DebugLog("Transaction complete.");
              }
          });
    }
    #endregion

    #region Local Data Function
    private void saveUserData(int _loginStatus) {
        PlayerPrefs.SetString(KEY_LOCAL_DATA_EMAIL, email);
        PlayerPrefs.Save();
        PlayerPrefs.SetString(KEY_LOCAL_DATA_PASSWORD, password);
        PlayerPrefs.Save();
        PlayerPrefs.SetString(KEY_LOCAL_DATA_NAME, displayName);
        PlayerPrefs.Save();
        PlayerPrefs.SetInt(KEY_LOCAL_DATA_LOGIN_STATUS, _loginStatus);
        PlayerPrefs.Save();
    }
    private bool isUserLoggedIn() {
        if (PlayerPrefs.GetInt(KEY_LOCAL_DATA_LOGIN_STATUS) == ENUM_STATUS_LOGGED_IN)
            return true;
        else
            return false;
    }
    #endregion

    #region Button Funtion
    public void onPressSignUpEmailButton() {
        canvasSignUp.SetActive(true);
        canvasSignUpOption.SetActive(false);
        canvasHome.SetActive(false);
    }
    public void onPressSignUpGuestButton() {
        saveUserData(ENUM_STATUS_NOT_LOGGED_IN);
    }
    public void onPressSignUpButton() {
        if (checkSignUpField()) {
            createNewUserData();
            saveUserData(ENUM_STATUS_LOGGED_IN);
            CreateUserAsync();
            SigninAsync();
            initHomeCanvas();
        }
    }
    #endregion

    #region Canvas Function
    private void initCanvas() {
        canvasSplashScreen.SetActive(true);
        canvasHome.SetActive(false);
        canvasSignUp.SetActive(false);
        canvasSignUpOption.SetActive(false);
    }
    private void setAllCanvas(bool _active = false) {
        canvasSignUpOption.SetActive(_active);
        canvasSignUp.SetActive(_active);
        canvasHome.SetActive(_active);
        canvasSplashScreen.SetActive(false);
    }
    private void initSignUpOptionCanvas() {
        canvasSignUpOption.SetActive(true);
        canvasSignUp.SetActive(false);
        canvasHome.SetActive(false);
    }
    private void initHomeCanvas() {
        canvasHome.SetActive(true);
        canvasSignUpOption.SetActive(false);
        canvasSignUp.SetActive(false);
    }
    #endregion

    private IEnumerator initSplashScreen() {
        yield return new WaitForSeconds(3f);
        if (PlayerPrefs.GetInt("status") == ENUM_STATUS_LOGGED_IN) {
            initHomeCanvas();
        }
        else {
            initSignUpOptionCanvas();
        }
    }

    private bool checkSignUpField() {
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
        return result;
    }
}
