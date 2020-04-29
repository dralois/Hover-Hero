using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePositionClick : MonoBehaviour
{
    public float timeStay;
    public float maxDistance;
    public float sceeneChangeCooldown;
    private Vector3[] lastPositions;

    // Start is called before the first frame update
    void Start()
    {
        MouseControl.MouseMove(new Vector3(0.5f, 0.5f, 0),null);
        //MouseControl.MouseClick();
        lastPositions = new Vector3[(int)(timeStay * 30)];
        randomize();
    }

    void randomize()
    {
        for (int i = 0; i < lastPositions.Length; i++)
        {
            lastPositions[i] = new Vector3(Random.Range(0, 1000), Random.Range(0, 1000));
        }
    }

    int counter = 0;
    // Update is called once per frame
    void FixedUpdate()
    {
        if (sceeneChangeCooldown > 0)
        {
            sceeneChangeCooldown-=Time.fixedDeltaTime;
            return;
        }
        lastPositions[counter] = Input.mousePosition;
        counter = (counter + 1) % lastPositions.Length;

        Vector3 average = Vector3.zero;

        foreach (Vector3 item in lastPositions)
        {
            average += item;
        }
        average /= lastPositions.Length;
        bool allTogether = true;
        foreach (Vector3 item in lastPositions)
        {
            if((item - average).magnitude > maxDistance)
            {
                allTogether = false;
                break;
            }
        }

        if (allTogether)
        {
            MouseControl.MouseClick();
            randomize();
        }
    }
}
