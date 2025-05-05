using UnityEngine;

public class BossRoomTrigger : MonoBehaviour
{
    public TupikController tupik;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            tupik.CloseEntrance();
        }
    }
}