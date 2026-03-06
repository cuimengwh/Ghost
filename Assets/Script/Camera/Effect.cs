using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    public Shader shader;
    public Color color;
    public float slider;
    private void OnEnable()
    {
        GetComponent<Camera>().SetReplacementShader(shader, "");
    }
}
