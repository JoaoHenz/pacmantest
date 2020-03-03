using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    private Vector3 teleporter1 = new Vector3(-13.5f, 2.5f, 5f);
    private Vector3 teleporter2 = new Vector3(13.5f, 2.5f, 5f);


    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.gameObject.GetComponent(typeof(Character)))
        {
            Character character = collision.gameObject.GetComponent(typeof(Character)) as Character;


            if (Vector3.Distance(teleporter1, collision.transform.position) >
                Vector3.Distance(teleporter2, collision.transform.position))
            { //collided with tp2, go next to tp1
                collision.gameObject.transform.position = new Vector3(teleporter1.x + 2,
                    teleporter1.y,
                    collision.gameObject.transform.position.z);
            }
            else
            {//collided with tp1, go next to tp2
                collision.gameObject.transform.position = new Vector3(teleporter2.x - 2,
                    teleporter2.y,
                    collision.gameObject.transform.position.z);
            }

            character.haveTeleported = true;
        }
    }
}
