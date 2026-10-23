using UnityEngine;

public class MenuParallax : MonoBehaviour
{
    private float length, startPos;
    public float speed; // Velocidade do movimento (ex: 0.1, 0.5)

    void Start()
    {
        startPos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void FixedUpdate()
    {
        // Módulo garante que o movimento sempre fica entre 0 e length, sem deriva
        float looped = (Time.time * speed) % length;
        transform.position = new Vector3(startPos + looped, transform.position.y, transform.position.z);
    }
}