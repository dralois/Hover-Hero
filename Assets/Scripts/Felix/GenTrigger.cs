using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenTrigger : MonoBehaviour {

    public Element myElement;

    public void Start()
    {
        if (myElement == null)
        {
            myElement = GetComponentInParent<Element>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (this.tag == "Enter")
        {
            FindObjectOfType<GenManager>().Entering(myElement);
            Debug.Log("trigger enter @" + Time.timeSinceLevelLoad);
        }
        else if(this.tag == "Leave")
        {
            FindObjectOfType<GenManager>().Leaving();
            Debug.Log("trigger leave @"+Time.timeSinceLevelLoad);
        }
        else
        {
            //Debug.Log("trigger but no action");
        }
    }

}
