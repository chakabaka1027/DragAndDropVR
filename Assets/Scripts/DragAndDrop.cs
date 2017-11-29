using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragAndDrop : MonoBehaviour {

    private SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device device;
    GameObject holder;

    public List<GameObject> objects;
    GameObject middleObj;
    GameObject leftObj;
    GameObject rightObj;

    int currentlySelectedObjIndex = 0;

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
        if(GetComponent<SteamVR_TrackedController>().padTouched){
            StartCoroutine("ScrollSelectItem");
        } else {
            StopCoroutine("ScrollSelectItem");
        }
        if(!GetComponent<SteamVR_TrackedController>().padTouched){
            CenterMiddleObj();
        }
    }

    IEnumerator ScrollSelectItem(){
        //calculate swipe magnitude
        float initialTouch = device.GetAxis().x;
        yield return new WaitForSeconds(0.1f);
        float finalTouch = device.GetAxis().x;
        float touchDistance = finalTouch - initialTouch;

        //move object UI
        if(rightObj != null || middleObj != null || leftObj != null){
            middleObj.transform.localPosition += new Vector3(touchDistance * .01f, 0, 0);
            leftObj.transform.localPosition += new Vector3(touchDistance * .01f, 0, 0);
            rightObj.transform.localPosition += new Vector3(touchDistance * .01f, 0, 0);
        }

        //scroll to the right
        if(middleObj.transform.localPosition.x >= .05f){
            currentlySelectedObjIndex--;
            if(currentlySelectedObjIndex < 0){
                currentlySelectedObjIndex = objects.Count - 1;
            }
            Debug.Log(currentlySelectedObjIndex);
            
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

}
