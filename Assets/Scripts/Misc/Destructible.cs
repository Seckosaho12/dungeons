using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    [SerializeField] private GameObject destroyVFX;
    [SerializeField] private PickupSpawner pickupSpawner;

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.GetComponent<DamageSource>() || other.gameObject.GetComponent<Projectile>()) {
            if (pickupSpawner)
            {
                pickupSpawner.DropItems();
            }
            Instantiate(destroyVFX, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    private void Reset(){
        pickupSpawner = GetComponent<PickupSpawner>();
    }
}
