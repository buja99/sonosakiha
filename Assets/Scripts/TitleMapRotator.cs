using UnityEngine;

public class TitleMapRotator : MonoBehaviour
{
    public Vector3 rotationAxis = new Vector3(0.3f, 1f, 0.2f);
    public float rotationSpeed = 8f;

    void Update()
    {
        transform.Rotate(rotationAxis.normalized * rotationSpeed * Time.deltaTime, Space.World);
    }
}