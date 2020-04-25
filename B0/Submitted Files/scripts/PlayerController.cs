using UnityEngine;

// Include the namespace required to use Unity UI
using UnityEngine.UI;

using System.Collections;

public class PlayerController : MonoBehaviour {
	
	public float speed;
	public Text countText;
	public Text winText;
    public Text countTime;
    public Text remtime;

    private Rigidbody rb;
	public int count;
    public float timer = 120.0f;
    public int ballspeed = 30;
    public int jumpspeed = 30;
    private bool istouching = true;

    void Start ()
	{
		// Assign the Rigidbody component to our private rb variable
		rb = GetComponent<Rigidbody>();

		count = 0;
        SetCountText ();
        winText.text = "";
        remtime.text = "";

    }

	// Each physics step..
	void FixedUpdate ()
	{
		// Set some local float variables equal to the value of our Horizontal and Vertical Inputs
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");
        float jump;

        if ((Input.GetKeyDown(KeyCode.UpArrow)) && istouching == true)
        {
            jump = 15f;
        }

        else
        {
            istouching = false;
            jump = 0;
        }

		// Create a Vector3 variable, and assign X and Z to feature our horizontal and vertical float variables above
		Vector3 movement = new Vector3 (moveHorizontal, jump, moveVertical);

		// Add a physical force to our Player rigidbody using our 'movement' Vector3 above, 
		// multiplying it by 'speed' - our public player speed that appears in the inspector
		rb.AddForce (movement * speed);
	}

    private void OnCollisionStay(Collision collision)
    {
        istouching = true;
    }

    // When this game object intersects a collider with 'is trigger' checked, 
    // store a reference to that collider in a variable named 'other'..
    void OnTriggerEnter(Collider other) 
	{
		// ..and if the game object we intersect has the tag 'Pick Up' assigned to it..
		if (other.gameObject.CompareTag ("Pick Up"))
		{
			// Make the other game object (the pick up) inactive, to make it disappear
			other.gameObject.SetActive (false);

			count += 1;
            SetCountText ();
		}

        if (other.gameObject.tag == "DeadZone")
        {
            count -= 1;
            if(count < 0)
            {
                count = 0;
            }
            SetCountText();
        }
    }

    void Update()
    {
        timer -= Time.deltaTime;
        SetCountTime();
        if (Mathf.Round(timer) == 30)
        {
            remtime.text = Mathf.Round(timer) + " secs remaining!";
            Destroy(remtime, 2);
        }
        if (Mathf.Round(timer) == 1)
        {

            SetCountTime();

        }
        if (Mathf.Round(timer) == 0)
        {

            timer = 0f;
            SetCountTime();

        }
    }

    void SetCountText()
	{
		countText.text = "P1 Score: " + count.ToString ();

        if (count >= 12)
		{
			winText.text = "Player 1 Wins!";
        }

    }
    void SetCountTime()
    {
        countTime.text = Mathf.Round(timer) + " secs left";

    }
}