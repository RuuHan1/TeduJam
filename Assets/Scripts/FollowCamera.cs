using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform player; // Takip edilecek karakter
    public Vector3 offset; // Kameran�n pozisyon ofseti
    public float smoothSpeed = 5f; // Takip h�z�n� ayarlar
    private float num;
    private void LateUpdate()
    {
        if (player == null) return;

        // Hedef pozisyon: oyuncunun pozisyonu + ofset
        Vector3 targetPosition = player.position + offset;

        // P�r�zs�z takip i�in Lerp (Linear Interpolation) kullan�yoruz
        transform.position = Vector3.Lerp(transform.position, targetPosition, num);
    }
    private void FixedUpdate()
    {
        num = smoothSpeed * Time.deltaTime;
    }
}
