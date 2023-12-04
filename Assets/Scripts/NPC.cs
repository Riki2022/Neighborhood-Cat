using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public Dialogue dialogue;
    public GameObject indicator;

    public static bool isOpened = false;

    private bool isActive = false;

    private void Update()
    {
        if (!isOpened)
        {
            if (CharacterController2D.playerInput.actions["Move"].ReadValue<Vector2>().y > 0 && isActive)
            {
                isOpened = true;
                FindObjectOfType<DialogueManager>().StartDialogue(dialogue);
            }
        }
        else
        {
            if (CharacterController2D.playerInput.actions["Jump"].WasPressedThisFrame() && isActive)
            {
                FindObjectOfType<DialogueManager>().DisplayNextSentence();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        indicator.SetActive(true);
        isActive = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        indicator.SetActive(false);
        isActive = false;
    }
}
