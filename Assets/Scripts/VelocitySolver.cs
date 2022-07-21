using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface VelocitySolver
{
    Vector3 UpdateSolver(Vector3 input, bool boost);
}
