using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform player; // Takip edilecek karakter
    public Vector3 offset; // Kameranýn pozisyon ofseti
    public float smoothSpeed = 5f; // Takip hýzýný ayarlar
    private float num;
    private void LateUpdate()
    {
        if (player == null) return;

        // Hedef pozisyon: oyuncunun pozisyonu + ofset
        Vector3 targetPosition = player.position + offset;

        // Pürüzsüz takip için Lerp (Linear Interpolation) kullanýyoruz
        transform.position = Vector3.Lerp(transform.position, targetPosition, num);
    }
    private void FixedUpdate()
    {
        num = smoothSpeed * Time.deltaTime;
    }
}
