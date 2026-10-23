using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Coloque este script no objeto StarsPage.
/// Estrelas iniciam vazias e só preenchem após o SaveSystem estar pronto.
/// </summary>
public class StarsPageDisplay : MonoBehaviour
{
    [System.Serializable]
    public struct MinigameStarRow
    {
        [Tooltip("Igual ao progressKey do LevelCompletePanel do minigame. Ex: cacapalavras")]
        public string progressKey;
        public Image star1;
        public Image star2;
        public Image star3;
    }

    public MinigameStarRow[] rows;

    // Idêntico ao LevelCompletePanel
    private static readonly Color FilledColor = Color.white;
    private static readonly Color EmptyColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    private void Start()
    {
        // Inicia todas as estrelas vazias
        ClearAllStars();
    }

    private void OnEnable()
    {
        // Só atualiza se o SaveSystem já estiver pronto
        // (evita o warning quando o objeto ativa antes do Awake do SaveSystem)
        if (SaveSystem.Instance != null)
            Refresh();
    }

    /// <summary>Lê o SaveSystem e atualiza todas as estrelas na tela.</summary>
    public void Refresh()
    {
        if (SaveSystem.Instance == null) return;

        foreach (var row in rows)
        {
            int stars = SaveSystem.Instance.GetStars(row.progressKey);
            SetStar(row.star1, stars >= 1);
            SetStar(row.star2, stars >= 2);
            SetStar(row.star3, stars >= 3);
        }
    }

    /// <summary>Deixa todas as estrelas vazias (cinza).</summary>
    public void ClearAllStars()
    {
        foreach (var row in rows)
        {
            SetStar(row.star1, false);
            SetStar(row.star2, false);
            SetStar(row.star3, false);
        }
    }

    private void SetStar(Image img, bool filled)
    {
        if (img != null)
            img.color = filled ? FilledColor : EmptyColor;
    }
}