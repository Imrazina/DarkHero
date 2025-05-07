using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorldType { PeopleWorld, SpiritWorld, AlwaysActive }

public class WorldDependentObject : MonoBehaviour
{
    public WorldType worldType;
    
    private void Start()
    {
        UpdateVisibility(GameStateManager.Instance.CurrentState.isInSpiritWorld);
    }

    public void UpdateVisibility(bool isSpiritWorld)
    {
        bool shouldBeVisible = (worldType == WorldType.AlwaysActive) ||
                               (isSpiritWorld && worldType == WorldType.SpiritWorld) ||
                               (!isSpiritWorld && worldType == WorldType.PeopleWorld);

        gameObject.SetActive(shouldBeVisible);
    }
}
