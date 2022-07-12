using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Item
{
    string name { get; }

    Mech owner { get; set; }
}
