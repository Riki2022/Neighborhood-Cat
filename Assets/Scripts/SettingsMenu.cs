using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public TMP_Text keyBindText;
    public TMP_Text graphicText;
    public TMP_Text generalText;
    public Button keyBindButton;
    public Button graphicButton;
    public Button generalButton;
    private void Awake()
    {
        keyBindText.color = new Color32(0, 0, 0, 255);
        keyBindButton.image.color = new Color32(0, 0, 0, 0);
    }
    public void OnKeyBindSelect()
    {
        keyBindText.color = new Color32(0, 0, 0, 255);
        graphicText.color = new Color32(255, 255, 255, 255);
        generalText.color = new Color32(255, 255, 255, 255);
        keyBindButton.image.color = new Color32(0, 0, 0, 0);
        graphicButton.image.color = new Color32(0, 0, 0, 255);
        generalButton.image.color = new Color32(0, 0, 0, 255);
        keyBindButton.enabled = false;
        graphicButton.enabled = true;
        generalButton.enabled = true;

    }
    public void OnGraphicSelect()
    {
        keyBindText.color = new Color32(255, 255, 255, 255);
        graphicText.color = new Color32(0, 0, 0, 255);
        generalText.color = new Color32(255, 255, 255, 255);
        keyBindButton.image.color = new Color32(0, 0, 0, 255);
        graphicButton.image.color = new Color32(0, 0, 0, 0);
        generalButton.image.color = new Color32(0, 0, 0, 255);
        keyBindButton.enabled = true;
        graphicButton.enabled = false;
        generalButton.enabled = true;
    }
    public void OnAudioSelect()
    {
        keyBindText.color = new Color32(255, 255, 255, 255);
        graphicText.color = new Color32(255, 255, 255, 255);
        generalText.color = new Color32(0, 0, 0, 255);
        keyBindButton.image.color = new Color32(0, 0, 0, 255);
        graphicButton.image.color = new Color32(0, 0, 0, 255);
        generalButton.image.color = new Color32(0, 0, 0, 0);
        keyBindButton.enabled = true;
        graphicButton.enabled = true;
        generalButton.enabled = false;
    }
}
