using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speed = 0.5f;
    private Rigidbody2D rb;
    private Vector2 input;

    Animator anim;
    private Vector2 lastMoveDiretion;
    private bool facingfLeft = true;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        ProcessInputs();
        Animate();
    }

    private void FixedUpdate()
    {
        // ✅ Para o jogador completamente durante diálogo ou qualquer pausa
        if (PauseController.IsGamePaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = input * speed;
    }

    void ProcessInputs()
    {
        // Não processa input durante pausa
        if (PauseController.IsGamePaused)
        {
            input = Vector2.zero;
            return;
        }

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        if ((moveX == 0 && moveY == 0) && (input.x != 0 || input.y != 0))
            lastMoveDiretion = input;

        input.x = moveX;
        input.y = moveY;

        // Bloqueia diagonal
        if (input.x != 0)
            input.y = 0;
    }

    void Animate()
    {
        anim.SetFloat("MoveX", input.x);
        anim.SetFloat("MoveY", input.y);
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        facingfLeft = !facingfLeft;
    }
}