using UnityEngine;

public class LightManager : MonoBehaviour
{
    public Light pointLight;
    
    void Start()
    {
        pointLight = this.GetComponent<Light>();
    }

    public void UpdateLight()
    {
        float h = Random.Range(0f, 1f);
        
        Color color = Color.HSVToRGB(h, 1, 1);
        Debug.Log(color);
        pointLight.color = color;
    }
}
