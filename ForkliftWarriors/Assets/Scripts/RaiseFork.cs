using UnityEngine;

public class RaiseFork : MonoBehaviour
{
    [SerializeField] VRLever raiseLever;
    [SerializeField] int forkRaiseSpeed = 2;
    [SerializeField] private int minY, maxY;
    GameObject forkliftFork;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
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

        transform.position += movement * forkRaiseSpeed * Time.deltaTime;

        float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);
        transform.position = new Vector3(transform.position.x, clampedY, transform.position.z);
    }
}
