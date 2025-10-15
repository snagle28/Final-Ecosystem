using UnityEngine;

public class bubbleManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
    
    public void moveBubble(Transform clamTransform)
    {
        transform.position = new Vector3(clamTransform.position.x,transform.position.y);
    }
}
