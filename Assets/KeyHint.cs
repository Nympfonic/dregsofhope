using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyHint : MonoBehaviour
{
    [SerializeField] private bool isKeyCombination;
    [SerializeField] private KeyCode key;
    [SerializeField] private KeyCode key2;

    private void Update()
    {
        if (KeyInput() && !isKeyCombination)
        {
            Destroy(gameObject);
        }
        else if (KeyInput() && Key2Input() && isKeyCombination)
        {
            Destroy(gameObject);
        }
    }

    private bool KeyInput()
    {
        return Input.GetKey(key);
    }

    private bool Key2Input()
    {
        return Input.GetKey(key2);
    }
}
