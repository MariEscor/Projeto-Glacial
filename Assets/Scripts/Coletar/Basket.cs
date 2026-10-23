using UnityEngine;

public class Basket : MonoBehaviour
{
    [SerializeField] float speed = 0.2f;

    [Header("Offsets das bordas (valores positivos afastam do limite)")]
    [SerializeField] float leftOffset = 2.5f;
    [SerializeField] float rightOffset = 4.5f;

    float limitX;
    Transform tr;

    void Start()
    {
        tr = transform;
        limitX = Camera.main.orthographicSize * Camera.main.aspect;
    }

    void FixedUpdate()
    {
        bool movingRight = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
        bool movingLeft = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);

        if (movingRight && tr.position.x < limitX - rightOffset)
            tr.position += new Vector3(speed, 0f, 0f);

        if (movingLeft && tr.position.x > -limitX + leftOffset)
            tr.position += new Vector3(-speed, 0f, 0f);
    }
}