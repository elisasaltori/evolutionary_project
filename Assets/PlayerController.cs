using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public bool playerControlled = true;
    public float speed = 3f;
    Rigidbody2D rb;
    Vector3 initialPos;


	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();
        initialPos = transform.position;
       
    }
	
	// Update is called once per frame
	void Update () {
        if (playerControlled)
            MoveInput();
	}

    void MoveInput()
    {
        var v3 = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0.0f);
        rb.MovePosition(transform.position + speed * v3.normalized * speed *Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
            transform.position = initialPos;
        else
            if (collision.gameObject.tag == "Goal")
            print("I win!");
    }
}
