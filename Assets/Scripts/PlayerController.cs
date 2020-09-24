using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : AbstractController
{
    public KeyCode upKey;
    public KeyCode downKey;
    public KeyCode rightKey;
    public KeyCode leftKey;
    public KeyCode shootKey;

    // Start is called before the first frame update

    public override void Start()
    {
        timer = inputTimer;
        moveXY = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Handles player movement according to input
        // Players can move only after inputTimer seconds after previous action
        // Shooting takes twice as long as movement or rotation
        if (timer >= inputTimer)
        {
            originalPosition = transform.position;
            transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), Mathf.Round(transform.position.z));

            if (Input.GetKey(upKey))
            {
                if (direction != Direction.Up)
                {
                    newAngle = new Vector3(0, 0, 90);
                    direction = Direction.Up;
                } else
                {
                    moveXY = transform.position + new Vector3(0, 1f, 0);
                }
                timer = 0f;
            }

            else if (Input.GetKey(downKey))
            {
                if (direction != Direction.Down)
                {
                    newAngle = new Vector3(0, 0, 270);
                    direction = Direction.Down;
                } else
                {
                    moveXY = transform.position + new Vector3(0, -1f, 0);
                }
                timer = 0f;

            }

            else if (Input.GetKey(leftKey))
            {
                if (direction != Direction.Left)
                {
                    newAngle = new Vector3(0, 0, 180);
                    direction = Direction.Left;
                }
                else
                {
                    moveXY = transform.position + new Vector3(-1f, 0, 0);
                }
                timer = 0f;
            }

            else if (Input.GetKey(rightKey))
            {
                if (direction != Direction.Right)
                {
                    newAngle = new Vector3(0, 0, 0);
                    direction = Direction.Right;
                }
                else
                {
                    moveXY = transform.position + new Vector3(1f, 0, 0);
                }
                timer = 0f;
            }
            else if (Input.GetKey(shootKey))
            {
                Vector3 bulletPosition;
                switch (direction)
                {
                    case Direction.Up:
                        bulletPosition = new Vector3(0, 0.5f, 0);
                        break;
                    case Direction.Down:
                        bulletPosition = new Vector3(0, -0.5f, 0);
                        break;
                    case Direction.Right:
                        bulletPosition = new Vector3(0.5f, 0, 0);
                        break;
                    case Direction.Left:
                        bulletPosition = new Vector3(-0.5f, 0, 0);
                        break;
                    default:
                        bulletPosition = new Vector3(0.5f, 0, 0);
                        break;
                }
                GameObject.Instantiate(Bullet, transform.position + bulletPosition, transform.rotation);
                timer = -inputTimer;
            }
            else
            {
                moveXY = transform.position;
            }

        }

        timer += Time.deltaTime;
        transform.eulerAngles = AngleLerp(transform.eulerAngles, newAngle, timer/ 1.6f);
        transform.position = Vector3.Lerp(transform.position, moveXY, timer/2f);

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Walls")
        {
            moveXY = originalPosition;
            timer = 0f;
        }

        if (collision.gameObject.name.Contains("Bullet"))
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}
