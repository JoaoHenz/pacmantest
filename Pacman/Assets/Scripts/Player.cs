using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player : Character
{
    public bool isDead;
    public AudioSource eatPelletSound1;
    public AudioSource eatPelletSound2;
    private bool pelletOne;
   

    protected override void Update()
    {
        base.Update();

        if (!isDead)
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            if (v == 1)
                bufferedDir = Direction.up;
            if (v == -1)
                bufferedDir = Direction.down;
            if (h == 1)
                bufferedDir = Direction.right;
            if (h == -1)
                bufferedDir = Direction.left;

        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "Edible")
        {
            Vector3Int currentCell = this.currentCell;
            if (gameManager.edible.GetTile(currentCell) == gameManager.pelletTile)
            {
                gameManager.edible.SetTile(currentCell, null);
                gameManager.pelletsEaten += 1;
                gameManager.Score += 10;
                soundManager.CheckSiren(gameManager.pelletsEaten);
                gameManager.checkFruitSpawn();

                if (pelletOne)
                {
                    eatPelletSound1.Play();
                    pelletOne = false;
                }
                else
                {
                    eatPelletSound2.Play();
                    pelletOne = true;
                }
                
            }
            if (gameManager.edible.GetTile(currentCell) == gameManager.powerUpTile)
            {
                gameManager.Score += 50;
                gameManager.edible.SetTile(currentCell, null);
                gameManager.IntoPowerMode = true;
            }

        }
        else
        {
            if (collision.gameObject.tag == "Fruit")
            {
                gameManager.Score += gameManager.fruitReward();
                soundManager.eatFruitSound.Play();
                StartCoroutine(gameManager.ShowScoreGained(gameManager.fruitRewards[gameManager.currentFruit], collision.gameObject.transform.position));
                if (gameManager.nextFruitPos < gameManager.fruitPositions.Count)
                {
                    collision.gameObject.transform.position = gameManager.fruitPositions[gameManager.nextFruitPos];
                    gameManager.nextFruitPos += 1;
                }
                else
                {
                    gameManager.nextFruitPos = 0;
                    collision.gameObject.transform.position = gameManager.fruitPositions[gameManager.nextFruitPos];
                    gameManager.nextFruitPos += 1;
                }
                gameManager.fruit = null;
                

            }
        }
    }

    protected override void changeAnim(bool value)
    {

        if (value)
        {
            switch (currentDir)
            {
                case Direction.up:
                    transform.eulerAngles = new Vector3(0, 0, 90);
                    break;
                case Direction.down:
                    transform.eulerAngles = new Vector3(0, 0, -90);
                    break;
                case Direction.right:
                    transform.eulerAngles = new Vector3(0, 0, 0);
                    break;
                case Direction.left:
                    transform.eulerAngles = new Vector3(0, 0, 180);
                    break;

            }
        }

        animator.SetBool("isMoving", value);
    }

    public void playDeath()
    {
        animator.SetBool("isDead", true);
        bufferedDir = Direction.none;
    }

    public void ressurect()
    {
        SpriteRenderer spriteRenderer = this.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
        spriteRenderer.enabled = true;
        isDead = false;
        animator.SetBool("isDead", false);
        bufferedDir = Direction.none;
    }



}
