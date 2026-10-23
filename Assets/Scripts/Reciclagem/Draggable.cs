using UnityEngine;

public class Draggable : MonoBehaviour
{
    public bool IsDragging;
    public Vector2 OriginalPosition { get; private set; }

    private Collider2D _collider;
    private DragController _dragController;
    private Vector2 _accumulatedCorrection;

    void Start()
    {
        _collider = GetComponent<Collider2D>();
        _dragController = FindFirstObjectByType<DragController>();
        OriginalPosition = transform.position; // salva posição inicial
    }

    void FixedUpdate()
    {
        if (_accumulatedCorrection != Vector2.zero)
        {
            // Aplica só uma fração da correção por frame, suavizando o movimento
            Vector2 smoothed = Vector2.Lerp(Vector2.zero, _accumulatedCorrection, 0.3f);
            transform.position += (Vector3)smoothed;
            _accumulatedCorrection = Vector2.zero;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        Draggable collidedDraggable = other.GetComponent<Draggable>();

        if (collidedDraggable != null && _dragController.LastDragged.gameObject == gameObject)
        {
            ColliderDistance2D colliderDistance2D = other.Distance(_collider);

            if (colliderDistance2D.distance < 0f)
            {
                Vector2 correction = colliderDistance2D.normal * colliderDistance2D.distance;
                _accumulatedCorrection -= correction;
            }
        }
    }
}