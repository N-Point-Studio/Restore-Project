using UnityEngine;

public class Tool : MonoBehaviour
{
    [Range(0.01f, 1f)] public float brushRadius = 0.1f;
    [Range(0f, 1f)] public float brushHardness = 0.5f;

    // Menggunakan OnCollisionStay agar objek terus terhapus selama spons digesek

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collision detected with {collision.gameObject.name}");
    }
    private void OnCollisionStay(Collision collision)
    {
        Debug.Log($"Collision detected with {collision.gameObject.name}");
        // Cek apakah objek yang ditabrak punya script PaintableArtifact
        LightmapCleaning artifact = collision.gameObject.GetComponent<LightmapCleaning>();

        if (artifact != null)
        {
            // collision.contacts berisi daftar titik persentuhan. 
            // Jika sponsnya ceper dan nempel sempurna, titik kontaknya bisa lebih dari satu!
            foreach (ContactPoint contact in collision.contacts)
            {
                // Kirim posisi persis dan normal dari titik tabrakan tersebut ke Artefak
                // contact.normal.Normalize(); // Pastikan normalnya sudah dinormalisasi
                Vector3 minusNormal = -contact.normal.normalized; // Arahkan ke dalam permukaan
                artifact.ReceivePaint(contact.point, contact.normal, brushRadius, brushHardness);
                Debug.Log($"[Shader] Contact Point: {contact.point}, Normal: {contact.normal}");
                //Contact Point: (0.04, 2.86, -0.02), Normal: (-0.01, -0.18, 0.98)
            }
        }
    }
}