using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public bool playerControlled = true;
    public float speed = 0.1f;
    Rigidbody2D rb;


	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();
    }
	
	// Update is called once per frame
	void Update () {
        if (playerControlled)
            MoveInput();
	}

    void MoveInput()
    {
        /*
        if (Input.GetKey("up"))
            rb.MovePosition(transform.position + Vector3.up*speed);
        
        if (Input.GetKey("down"))
            rb.MovePosition(transform.position + Vector3.down * speed);
     
        if (Input.GetKey("left"))
            rb.MovePosition(transform.position + Vector3.left * speed);
   
        if (Input.GetKey("right"))
            rb.MovePosition(transform.position + Vector3.right * speed);*/

        var v3 = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0.0f);
        rb.MovePosition(transform.position + speed * v3.normalized * speed);
    }
}
