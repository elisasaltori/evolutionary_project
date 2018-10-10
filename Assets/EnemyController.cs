using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {

    Vector3 initialPosition; //save position for resetting level
    public float speed = 2.5f;

   

	// Use this for initialization
	void Start () {
        initialPosition = this.transform.position;


	}
	
	// Update is called once per frame
	void Update () {
        MoveHorizontal();
	}

    void MoveHorizontal()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

 
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Border")
            speed *= -1;
    }




}
