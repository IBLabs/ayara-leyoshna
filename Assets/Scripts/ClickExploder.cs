using UnityEngine;
using UnityEngine.InputSystem;

using Unity.Cinemachine;
using System.Collections;

public class ClickExploder : MonoBehaviour
{
    public InputActionProperty clickAction;
    public GameObject hitParticlePrefab;
    public float shakeDuration = 0.5f;

    public CinemachineCamera VirtualCamera;
    private CinemachineBasicMultiChannelPerlin virtualCameraNoise;

    // Use this for initialization
    void Start()
    {
        // Get Virtual Camera Noise Profile
        if (VirtualCamera != null)
            virtualCameraNoise = VirtualCamera.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();
    }

    // Update is called once per frame
    void Update()
    {
        if (clickAction.action.WasPressedThisFrame())
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                // Spawn particle effect at hit point
                if (hitParticlePrefab != null)
                {
                    GameObject particle = Instantiate(hitParticlePrefab, hit.point, Quaternion.identity);
                    Destroy(particle, 2f);
                }
                
                Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 explosionPosition = hit.point;
                    float explosionForce = 600f;
                    
                    rb.isKinematic = false;
                    rb.AddExplosionForce(explosionForce, explosionPosition, 1f);
                    
                    // Add random torque to make it spin
                    Vector3 randomTorque = new Vector3(
                        Random.Range(-10f, 10f),
                        Random.Range(-10f, 10f),
                        Random.Range(-10f, 10f)
                    );
                    rb.AddTorque(randomTorque, ForceMode.Impulse);
                    
                    // Add screen shake
                    if (virtualCameraNoise != null)
                    {
                        StartCoroutine(ScreenShake());
                    }
                }
            }
        }
    }

    private IEnumerator ScreenShake()
    {
        float elapsed = 0f;
        float initialAmplitude = 2f;
        float initialFrequency = 2f;
        
        while (elapsed < shakeDuration)
        {
            float t = elapsed / shakeDuration;
            float dampening = 1f - t;
            
            virtualCameraNoise.AmplitudeGain = initialAmplitude * dampening;
            virtualCameraNoise.FrequencyGain = initialFrequency * dampening;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        virtualCameraNoise.AmplitudeGain = 0f;
        virtualCameraNoise.FrequencyGain = 0f;
    }
}
