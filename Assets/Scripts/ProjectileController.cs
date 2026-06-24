using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public int Damage;
    public AttackerController Target;
    public float Speed = 3f;
    public float targetThreshold = 0.05f;

    void Update()
    {
        if (Target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Rotate toward target
        Vector3 direction =
            Target.transform.position -
            transform.position;

        float angle =
            Mathf.Atan2(direction.y, direction.x) *
            Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(0, 0, angle);


        // Move toward target
        transform.position =
            Vector3.MoveTowards(
                transform.position,
                Target.transform.position,
                Speed * Time.deltaTime
            );


        float distance =
            Vector3.Distance(
                transform.position,
                Target.transform.position
            );

        if (distance <= targetThreshold)
        {
            Target.TakeDamage(Damage);
            Destroy(gameObject);
        }
    }
}