using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface VelocitySolver
{
    Vector3 UpdateSolver(Mech mech, Vector3 input, bool boost, out float smoothTime);
}
