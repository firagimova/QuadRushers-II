using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollowerFunction : MonoBehaviour
{
    public Transform target; // Takip edilecek hedef nesne (drone)
    public float distance = 0.7f; // Kameranýn hedefe olan uzaklýðý
    public float height = 0.2f; // Kameranýn hedefin üstündeki yüksekliði
    public float rotationDamping = 50f; // Kameranýn rotasyonunun yumuþaklýðý

    // Update is called once per frame
    void LateUpdate()
    {
        // Hedef nesnenin konumunu ve rotasyonunu al
        Vector3 targetPosition = target.position;
        Quaternion targetRotation = target.rotation;

        // Hedefin x ve z rotasyonlarýný yoksayarak kameranýn rotasyonunu belirle
        Quaternion newRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);

        // Kameranýn yeni rotasyonunu hedefe doðru yumuþak bir þekilde döndür
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * rotationDamping);

        // Kameranýn hedefin arkasýnda ve yüksekliði belirtilen uzaklýkta konumlandýr
        Vector3 newPosition = targetPosition - (newRotation * Vector3.forward * distance);
        newPosition.y = targetPosition.y + height;
        transform.position = newPosition;
    }
}
