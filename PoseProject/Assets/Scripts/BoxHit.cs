using System;
using UnityEngine;

public class BoxHit : MonoBehaviour
{
    public GameObject jointObj;
    Color originalColor;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalColor = GetComponent<SpriteRenderer>().color;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = jointObj.transform.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        GetComponent<SpriteRenderer>().color = Color.red;
        Invoke("BackToNormal", 1);
    }

    private void BackToNormal()
    {
        GetComponent<SpriteRenderer>().color = originalColor;
    }
}
