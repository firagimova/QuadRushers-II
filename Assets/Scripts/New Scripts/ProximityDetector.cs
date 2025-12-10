using Resources;
using UnityEngine;
using static EventList;

[RequireComponent(typeof(SphereCollider))]
public class ProximityDetector : MonoBehaviour
{
    // detection radius
    [Tooltip("Radius")]
    [SerializeField] private float detectionRadius = 5f;

    // the detection tag
    [SerializeField] private string targetTag = "Environment";

    private SphereCollider proximityCollider;

    private void Awake()
    {
        // manually add SphereCollider if not present
        proximityCollider = GetComponent<SphereCollider>();
        proximityCollider.isTrigger = true; // trigger on
        proximityCollider.radius = detectionRadius * 0.1f;
    }

    private void OnTriggerEnter(Collider other)
    {
        // comes close to an object with the specified tag
        if (other.CompareTag(targetTag))
        {
            print("YOU ARE CLOSE TO: " + other.name);
            EventBus<DetectionWarning>.Emit(this, new DetectionWarning());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // leaves the proximity of an object with the specified tag
        if (other.CompareTag(targetTag))
        {
            print("YOU ARE FAR FROM: " + other.name);
        }
    }

    // gizmos to visualize detection radius
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        float scaledRadius = detectionRadius * transform.lossyScale.y;

        Gizmos.DrawSphere(Vector3.zero, detectionRadius);
    }
}