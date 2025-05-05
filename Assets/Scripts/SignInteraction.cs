using UnityEngine;

public class SignInteraction : MonoBehaviour
{
    public GameObject infoPanel; 

    private void Start()
    {
        infoPanel.SetActive(false); 
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            infoPanel.SetActive(true); 
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            infoPanel.SetActive(false); 
        }
    }
}