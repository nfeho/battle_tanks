using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractState
{
    public abstract void OnStateEnter(AIController aI);

    public abstract void Update(AIController aI);

}
