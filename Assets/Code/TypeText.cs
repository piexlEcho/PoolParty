using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TypeText : MonoBehaviour
{
    public string TextToShow;
    public float bScale = 1f;
    TextMesh textMesh;
    float timePassed = 0f;
    public Vector2 normalScale = new Vector2(0.12f, 0.12f);


    private void Awake()
    {
        textMesh = GetComponent<TextMesh>();
        normalScale = transform.localScale;
    }

    void Start()
    {
        textMesh.text = TextToShow;
    }
    
    void Update()
    {
        if (timePassed < 0.1f)
        {
            timePassed += Time.deltaTime;
            float t = timePassed / 0.1f;
            transform.localScale = normalScale * Vector2.Lerp(new Vector2(bScale, bScale), new Vector2(bScale * 1.2f, bScale * 1.2f), t);
        }
        else if (timePassed < 0.4f)
        {
            timePassed += Time.deltaTime;
            float t = (timePassed - 0.1f) / 0.4f;
            transform.localScale = normalScale * Vector2.Lerp(new Vector2(bScale * 1.2f, bScale * 1.2f), new Vector2(0, 0), t);
            textMesh.color = Color.Lerp(Color.white, new Color(0, 0, 0, 0), t);
        }
        else Destroy(gameObject);
    }
}
