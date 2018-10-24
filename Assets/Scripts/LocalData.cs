using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalData : MonoBehaviour {
    public static string _name;
    public static string _password;
    public static string _email;
    public static int _status;

    void Start () {
        DontDestroyOnLoad(gameObject);
    }
}
