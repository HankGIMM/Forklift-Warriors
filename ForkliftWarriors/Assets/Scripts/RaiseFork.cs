using UnityEngine;

public class RaiseFork : MonoBehaviour
{
    [SerializeField] VRLever raiseLever,leftRightLever,yawLever;
    [SerializeField] int forkRaiseSpeed = 2;
    [SerializeField] private int minY, maxY, minX, maxX;
    GameObject forkliftFork;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
        
        RaisingFork();
        LeftRightFork();
        YawFork();

        
    }
    void RaisingFork()
    {
        Vector3 movement = Vector3.zero;

        if (raiseLever.leverOutput <= 140)
        {
            movement = Vector3.up;
        }
        else if (raiseLever.leverOutput >= 160)
        {
            movement = Vector3.down;
        }
        if(transform.position.z >= 3f || transform.position.z <= -3f )
        {
            movement = Vector3.zero;
        }
        transform.position += movement * forkRaiseSpeed * Time.deltaTime;

        float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);
        transform.position = new Vector3(transform.position.x, clampedY, transform.position.z);
    }
    void LeftRightFork()
    {
        float movement = 0f;

        if (leftRightLever.leverOutput < 140)
        {
            movement = -1f; // left
        }
        else if (leftRightLever.leverOutput > 160)
        {
            movement = 1f; // right
        }
        if(transform.position.z >= 3f || transform.position.z <= -3f )
        {
            movement = 0f;
        }
        transform.position += new Vector3(0f, 0f, movement) * forkRaiseSpeed * Time.deltaTime;

        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
        
    }
    void YawFork()
{
    float rotation = 0f;

    if (yawLever.leverOutput <= 140)
    {
        rotation = -1f; // rotate left
    }
    else if (yawLever.leverOutput >= 160)
    {
        rotation = 1f; // rotate right
    }
    if(transform.rotation.z >= 3f || transform.rotation.z <= -3f )
        {
            rotation = 0f;
        }

    transform.Rotate(rotation * forkRaiseSpeed * Time.deltaTime ,0f, 0f);
}
    
}
