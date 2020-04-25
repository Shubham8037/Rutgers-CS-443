using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player2Controller : MonoBehaviour
{
    public float speed_2;
    public Text countText_2;
    public Text winText_2;

    private Rigidbody rb_2;
    public int count_2;
    public int ballspeed = 30;
    public int jumpspeed = 30;
    private bool istouching_2 = true;

    void Start()
    {
        rb_2 = GetComponent<Rigidbody>();

        count_2 = 0;
        SetCountText_2();
        winText_2.text = "";

    }
    
    void FixedUpdate()
    {
        float moveHorizontal_2 = Input.GetAxis("Horizontal_2");
        float moveVertical_2 = Input.GetAxis("Vertical_2");
        float jump_2;
        
        if ((Input.GetKeyDown(KeyCode.W)) && istouching_2 == true)
        {
            jump_2 = 15f;
        }

        else
        {
            istouching_2 = false;
            jump_2 = 0;
        }

        Vector3 movement_2 = new Vector3(moveHorizontal_2, jump_2, moveVertical_2);

        rb_2.AddForce(movement_2 * speed_2);
    }

    private void OnCollisionStay(Collision collision)
    {

        istouching_2 = true;

    }

    void OnTriggerEnter(Collider other_2)
    {
        if (other_2.gameObject.CompareTag("Pick Up"))
        {
           other_2.gameObject.SetActive(false);

            count_2 += 1;
            SetCountText_2();
        }

        if (other_2.gameObject.tag == "DeadZone")
        {
            count_2 -= 1;
            if (count_2 < 0)
            {
                count_2 = 0;
            }
            SetCountText_2();
        }
    }

    void Update()
    {
        
    }

    void SetCountText_2()
    {
        countText_2.text = "P2 Score: " + count_2.ToString();

        if (count_2 >= 12)
        {
            winText_2.text = "Player 2 Wins!";
        }

    }
}
