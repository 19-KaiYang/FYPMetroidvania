using UnityEngine;

public class SpiritSlashTrail : MonoBehaviour
{

    public Material trailMaterial;

    private TrailRenderer trail;

    void Awake()
    {
        trail = GetComponent<TrailRenderer>();

        // Configure trail settings
        trail.time = 0.2f; 
        trail.startWidth = 0.3f;
        trail.endWidth = 0f;
        trail.material = trailMaterial; 

        // Set colors
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.clear, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        trail.colorGradient = gradient;
    }
}