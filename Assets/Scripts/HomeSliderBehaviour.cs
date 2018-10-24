using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HomeSliderBehaviour : MonoBehaviour,IPointerClickHandler {
    [SerializeField] string url;
    [SerializeField] Text txtDebug;
    [SerializeField] TouchDetector touchDetector;

    public void OnPointerClick(PointerEventData eventData)
    {
        if(!touchDetector.SwipeRight && !touchDetector.SwipeLeft){
            Debug.Log("Open url \n" + url);
            Application.OpenURL(url);
        }
    }

	private void OnMouseDown() {
        //open url

    }
}
