using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(Test(3));
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    int Test(int a)
    {
        a += 5;
        return a;
    }


}
