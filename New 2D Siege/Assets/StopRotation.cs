using UnityEngine;

public class StopRotation : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.identity;
    }
}
