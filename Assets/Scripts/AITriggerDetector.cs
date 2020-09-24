using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITriggerDetector : MonoBehaviour
{
    private AIController AI;

    // Start is called before the first frame update
    void Start()
    {
        AI = this.gameObject.GetComponentInParent<AIController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // This is circular trigger for simulating of AI vision, after bullet or player entering this trigger, it is detected
        if (collision.gameObject.layer == LayerMask.NameToLayer("Bullet"))
        {
            AI.OnBulletDetected(collision.gameObject);

        }


        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            AI.OnEnemyDetected(collision.gameObject);

        }
    }
}
