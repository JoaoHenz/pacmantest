using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowCorridor : MonoBehaviour
{
    private GameManager gameManager;

    private void Start()
    {
        GameObject gmGameObject = GameObject.FindWithTag("GameManager");
        gameManager = gmGameObject.GetComponent(typeof(GameManager)) as GameManager;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent(typeof(Ghost)))
        {
            Character character = collision.gameObject.GetComponent(typeof(Character)) as Character;

            character.speed = gameManager.ghostSpeed * 0.5f;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent(typeof(Ghost)))
        {
            Character character = collision.gameObject.GetComponent(typeof(Character)) as Character;

            character.speed = gameManager.ghostSpeed;
        }
    }
}
