using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Moves player camera together with player
        if (player != null)
            transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -4);
        else
            transform.position = new Vector3(-500f, -500f, -4);
    }

}
