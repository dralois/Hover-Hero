using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditsButtons : MonoBehaviour
{
    public Button BackButton;

    // Start is called before the first frame update
    void Start()
    {
        BackButton.onClick.AddListener(delegate { OnBackButton(); });
    }

    void OnBackButton()
    {
        AudioManager.Instance.Play("Click");
        GameManager.Instance.LoadScene("MainMenu");
    }
    
}
