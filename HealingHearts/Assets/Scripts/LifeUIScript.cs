using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeUIScript : MonoBehaviour
{

    Slider slider;
    [SerializeField] float life;
    [SerializeField] bool isPlayer;
    [SerializeField] 
    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();
        if (isPlayer)
        {
            slider.maxValue = FindObjectOfType<PlayerManager>().maxHP;
            slider.value = FindObjectOfType<PlayerManager>().currentHP;
        }
        else
        {
            slider.maxValue = FindObjectOfType<Enemy_Base>().maxHP;
            slider.value = FindObjectOfType<Enemy_Base>().currentHP;
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isPlayer) {
            slider.value = FindObjectOfType<PlayerManager>().currentHP;
        }
        else
        {
            slider.value = FindObjectOfType<Enemy_Base>().currentHP;
        }
    }
}
