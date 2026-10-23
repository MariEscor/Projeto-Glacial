using UnityEngine;

public class Generator : MonoBehaviour
{
    [SerializeField] float interval = 0.7f;
    [SerializeField] int badObjectChance = 20;
    [SerializeField] float leftOffset = 2.5f;
    public GameObject[] gm;

    [Header("Área de spawn (ajuste no Inspector)")]
    [Tooltip("Limite esquerdo da área clara (em unidades do mundo)")]
    [SerializeField] float spawnXMin = -2f;
    [Tooltip("Limite direito da área clara (em unidades do mundo)")]
    [SerializeField] float spawnXMax = 5f;

    float timer = 1f;
    float spawnY;

    void Start()
    {
        spawnY = Camera.main.orthographicSize + 1f;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            float pos_x = Random.Range(spawnXMin, spawnXMax);
            int chance = Random.Range(1, 101);
            int index;

            // Se o sorteio cair na chance de objeto ruim
            if (chance <= badObjectChance)
            {
                // Sorteia entre os �ndices 2 e 3 (os itens ruins)
                // O Random.Range para inteiros exclui o valor m�ximo, por isso usamos 4
                index = Random.Range(2, 4);
            }
            else
            {
                // Sorteia entre os �ndices 0 e 1 (os itens bons)
                index = Random.Range(0, 2);
            }

            Instantiate(gm[index], new Vector3(pos_x, spawnY, 0.1f), Quaternion.identity);

            timer = interval;
        }
    }
}