using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragAndDrop : MonoBehaviour {
    
	//// Use this for initialization
	//void OnEnable () {
	//	SteamVR_TrackedController controller = GetComponent<SteamVR_TrackedController>();
	//	controller.TriggerClicked += OnClickTrigger;
	//	controller.TriggerUnclicked += OnUnclickTrigger;
	//	controller.PadClicked += OnPadClicked;
 //       controller.PadTouched += OnPadTouched;
	//}
	
	//void OnDisable(){
	//	SteamVR_TrackedController controller = GetComponent<SteamVR_TrackedController>();
	//	controller.TriggerClicked -= OnClickTrigger;
	//	controller.TriggerUnclicked -= OnUnclickTrigger;
	//	controller.PadClicked -= OnPadClicked;
	//}

	//void OnPadClicked(object sender, ClickedEventArgs e){
	//	Debug.Log ("Pad Clicked! X: " + e.padX + " " + e.padY);
	//}

	//void OnPadTouched(object sender, ClickedEventArgs e){
	//	Debug.Log ("Pad Touched! X: " + e.padX + " " + e.padY);
	//}

	//void OnUnclickTrigger(object sender, ClickedEventArgs e) {
	//	Debug.Log("Unclicked trigger!");
	//}

	//void OnClickTrigger(object sender, ClickedEventArgs e) {
	//	Debug.Log("Clicked trigger!");
	//}

    private SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device device;
    void Start(){
         trackedObject = GetComponent<SteamVR_TrackedObject>();
    }
    void Update(){
         device = SteamVR_Controller.Input((int)trackedObject.index);
         if(device.GetAxis().x != 0 || device.GetAxis().y != 0){
           	    Debug.Log(device.GetAxis().x + " " + device.GetAxis().y);
         }
    }

}
