using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public TargetType type;
}

public enum TargetType {
    THERMAL,
    VITAL,
}