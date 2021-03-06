﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragAndDrop : MonoBehaviour {

    SteamVR_TrackedObject trackedObject;
    SteamVR_TrackedObject otherTrackedObject;
    SteamVR_Controller.Device device;
    SteamVR_Controller.Device otherDevice;
    public GameObject leftController;

    GameObject holder;

    public GameObject compass;
    GameObject currentCompass;

    public List<GameObject> objects;
    GameObject middleObj;
    GameObject leftObj;
    GameObject rightObj;

    GameObject heldObject;
    float heldObjectThickness;

    bool objSpawned = false;
    bool isMoving = false;

    //velocity info
    Vector3 oldPos;
    Vector3 newPos;
    Vector3 heldObjVelocity;

    //torque info
    Vector3 oldRot;
    Vector3 newRot;
    Vector3 heldObjRotation;

    //menu info
    int menuToggle = 0;
    enum EditMode{Moving, Spawning};
    EditMode editMode = EditMode.Spawning;

    int currentlySelectedObjIndex = 0;

    public LayerMask environment;
    public LayerMask spawnableObjects;

    float maxObjDistance;
    float objDistance;
    
    float objScale;
    float controllerDistance;

    void OnEnable() {
        SteamVR_TrackedController rController = GetComponent<SteamVR_TrackedController>();
        SteamVR_TrackedController lController = leftController.GetComponent<SteamVR_TrackedController>();
        rController.PadClicked += OnPadClicked;
        rController.TriggerClicked += OnTriggerClicked;

        lController.PadTouched += OnPadTouched;
        lController.PadUntouched += OnPadUntouched;
    }

    void OnDisable() {
        SteamVR_TrackedController rController = GetComponent<SteamVR_TrackedController>();
        SteamVR_TrackedController lController = leftController.GetComponent<SteamVR_TrackedController>();
        rController.PadClicked -= OnPadClicked;
        rController.TriggerClicked -= OnTriggerClicked;

        lController.PadTouched -= OnPadTouched;
        lController.PadUntouched -= OnPadUntouched;

    }

    void OnPadClicked(object sender, ClickedEventArgs e) {
        StopCoroutine("MenuToggle");
        StartCoroutine("MenuToggle");
    }

    void OnPadTouched(object sender, ClickedEventArgs e) {
        if(heldObject != null){
            CreateRotationCompass();
        }
    }

    void OnPadUntouched(object sender, ClickedEventArgs e) {
        if(heldObject != null){
            DestroyRotationCompass();
        }
    }

    void OnTriggerClicked(object sender, ClickedEventArgs e) {
        IdentifyTarget();
    }


    void Start(){
        trackedObject = GetComponent<SteamVR_TrackedObject>();
        otherTrackedObject = leftController.GetComponent<SteamVR_TrackedObject>();

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
        otherDevice = SteamVR_Controller.Input((int)otherTrackedObject.index);

        //pressing the left controller touch pad
        if(GetComponent<SteamVR_TrackedController>().menuPressed){
            Destroy();
        }

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

            if(GetComponent<SteamVR_TrackedController>().padPressed){
                ModifyObjDistanceLarge();
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
                    DestroyRotationCompass();
                    heldObject.GetComponent<Rigidbody>().isKinematic = false;
                }
                heldObject = null;
            }
        } 
        
        else {
            //move obj when pressing trigger
            if(GetComponent<SteamVR_TrackedController>().triggerPressed){
                ObjFollowCursor();
                GetVelocity();
                GetTorque();

            }
            if(!GetComponent<SteamVR_TrackedController>().triggerPressed){
                if(heldObject != null){
                    heldObject.transform.parent = null;
                    DestroyRotationCompass();
                    heldObject.GetComponent<Rigidbody>().isKinematic = false;

                    heldObject.GetComponent<Rigidbody>().AddForce(heldObjVelocity * 5, ForceMode.Impulse);
                    heldObject.GetComponent<Rigidbody>().AddRelativeTorque(heldObjRotation, ForceMode.Acceleration);
                }
                heldObject = null;
                isMoving = false;
            }

            if(leftController.GetComponent<SteamVR_TrackedController>().triggerPressed){
                StartCoroutine("Scale");
                heldObject.transform.localScale = new Vector3(heldObject.transform.localScale.x + objScale, heldObject.transform.localScale.y + objScale, heldObject.transform.localScale.z + objScale);
            } else {
                StopCoroutine("Scale");
                objScale = 0;
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

        StopCoroutine("MenuToggle");
        StartCoroutine("MenuToggle");

        if(Physics.Raycast(raycast, out hit, Mathf.Infinity, environment)){
            //limit the object's spawning distance to maximum so it doesn't spawn too far away from the player
            if(hit.distance > 15){
                objDistance = 15;
                heldObject = Instantiate(middleObj, raycast.GetPoint(20), Quaternion.identity) as GameObject;
            } else if(hit.distance <= 20){
                objDistance = hit.distance;
                heldObject = Instantiate(middleObj, hit.point + hit.normal * .25f, Quaternion.identity) as GameObject;
            }
            heldObject.transform.localScale = new Vector3(.5f, .5f, .5f);
            heldObject.GetComponent<Rigidbody>().isKinematic = true;
        } else {
            objDistance = 5;
            heldObject = Instantiate(middleObj, raycast.GetPoint(5), Quaternion.identity) as GameObject;
            heldObject.transform.localScale = new Vector3(.5f, .5f, .5f);
            heldObject.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    void ObjFollowCursor(){
        Ray ray = new Ray(transform.position, transform.forward);
		RaycastHit hit;

        if(heldObject != null){
		    if(Physics.SphereCast(ray, heldObject.GetComponent<MeshRenderer>().bounds.extents.x, out hit, Mathf.Infinity, environment)){
                isMoving = true;
                maxObjDistance = hit.distance;

                //limit object distance so it doesn't collide through walls
                if(objDistance >= maxObjDistance){
                    float offset = Vector3.Distance(hit.point, heldObject.transform.position);
                    heldObjectThickness = offset;
                    heldObject.transform.position = hit.point + hit.normal * heldObject.GetComponent<MeshRenderer>().bounds.extents.x;
                } else {
                    heldObject.transform.position = ray.GetPoint(objDistance);
                }
           
                heldObject.GetComponent<Rigidbody>().isKinematic = true;
            } if(!Physics.SphereCast(ray, .25f, out hit, Mathf.Infinity, environment) && heldObject != null){
                isMoving = true;

                maxObjDistance = hit.distance;

                heldObject.transform.position = ray.GetPoint(objDistance);
                heldObject.GetComponent<Rigidbody>().isKinematic = true;

            } 

            if(GetComponent<SteamVR_TrackedController>().padTouched){
                StartCoroutine("ModifyObjDistance");
            } else {
                StopCoroutine("ModifyObjDistance");
            }

            //held obj has same rotation as controller when being moved
            if (heldObject != null) {
                heldObject.transform.parent = transform;
            }

            if (leftController.GetComponent<SteamVR_TrackedController>().padTouched){
                StartCoroutine("Rotate");
            } else {
                StopCoroutine("Rotate");
            }
        }
    }

    void IdentifyTarget(){
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, Mathf.Infinity, spawnableObjects)){
            heldObject = hit.collider.gameObject;
            float offset = Vector3.Distance(hit.point, heldObject.transform.position);
            objDistance = hit.distance + offset;
        } 
    }

    IEnumerator MenuToggle(){          
        menuToggle = 1 - menuToggle;
        float percent = 0;
        float time = .225f;
        float speed = 1 / time;

        if(!isMoving){
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

    IEnumerator ModifyObjDistance(){
        //calculate swipe magnitude
        float initialTouch = device.GetAxis().y;
        yield return new WaitForSeconds(0.0001f);
        float finalTouch = device.GetAxis().y;
        float touchDistance = finalTouch - initialTouch;

        //alter objDistance based on touch magnitude
        objDistance += (touchDistance * 3f);
        if (objDistance <= .5f){
            objDistance = .5f;
        }
    }

    void ModifyObjDistanceLarge(){
        if(device.GetAxis().y > 0){
            objDistance += Time.deltaTime * 2f;
            Debug.Log("hi");
        } else {
            objDistance += Time.deltaTime * -2f;
            if(objDistance <= .5f){
                objDistance = .5f;
            }        
        }
    }

    IEnumerator Scale(){
        //obtain scaling factor
        Vector3 initialPos = leftController.transform.position;
        yield return new WaitForSeconds(0.0001f);
        Vector3 finalPos = leftController.transform.position;
        float posDistance = Vector3.Distance(initialPos, finalPos);
        
        //account for sensor tracking noise
        if(posDistance < .001f){
            posDistance = 0;
        }

        float tempControllerDistance = Vector3.Distance(leftController.transform.position, transform.position);

        if(tempControllerDistance < controllerDistance){
            objScale = -posDistance * 2;
        } else if(tempControllerDistance > controllerDistance){
            objScale = posDistance * 2;
        }

        controllerDistance = tempControllerDistance;

    }



    IEnumerator Rotate(){

        //calculate swipe magnitude
        float initialTouch = otherDevice.GetAxis().x;
        yield return new WaitForSeconds(0.001f);
        float finalTouch = otherDevice.GetAxis().x;
        float touchVector = initialTouch - finalTouch;

        //initialTouch = otherDevice.GetAxis();
        //Vector2 diff = initialTouch - finalTouch;
        //Vector2 diffOverTime = diff/ Time.deltaTime;
        //finalTouch = initialTouch;

        //heldObject.transform.Rotate(new Vector3(0, -diffOverTime.x, 0), Space.Self);

        //float sensitivity = .5f;

        ////Get Main camera in Use.
        //Camera cam = Camera.main;
        ////Gets the world vector space for cameras up vector 
        //Vector3 relativeUp = cam.transform.TransformDirection(Vector3.up);
        ////Gets world vector for space cameras right vector
        //Vector3 relativeRight = cam.transform.TransformDirection(Vector3.right);

        ////Turns relativeUp vector from world to objects local space
        //Vector3 objectRelativeUp = heldObject.transform.InverseTransformDirection(relativeUp);
        ////Turns relativeRight vector from world to object local space
        //Vector3 objectRelativeRight = heldObject.transform.InverseTransformDirection(relativeRight);

        ////heldObject.transform.RotateAround(heldObject.transform.position, Camera.main.transform.InverseTransformDirection(Vector3.up), touchVector.x * 100);
        ////heldObject.transform.RotateAround(heldObject.transform.position, Camera.main.transform.InverseTransformDirection(Vector3.right), -touchVector.y * 100);
        if (leftController.GetComponent<SteamVR_TrackedController>().padTouched){
            heldObject.transform.Rotate(new Vector3(0, touchVector * 50, 0), Space.Self);
        }
    }


     void CreateRotationCompass(){
        currentCompass = Instantiate(compass, heldObject.transform.position, heldObject.transform.rotation * Quaternion.Euler(90, 0, 0), heldObject.transform) as GameObject;
     }

     void DestroyRotationCompass(){
        if(currentCompass != null){
            Destroy(currentCompass);
        }
     }

     void Destroy(){
        Ray raycast = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if(Physics.Raycast(raycast, out hit, Mathf.Infinity, spawnableObjects)){
            Destroy(hit.collider.gameObject);
            if(heldObject != null){
                heldObject = null;
            }
        } 
     }

     void GetVelocity(){
        if(heldObject != null){
             newPos = transform.position;
             Vector3 speed =  (newPos - oldPos);
             heldObjVelocity = speed /Time.deltaTime;
             oldPos = newPos;
        }
     }

     void GetTorque(){
        if(heldObject != null){
             newRot = heldObject.transform.eulerAngles;
             Vector3 rot =  (newRot - oldRot);
             heldObjRotation = rot /Time.deltaTime;
             oldRot = newRot;
        }
     }
}
