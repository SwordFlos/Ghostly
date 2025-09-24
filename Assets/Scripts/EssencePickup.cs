using UnityEngine;

public class EssencePickup : MonoBehaviour
{
    public float essenceValue = 1f;
    public GameObject collectEffect; // Optional

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EssenceManager essenceManager = other.GetComponent<EssenceManager>();

            if (essenceManager != null)
            {
                essenceManager.AddEssence(essenceValue);

                if (collectEffect != null)
                {
                    Instantiate(collectEffect, transform.position, Quaternion.identity);
                }

                Destroy(gameObject);
            }
        }
    }
}