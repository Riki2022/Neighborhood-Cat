using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextStage : MonoBehaviour
{
    public GameObject changeScene;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        StartCoroutine(NextScene());        
    }

    IEnumerator NextScene()
    {
        changeScene.GetComponent<Animator>().SetBool("FadeOut",true);
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
