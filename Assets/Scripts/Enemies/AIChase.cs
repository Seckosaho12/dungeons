using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIChase : MonoBehaviour
{
    public float speed;
    private float distance;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        distance = Vector2.Distance(transform.position, PlayerController.Instance.gameObject.transform.position);
        Vector2 direction = PlayerController.Instance.gameObject.transform.position - transform.position;
        
        transform.position = Vector2.MoveTowards(this.transform.position, PlayerController.Instance.gameObject.transform.position, speed * Time.deltaTime);
        
    }
}
