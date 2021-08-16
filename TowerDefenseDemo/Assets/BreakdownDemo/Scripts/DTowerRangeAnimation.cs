using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DTowerRangeAnimation : MonoBehaviour
{
    public DTowerBehaviour towerBehaviour;
    private Material mat;
    private bool increasing = false;

    // Use this for initialization
    void Start()
    {
        mat = GetComponentInChildren<MeshRenderer>().material;
        Color newColor2 = mat.color;
        newColor2.a = 0.4f;
        mat.color = newColor2;

        towerBehaviour.OnSelectedTower += showRange;
        towerBehaviour.OnDeselectedTower += hideRange;

        showRange();
    }

    public void showRange()
    {
        transform.localScale = new Vector3(towerBehaviour.maxRange, 0.33f, towerBehaviour.maxRange);
        GetComponent<MeshRenderer>().enabled = true;
    }

    public void hideRange()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        Color newColor2 = mat.color;
        if (increasing)
        {
            newColor2.a += Time.deltaTime / 2;
        }
        else
        {
            newColor2.a -= Time.deltaTime / 2;
        }

        if (newColor2.a < 0.2)
        {
            newColor2.a = 0.2f;
            increasing = true;
        }
        else if (newColor2.a > 0.4)
        {
            newColor2.a = 0.4f;
            increasing = false;
        }
        mat.color = newColor2;
    }
}
