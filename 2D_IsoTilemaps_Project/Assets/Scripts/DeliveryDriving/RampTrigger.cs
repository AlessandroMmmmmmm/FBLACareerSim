using UnityEngine;

public class RampTrigger : MonoBehaviour
{
    public WarehouseManager warehouseManager;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            warehouseManager.SetAtRamp(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            warehouseManager.SetAtRamp(false);
    }
}
