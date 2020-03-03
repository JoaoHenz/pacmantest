using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public List<AudioSource> sirens;
    public AudioSource powerPellet;
    private int currentSiren=0;

    public AudioSource eatGhost;
    public AudioSource retreating;
    public AudioSource eatFruitSound;

    private int maxPellets = 242;
    

    public void PlayPower(bool shouldPlay)
    {
        if (shouldPlay)
        {
            sirens[currentSiren].Stop();
            powerPellet.Play();
        }
        else
        {
            powerPellet.Stop();
            sirens[currentSiren].Play();
        }

    }

    public void PlaySiren()
    {
        if (!sirens[currentSiren].isPlaying)
            sirens[currentSiren].Play();
    }

    public void CheckSiren(int pellets)
    {
        int threshhold = maxPellets / sirens.Count;
        int correctSiren = pellets / threshhold;
        if (correctSiren < sirens.Count){
            if (correctSiren != currentSiren)
            {
                if (sirens[currentSiren].isPlaying)
                    sirens[currentSiren].Stop();
                currentSiren = correctSiren;
                PlaySiren();
            }
        }

        
    }

    public void StopSiren()
    {
        if(sirens[currentSiren].isPlaying)
            sirens[currentSiren].Stop();
    }

    public void StopAll()
    {

        sirens[currentSiren].Stop();
        retreating.Stop();
        powerPellet.Stop();
    }


}
