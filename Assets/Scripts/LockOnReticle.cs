using UnityEngine;

public class LockOnReticle : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public float pulseSpeed = 5f;
    public float pulseAmount = 0.2f;

    void Update()
    {
        // Rotate the reticle
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

        // pulsing effect

        //float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        //transform.localScale = new Vector3(scale, scale, 1f);
    }

}
