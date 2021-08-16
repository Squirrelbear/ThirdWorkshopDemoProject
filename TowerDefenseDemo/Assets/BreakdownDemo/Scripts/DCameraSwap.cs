using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DCameraSwap : MonoBehaviour
{
    public List<GameObject> cameras;
    public int currentCameraIndex;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            setActiveCameraID(currentCameraIndex + 1);
        }
    }

    private void setActiveCameraID(int id)
    {
        currentCameraIndex = Mathf.Abs(id) % cameras.Count;
        for (int i = 0; i < cameras.Count; i++)
        {
            cameras[i].SetActive(i == currentCameraIndex);
        }
    }
}
