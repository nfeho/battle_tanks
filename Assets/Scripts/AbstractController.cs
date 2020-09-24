using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    Up,
    Down,
    Left,
    Right,
    None
}

public abstract class AbstractController : MonoBehaviour
{
    public float inputTimer;
    protected float timer = 0f;
    public Vector3 moveXY;
    protected Vector3 originalPosition;
    protected Vector3 newAngle;
    public Direction direction;
    public GameObject Bullet;
    public int team;

    // Start is called before the first frame update
    public virtual void Start()
    {
        timer = inputTimer;
        moveXY = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetDirection(Direction dir)
    {
        // Sets the property direction and correct rotation of tank
        direction = dir;
        switch(dir) {
            case Direction.Left:
                transform.eulerAngles = new Vector3(0, 0, 180f);
                break;
            case Direction.Right:
                transform.eulerAngles = new Vector3(0, 0, 0f);
                break;
            case Direction.Up:
                transform.eulerAngles = new Vector3(0, 0, 90f);
                break;
            case Direction.Down:
                transform.eulerAngles = new Vector3(0, 0, 270f);
                break;
        }
    }

    public void SetSprite(int teamID)
    {
        // Sets sprite color based on teamID
        if (teamID == 0)
            this.GetComponentInChildren<SpriteRenderer>().sprite = GlobalManager.Instance.greenSprite;
        if (teamID == 1)
            this.GetComponentInChildren<SpriteRenderer>().sprite = GlobalManager.Instance.blueSprite;
    }

    public Vector3 AngleLerp(Vector3 StartAngle, Vector3 FinishAngle, float t)
    {
        // Helper function to smooth out the rotation of tank
        float xLerp = Mathf.LerpAngle(StartAngle.x, FinishAngle.x, t);
        float yLerp = Mathf.LerpAngle(StartAngle.y, FinishAngle.y, t);
        float zLerp = Mathf.LerpAngle(StartAngle.z, FinishAngle.z, t);
        Vector3 Lerped = new Vector3(xLerp, yLerp, zLerp);
        return Lerped;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Collider to prevent weird behaviour on collider entering the Walls collider
        if (collision.gameObject.name == "Walls")
        {
            moveXY = originalPosition;
            timer = 0f;
        }
    }
}
