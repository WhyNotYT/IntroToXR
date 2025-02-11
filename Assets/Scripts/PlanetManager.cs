using UnityEngine;

public class PlanetManager : MonoBehaviour
{
    public float rotationSpeed = 5;
    void Update()
    {
        this.transform.Rotate(Vector3.up, rotationSpeed* Time.deltaTime);
    }
}

