using UnityEngine;

//INSPIRATION/ SOURCE:
//https://www.youtube.com/watch?v=3xSYkFdQiZ0

public class OxygenMeter : MonoBehaviour
{
    private Transform needleTransform;
    
    private float maxSpeedAngle = -20;
    private float zeroSpeedAngle = 210;
    private float speedMax;
    private float speed;
    
    void Awake()
    {
        needleTransform = transform.Find("needle");

        speed = 0f;
        speedMax = 200f;
    }

    // Update is called once per frame
    void Update()
    {
        speed += 30f * Time.deltaTime;
        if (speed > speedMax)
        {
            speed = speedMax;
        }

        needleTransform.localEulerAngles = new Vector3(0, 0, GetSpeedRotation());
    }


    private float GetSpeedRotation()
    {
        float totalAngle = zeroSpeedAngle -  maxSpeedAngle;
        
        float speedNormalized = speed/speedMax;
        
        return zeroSpeedAngle - speedNormalized * totalAngle;
    }
}
