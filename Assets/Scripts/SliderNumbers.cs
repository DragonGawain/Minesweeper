using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderNumbers : MonoBehaviour
{
    [SerializeField]
    Slider slider;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update()
    {
        GetComponent<TextMeshProUGUI>().text = "" + slider.value;
    }
}
