using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    private Character character;

    void Start()
    {
        character = GetComponentInParent<Character>(); 
    }

    public void StartDash()
    {
        if (character != null)
        {
            character.StartDashFromAnimation();
        }
    }

    public void EndDash()
    {
        if (character != null)
        {
            character.EndDash();
        }
    }
}