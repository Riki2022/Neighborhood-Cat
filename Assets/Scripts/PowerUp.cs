using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public GameObject powerUp;
    public GameObject audio;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != 3)
            return;
        switch (powerUp.name)
        {
            case "DoubleJumpPowerUp":
                audio.GetComponentsInChildren<AudioSource>()[2].Play();
                CharacterController2D.doubleJumpEnable = true;
                Destroy(powerUp);
                break;
            case "DashPowerUp":
                audio.GetComponentsInChildren<AudioSource>()[2].Play();
                CharacterController2D.dashEnable = true;
                Destroy(powerUp);
                break;
            case "WallSlidePowerUp":
                audio.GetComponentsInChildren<AudioSource>()[2].Play();
                CharacterController2D.wallSlideEnable = true;
                Destroy(powerUp);
                break;
            default:
                break;
        }
    }
}
