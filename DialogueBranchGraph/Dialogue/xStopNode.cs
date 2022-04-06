using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateNodeMenu("Dialogue/Stop")]
public class xStopNode : xBaseNode {

	[Input(ShowBackingValue.Never)] public int Entry;
    public override string GetString()
    {
        return "Stop" ;
    }
}