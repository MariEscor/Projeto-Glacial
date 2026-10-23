using UnityEngine;

public class BadObject : MonoBehaviour
{
    [SerializeField] float speed = 0.12f;
    float limitY;
    Transform tr;

    void Start()
    {
        tr = transform;
        limitY = -(Camera.main.orthographicSize + 1f);
    }

    void FixedUpdate()
    {
        tr.position -= new Vector3(0f, speed, 0f);

        if (tr.position.y < limitY)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Basket"))
        {
            SoundManager.Instance?.PlaySound2D("collect_bad");
            GameManager.instance.BadItem();
            Destroy(gameObject); // destr�i s� o carro
                                 // removido o Destroy da cesta
        }
    }


}