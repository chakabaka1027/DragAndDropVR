using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragAndDrop : MonoBehaviour {

    SteamVR_TrackedObject trackedObject;
    SteamVR_Controller.Device device;
    GameObject holder;

    public List<GameObject> objects;
    GameObject middleObj;
    GameObject leftObj;
    GameObject rightObj;

    GameObject heldObject;

    bool objSpawned = false;

    //menu info
    int menuToggle = 0;
    enum EditMode{Moving, Spawning};
    EditMode editMode = EditMode.Spawning;

    int currentlySelectedObjIndex = 0;

    public LayerMask environment;
    public LayerMask spawnableObjects;



    void OnEnable() {
        SteamVR_TrackedController controller = GetComponent<SteamVR_TrackedController>();
        controller.PadClicked += OnPadClicked;
        controller.TriggerClicked += OnTriggerClicked;
    }

    void OnDisable() {
        SteamVR_TrackedController controller = GetComponent<SteamVR_TrackedController>();
        controller.PadClicked -= OnPadClicked;
        controller.TriggerClicked -= OnTriggerClicked;
    }

    void OnPadClicked(object sender, ClickedEventArgs e) {
        StopCoroutine("MenuToggle");
        StartCoroutine("MenuToggle");
    }

    void OnTriggerClicked(object sender, ClickedEventArgs e) {
        IdentifyTarget();
    }


    void Start(){
        trackedObject = GetComponent<SteamVR_TrackedObject>();
        holder = new GameObject();
        holder.transform.parent = this.transform;
        holder.transform.localPosition = Vector3.zero;
		holder.transform.localRotation = Quaternion.identity;
        holder.name = "Object Menu";

        middleObj = Instantiate(objects[0]) as GameObject;
        middleObj.transform.parent = holder.transform;
        middleObj.transform.localPosition = new Vector3(0f, .06f, 0f);

        rightObj = Instantiate(objects[1]) as GameObject;
        rightObj.transform.parent = holder.transform;
        rightObj.transform.localPosition = middleObj.transform.localPosition + new Vector3(.1f, 0, 0f);

        leftObj = Instantiate(objects[2]) as GameObject;
        leftObj.transform.parent = holder.transform;
        leftObj.transform.localPosition = middleObj.transform.localPosition + new Vector3(-.1f, 0, 0f);
    }
    void Update(){
        device = SteamVR_Controller.Input((int)trackedObject.index);
        
        if (editMode == EditMode.Spawning){
            //scroll thru menu when touching touchpad
            if(GetComponent<SteamVR_TrackedController>().padTouched){
                StartCoroutine("ScrollSelectItem");
            } else {
                StopCoroutine("ScrollSelectItem");
            }

            //snap middle object to center when not touching touchpad
            if(!GetComponent<SteamVR_TrackedController>().padTouched){
                CenterMiddleObj();
            }

            //spawn obj when pressing trigger
            if(GetComponent<SteamVR_TrackedController>().triggerPressed){
                if(!objSpawned){
                    SpawnObj();
                    objSpawned = true;
                }
                ObjFollowCursor();
            
            }
            if(!GetComponent<SteamVR_TrackedController>().triggerPressed){
                objSpawned = false;
                if(heldObject != null){
                    heldObject.GetComponent<Rigidbody>().isKinematic = false;
                }
                heldObject = null;
            }
        } 
        
        else {
            //move obj when pressing trigger
            if(GetComponent<SteamVR_TrackedController>().triggerPressed){
                ObjFollowCursor();
            }
            if(!GetComponent<SteamVR_TrackedController>().triggerPressed){
                if(heldObject != null){
                    heldObject.GetComponent<Rigidbody>().isKinematic = false;
                }
                heldObject = null;
            }
        }
    }

    IEnumerator ScrollSelectItem(){
        //calculate swipe magnitude
        float initialTouch = device.GetAxis().x;
        yield return new WaitForSeconds(0.01f);
        float finalTouch = device.GetAxis().x;
        float touchDistance = finalTouch - initialTouch;

        //move object UI
        if(rightObj != null || middleObj != null || leftObj != null){
            middleObj.transform.localPosition += new Vector3(touchDistance * .07f, 0, 0);
            leftObj.transform.localPosition += new Vector3(touchDistance * .07f, 0, 0);
            rightObj.transform.localPosition += new Vector3(touchDistance * .07f, 0, 0);
        }

        //scroll to the right
        if(middleObj.transform.localPosition.x >= .05f){
            currentlySelectedObjIndex--;
            if(currentlySelectedObjIndex < 0){
                currentlySelectedObjIndex = objects.Count - 1;
            }
            //Debug.Log(currentlySelectedObjIndex);
            
            Destroy(rightObj);
            rightObj = middleObj;

            middleObj = leftObj;

            //if the currently selected obj is of index 0, the left one cannot equal -1 so it must be set to equal the value at the top of the list
            if(currentlySelectedObjIndex == 0){
                leftObj = Instantiate(objects[objects.Count - 1]) as GameObject;
            } else if (currentlySelectedObjIndex > 0){
               leftObj = Instantiate(objects[currentlySelectedObjIndex-1]) as GameObject;
            }
            leftObj.transform.parent = holder.transform;
            leftObj.transform.localPosition = middleObj.transform.localPosition + new Vector3(-.1f, 0, 0f);
            leftObj.transform.localEulerAngles = Vector3.zero;
        }

        //scroll to the left
        if(middleObj.transform.localPosition.x <= -.05f){
            currentlySelectedObjIndex++;
            if(currentlySelectedObjIndex > objects.Count - 1){
                currentlySelectedObjIndex = 0;
            }
            Debug.Log(currentlySelectedObjIndex);
            
            Destroy(leftObj);
            leftObj = middleObj;

            middleObj = rightObj;

            //if the currently selected obj is of index 0, the left one cannot equal -1 so it must be set to equal the value at the top of the list
            if(currentlySelectedObjIndex == objects.Count - 1){
                rightObj = Instantiate(objects[0]) as GameObject;
            } else if (currentlySelectedObjIndex < objects.Count - 1){
               rightObj = Instantiate(objects[currentlySelectedObjIndex+1]) as GameObject;
            }
            rightObj.transform.parent = holder.transform;
            rightObj.transform.localPosition = middleObj.transform.localPosition + new Vector3(.1f, 0, 0f);
            rightObj.transform.localEulerAngles = Vector3.zero;
        }
    }

    void CenterMiddleObj(){
        if(middleObj.transform.localPosition.x < 0.04f || middleObj.transform.localPosition.x > -0.04f){
            float step = .25f * Time.deltaTime;
            middleObj.transform.localPosition = Vector3.MoveTowards(middleObj.transform.localPosition, new Vector3 (0f, 0.06f, 0), step);
            rightObj.transform.localPosition = Vector3.MoveTowards(rightObj.transform.localPosition, new Vector3 (.1f, 0.06f, 0), step);
            leftObj.transform.localPosition = Vector3.MoveTowards(leftObj.transform.localPosition, new Vector3 (-.1f, 0.06f, 0), step);
        }
    }

    void SpawnObj(){
        Ray raycast = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if(Physics.Raycast(raycast, out hit, Mathf.Infinity, environment)){
            heldObject = Instantiate(middleObj, hit.point + hit.normal * .25f, Quaternion.identity) as GameObject;
            heldObject.transform.localScale = new Vector3(.5f, .5f, .5f);
            heldObject.GetComponent<Rigidbody>().isKinematic = true;
        } else {
            heldObject = Instantiate(middleObj, raycast.GetPoint(5), Quaternion.identity) as GameObject;
            heldObject.transform.localScale = new Vector3(.5f, .5f, .5f);
            heldObject.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    void ObjFollowCursor(){
        Ray ray = new Ray(transform.position, transform.forward);
		RaycastHit hit;

		if(Physics.SphereCast(ray, .25f, out hit, Mathf.Infinity, environment) && heldObject != null){
            heldObject.transform.position = hit.point + hit.normal * .25f;
            heldObject.GetComponent<Rigidbody>().isKinematic = true;
        } if(!Physics.SphereCast(ray, .25f, out hit, Mathf.Infinity, environment) && heldObject != null){
            heldObject.transform.position = ray.GetPoint(5);
            heldObject.GetComponent<Rigidbody>().isKinematic = true;

        } 
    }

    void IdentifyTarget(){
        Ray raycast = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if(Physics.Raycast(raycast, out hit, Mathf.Infinity, spawnableObjects)){
            heldObject = hit.collider.gameObject;
        } 
    }

    IEnumerator MenuToggle(){          
        menuToggle = 1 - menuToggle;
        float percent = 0;
        float time = .225f;
        float speed = 1 / time;

        //close
        if(menuToggle == 1){
            editMode = EditMode.Moving;
            while(percent < 1){
                percent += Time.deltaTime * speed;
                middleObj.transform.localScale = Vector3.Lerp(middleObj.transform.localScale, Vector3.zero, percent);
                rightObj.transform.localScale = Vector3.Lerp(rightObj.transform.localScale, Vector3.zero, percent);
                leftObj.transform.localScale = Vector3.Lerp(leftObj.transform.localScale, Vector3.zero, percent);
                yield return null;
            }

        //open
        } else {
            editMode = EditMode.Spawning;
            while(percent < 1){
                percent += Time.deltaTime * speed;
                middleObj.transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(0.05f, 0.05f, 0.05f), percent);
                rightObj.transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(0.05f, 0.05f, 0.05f), percent);
                leftObj.transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(0.05f, 0.05f, 0.05f), percent);
                yield return null;
            }
        }
    }

}
