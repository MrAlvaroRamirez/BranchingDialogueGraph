using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoLayoutColumn : MonoBehaviour
{
    // Simple script to change column count based on child
    public void UpdateColumns(int choiceCount)
    {
        var layout = GetComponent<Beardy.GridLayoutGroup>();
        if(choiceCount <= 4)
            layout.constraintCount = 2;
        else
            layout.constraintCount = 3;            
    }
}
