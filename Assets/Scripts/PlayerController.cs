using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Inventory inventory;
    public bool inventoryShowing = false;

    public TileClass selectedTile;

    public int Reach;
    public Vector2Int mousePos;
    
    public float moveSpeed;
    public float jumpForce;
    public bool onGround;


    private Rigidbody2D rb;
    private Animator anim;

    public float horizontal;
    public bool dig;
    public bool place;

    [HideInInspector]
    public Vector2 spawnPos;

    public TerrarianGen terrainGenerator;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        inventory = GetComponent<Inventory>();
    }

    public void Spawn()
    {
        GetComponent<Transform>().position = spawnPos;        
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("Ground"))
        {
            onGround = true;
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Ground"))
        {
            onGround = false;
        }
    }


    private void FixedUpdate()
    {
        // player Movement
        horizontal = Input.GetAxis("Horizontal");
        float jump = Input.GetAxisRaw("Jump");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(horizontal * moveSpeed, rb.velocity.y);

        dig = Input.GetMouseButton(0);
        place = Input.GetMouseButton(1);



        if (Vector2.Distance(transform.position, mousePos) <= Reach)
        {
            if (dig)
            {
                terrainGenerator.RemoveTile(mousePos.x, mousePos.y);
            }
            else if (place)
            {
                terrainGenerator.placeTile(selectedTile.tileSprites, mousePos.x, mousePos.y, false);
            }
        }
        


        if (horizontal > 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (horizontal < 0)
            transform.localScale = new Vector3(1, 1, 1);


        if (vertical > 0.1f || jump > 0.1f)
        {
            if (onGround)
             movement.y = jumpForce;
        }

        rb.velocity = movement;
    }

    private void Update()
    {
        // Set Mouse Position
        mousePos.x = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).x - 0.5f);
        mousePos.y = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).y - 0.5f);


        if (Input.GetKeyDown(KeyCode.E))
        {
            inventoryShowing = !inventoryShowing;
        }

        inventory.InventoryUI.SetActive(inventoryShowing);

        anim.SetFloat("Horizontal", horizontal);
        anim.SetBool("dig", dig || place);

    }
}