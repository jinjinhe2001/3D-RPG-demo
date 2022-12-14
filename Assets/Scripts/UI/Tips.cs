using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tips : MonoBehaviour
{
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)||Input.GetMouseButtonDown(0))
        {
            gameObject.SetActive(false);
        }
    }
}
