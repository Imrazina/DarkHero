using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorldType { PeopleWorld, SpiritWorld, AlwaysActive }

public class WorldDependentObject : MonoBehaviour
{
    public WorldType worldType;
}
