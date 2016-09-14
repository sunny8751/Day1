using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public Transform playerTransform;

	//height of the player in world units
	float playerHeight;

	TerrainGenerator gen;
	Rigidbody2D myRigidBody;
	Animator anim;

	int chunkPos = 0, chunkSize;
	GameObject beginningObject;

	float baseSpeed = 1.5f, speed;
	float jumpForce = 500f;
	bool facingRight = true;
	public bool jumped = false;
	float jumpTime, attackTime;
	bool grounded = false; // Flag to check whether the character is on the ground
	public Transform groundCheck;
	float groundRadius = 0.2f;
	public LayerMask whatIsGround;
	GameObject inventoryObject;
	Inventory inventory;
	bool canMove = true;

	GameObject swordCollider, hatchetCollider, hatchetButton;

	Transform targetTree;
	float targetTreeMaxHealth = 50, targetTreeHealth;
	float hatchetDamage = 10;
	bool usingHatchet = false;

	bool inWater = false, wasUnderWater = false;
	float waterLevel, buoyancyForce = 40f;

	Sprite [] playerSprites;

	//mobile controls
	//private Vector2 touchOrigin = -Vector2.one;
	Vector2 movement = Vector2.zero;
	//if there is a touch over any of the buttons
	bool moving = false;
	//for double clicking buttons to run
	float movementTime = 0;

	GameObject downButton;

	GameObject[] tempUI;

	void Start () {
		myRigidBody = GetComponent<Rigidbody2D> ();
		anim = GetComponent<Animator> ();

		speed = baseSpeed;

		chunkSize = TerrainGenerator.chunkSize;
		playerTransform.position = new Vector3 (GameHandler.cameraExtent, TerrainGenerator.getHeight(-chunkSize / 2), -10);

		gen = GameObject.Find ("Terrain").GetComponent<TerrainGenerator> ();
		beginningObject = GameObject.Find ("Beginning");

		inventoryObject = GameObject.FindWithTag ("Inventory Panel");
		inventoryObject.SetActive (false);

		inventory = GameObject.Find ("GameHandler").GetComponent<Inventory> ();

		playerSprites = Resources.LoadAll<Sprite> ("Player");

		swordCollider = transform.FindChild ("torso/rightarm/sword collider").gameObject;
		hatchetCollider = transform.FindChild ("torso/rightarm/hatchet collider").gameObject;
		hatchetCollider.SetActive (false);

		hatchetButton = GameObject.Find ("HatchetButton");
		hatchetButton.SetActive (false);
		targetTreeHealth = targetTreeMaxHealth;

		playerHeight = (transform.FindChild("torso/face").position.y + transform.FindChild("torso/face").GetComponent<SpriteRenderer> ().sprite.bounds.size.y) -
			(transform.FindChild("rightleg").position.y - transform.FindChild("rightleg").GetComponent<SpriteRenderer> ().sprite.bounds.size.y);

		downButton = GameObject.Find ("DownButton");
		downButton.SetActive (false);

		transform.position = gen.getPlayerStartingPosition ();
	}
	void Update () {
		//water
		if (!inWater && gen.isWater (transform.position.x, 0, false)) {
			//entered the water zone
			inWater = true;
			waterLevel = -3.46f;
		}
		if (inWater) {
			//player is underwater
			if (transform.position.y <= waterLevel) {
				if (!wasUnderWater) {
					if (transform.localScale.x == 1) {
						//facing right
						transform.localScale = new Vector3 (1, 1, 1);
					} else {
						//facing left 
						transform.localScale = new Vector3 (1, -1, 1);
					}
					if (speed > baseSpeed * 1.2f) {
						movement.x = baseSpeed;
					}
					speed = baseSpeed;
					jumped = false;
					downButton.SetActive (true);
				}
				wasUnderWater = true;
				anim.SetBool ("Water", true);
				myRigidBody.drag = 5f;
			}
			//exit the water zone
			int direction = facingRight ? 1 : -1;
			float tolerance = 0.5f;
			if (Mathf.Abs(transform.position.y - waterLevel) <= 0.2f && !gen.isWater (transform.position.x + direction * (playerHeight / 2 + gen.length / 2 + 0.1f + tolerance), 0, false)) {
				//close enough to the water's edge to jump
				bool jumpPressed;
				#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
				jumpPressed = Input.GetButtonDown("Jump");
				#else
				jumpPressed = Input.touchCount > 0 && !EventSystem.current.IsPointerOverGameObject(Input.touches[Input.touches.Length - 1].fingerId) && Input.touches[Input.touches.Length - 1].phase == TouchPhase.Began;
				#endif
				if (jumpPressed) {
					jumped = true;
					myRigidBody.velocity = new Vector2(myRigidBody.velocity.x, 0);
					myRigidBody.AddForce (new Vector2 (0, 1.2f * jumpForce));
					anim.SetTrigger ("Jump");
					wasUnderWater = false;
					myRigidBody.drag = 0;
					anim.SetBool ("Water", false);
					downButton.SetActive (false);
					if (transform.localScale.y == 1) {
						//facing right
						transform.localScale = new Vector3 (1, 1, 1);
					} else {
						//facing left 
						transform.localScale = new Vector3 (-1, 1, 1);
					}
					//transform.position = other.collider.transform.position + new Vector3 (0, 0.5f + playerHeight / 2, 0);
				}
			}
			if (grounded) {
				inWater = false;
			}
		}
		//attack
		if (!canMove &&  Time.time >= attackTime + 0.1f && anim.GetCurrentAnimatorStateInfo(0).IsName("Attack")) {
			canMove = true;
			if (usingHatchet) {
				//using hatchet
				targetTreeHealth -= hatchetDamage;
				targetTree.GetComponent<SpriteRenderer> ().color = Color.white;
				if (targetTreeHealth <= 0) {
					//fell tree
					GameHandler.removeObject(chunkPos, targetTree.position);
					targetTree.gameObject.SetActive (false);
					targetTree = null;
					hatchetButton.SetActive (false);
					targetTreeHealth = targetTreeMaxHealth;
					for (int i = 0; i < Random.Range (1, 4); i++) {
						inventory.AddItem (0);
					}
				}
				usingHatchet = false;
			}
		}

		#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
		if (Input.GetKeyDown (KeyCode.LeftCommand)) {
			attack ();
		}
		//run
		if (Input.GetKeyDown (KeyCode.LeftShift)) {
			speed = 1.5f*baseSpeed;
		} else if (Input.GetKeyUp (KeyCode.LeftShift)) {
			speed = baseSpeed;
		}
		//jump
		if (Input.GetButtonDown ("Jump") && grounded) {
			//jump pressed
			anim.SetBool("Ground", false);
			myRigidBody.AddForce (new Vector2 (0, jumpForce));
			anim.SetTrigger ("Jump");
			jumped = true;
			jumpTime = Time.time + 0.1f;
		}
		//toggle inventory
		if (Input.GetKeyDown (KeyCode.I)) {
		inventoryObject.SetActive (!inventoryObject.activeSelf);
		}
		#else
		//jump
		if (Input.touchCount > 0 && !EventSystem.current.IsPointerOverGameObject(Input.touches[Input.touches.Length - 1].fingerId) && Input.touches[Input.touches.Length - 1].phase == TouchPhase.Began && grounded) {
		//jump pressed
		anim.SetBool("Ground", false);
		myRigidBody.AddForce (new Vector2 (0, jumpForce));
		anim.SetTrigger ("Jump");
		jumped = true;
		jumpTime = Time.time + 0.1f;
		}
		#endif
		if (jumped && jumpTime <= Time.time && grounded) {
			//landed on the ground after jumping
			anim.SetTrigger ("Land");
			jumped = false;
		}

		//check if grounded
		grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, whatIsGround);
		anim.SetBool ("Ground", grounded);

		//position bounds
		if (transform.position.y < -4.8f) {
			//bottom of screen
			transform.position = new Vector3 (transform.position.x, -4.8f, 0);
		}

		endlessSideScrolling ();
	}

	void endlessSideScrolling() {
		//responsible for the endless sidescrolling terrain
		int newChunkPos = (int) (playerTransform.position.x - (playerTransform.position.x % chunkSize)) / chunkSize;
		if (chunkPos != newChunkPos) {
			if (newChunkPos > chunkPos) {
				//going right
				//create new chunk
				gen.GenerateChunk(newChunkPos + 1, "grass");
				if (chunkPos != 0) {
					//recycle old chunk
					gen.Recycle (chunkPos - 1);
				} else {
					beginningObject.SetActive (false);
				}
				//see if it's the farthest player has explored
				if (newChunkPos > TerrainGenerator.maxChunkExplored) {
					TerrainGenerator.maxChunkExplored = newChunkPos;
				}
			} else {
				//going left
				if (newChunkPos != 0) {
					//create a new chunk
					gen.GenerateChunk(newChunkPos - 1, "grass");
				} else {
					beginningObject.SetActive (true);
				}
				//just recycle old chunk
				gen.Recycle(chunkPos + 1);
			}
			chunkPos = newChunkPos;
		}
	}

	void FixedUpdate() {
		bool downDown = false;
		//player movement
		float move;
		#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
		move = Input.GetAxis("Horizontal")*speed;
		downDown = Input.GetAxis ("Vertical") < 0;
		#else
		/**if (Input.touchCount > 0) {
			Touch myTouch = Input.touches[0];
			if (myTouch.phase == TouchPhase.Began) {
				touchOrigin = myTouch.position;
			} else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0) {
				Vector2 touchEnd = myTouch.position;
				float x = touchEnd.x - touchOrigin.x;
				float y = touchEnd.y - touchOrigin.y;
				touchOrigin.x = -Vector2.one;
				if (Mathf.Abs(x) > Mathf.Abs(y)) {
					move = x > 0 ? 1 : -1;
					move *= speed;
				}
			}
		}**/
		move = movement.x;
		downDown = movement.y < 0;
		#endif
		anim.SetFloat ("Speed", Mathf.Abs (move));
		if ((move > 0 && !facingRight) || (move < 0 && facingRight)) {
			movementTime = 0;
			Flip ();
		}
		if (canMove && move != 0) {
			if (wasUnderWater && !gen.isWater (transform.position.x + (move / Mathf.Abs(move)) * (playerHeight / 2 + gen.length / 2 + 0.1f), 0, false)) {
			} else {
				myRigidBody.velocity = new Vector2 (move * speed, myRigidBody.velocity.y);
			}
		} else {
			myRigidBody.velocity = new Vector2 (0, myRigidBody.velocity.y);
		}
		//water buoyancy
		if (inWater && transform.position.y <= waterLevel) {
			if (downDown) {
				//allow player to move down
				myRigidBody.velocity = new Vector2 (myRigidBody.velocity.x, -1f);
			} else if (!jumped) {
				//buoyancy force is pushing player up
				myRigidBody.AddForce (Vector2.up * buoyancyForce, ForceMode2D.Force);
			}
		}
	}

	public void toggleInventory() {
		if (!inventoryObject.activeSelf) {
			//fix the size of the slots
			for (int i = 0; i < inventory.slotAmount; i++) {
				inventory.slots [i].GetComponent<RectTransform> ().localScale = Vector3.one;
			}
		}
		inventoryObject.SetActive (!inventoryObject.activeSelf);
		toggleUI (!inventoryObject.activeSelf);
		canMove = !inventoryObject.activeSelf;
	}

	public void leftDown() {
		moving = true;
		if (Time.time < movementTime + 0.5f && speed < baseSpeed * 1.2f && !inWater) {
			//double tapped- run
			speed = 1.5f * baseSpeed;
		} else {
			movementTime = Time.time;
		}
		movement.y = 0;
		movement.x = -speed;
	}

	public void leftEnter() {
		if (!moving) {
			return;
		}
		movement.y = 0;
		movement.x = -speed;
	}

	public void rightDown() {
		moving = true;
		if (Time.time < movementTime + 0.5f && speed < baseSpeed * 1.2f && !inWater) {
			//double tapped- run
			speed = 1.5f * baseSpeed;
		} else {
			movementTime = Time.time;
		}
		movement.y = 0;
		movement.x = speed;
	}

	public void rightEnter() {
		if (!moving) {
			return;
		}
		movement.y = 0;
		movement.x = speed;
	}

	public void leftRightUp() {
		moving = false;
		movement = Vector2.zero;
		speed = baseSpeed;
	}

	public void downEnter() {
		if (!moving) {
			return;
		}
		movement.x = 0;
		movement.y = -1;
	}

	public void downDown() {
		moving = true;
		movement.x = 0;
		movement.y = -1;
	}

	public void downUp() {
		moving = false;
		movement = Vector2.zero;
	}

	public void attack() {
		if (!grounded || Time.time < attackTime + 0.3f) {
			return;
		}
		if (!usingHatchet) {
			changePlayer ("sword");
		}
		anim.SetTrigger ("Attack");
		attackTime = Time.time;
		canMove = false;
	}

	public void useHatchet() {
		if (usingHatchet || !grounded || !canMove || Time.time < attackTime + 0.3f) {
			return;
		}
		usingHatchet = true;
		changePlayer ("hatchet");
		attack ();
		targetTree.GetComponent<SpriteRenderer> ().color = Color.red;
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.tag == "Tree") {
			hatchetButton.SetActive (true);
			targetTree = other.transform;
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		if (other.tag == "Tree" && !usingHatchet) {
			hatchetButton.SetActive (false);
			targetTree = null;
		}
	}

	void Flip() {
		facingRight = !facingRight;
		Vector3 theScale = transform.localScale;
		if (wasUnderWater) {
			theScale.y *= -1;
		} else {
			theScale.x *= -1;
		}
		transform.localScale = theScale;
	}

	/**void Flip(float direction) {
		if (direction > 0) {
			facingRight = true;
		} else {
			facingRight = false;
		}
		Vector3 theScale = transform.localScale;
		if (inWater) {
			if (facingRight) {
				theScale.y = 1;
			} else {
				theScale.y = -1;
			}
		} else {
			if (facingRight) {
				theScale.x = 1;
			} else {
				theScale.x = -1;
			}
		}
		transform.localScale = theScale;
	}**/

	void toggleUI (bool toggle) {
		if (toggle) {
			//turn on
			foreach (GameObject obj in tempUI) {
				obj.SetActive(true);
			}
		} else {
			//turn off
			//for inventory
			tempUI = new GameObject[] {GameObject.Find("SwordButton"), hatchetButton, GameObject.Find("RightButton"), GameObject.Find("LeftButton"), downButton};
			foreach (GameObject obj in tempUI) {
				obj.SetActive(false);
			}
		}
	}

	void changePlayer (string part) {
		switch (part) {
		case "frown":
			transform.FindChild("torso/face").GetComponent<SpriteRenderer>().sprite = playerSprites[2];
			break;
		case "mouth closed":
			transform.FindChild("torso/face").GetComponent<SpriteRenderer>().sprite = playerSprites[3];
			break;
		case "mouth open":
			transform.FindChild("torso/face").GetComponent<SpriteRenderer>().sprite = playerSprites[4];
			break;
		case "surprised":
			transform.FindChild("torso/face").GetComponent<SpriteRenderer>().sprite = playerSprites[5];
			break;
		case "hatchet":
			transform.FindChild ("torso/rightarm").GetComponent<SpriteRenderer> ().sprite = playerSprites [6];
			swordCollider.SetActive (false);
			hatchetCollider.SetActive (true);
			break;
		case "sword":
			transform.FindChild ("torso/rightarm").GetComponent<SpriteRenderer> ().sprite = playerSprites [14];
			hatchetCollider.SetActive (false);
			swordCollider.SetActive (true);
			break;
		default:
			break;
		}
	}
}
