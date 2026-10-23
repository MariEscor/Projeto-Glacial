using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    public Image[] tabImages;
    public GameObject[] pages;

    [Tooltip("Referência ao StarsPageDisplay para atualizar ao abrir a aba de estrelas")]
    public StarsPageDisplay starsPageDisplay;

    [Tooltip("Índice da aba de estrelas no array pages (normalmente 1)")]
    public int starsTabIndex = 1;

    void Start()
    {
        ActivateTab(0);
    }

    public void ActivateTab(int tabNo)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
            tabImages[i].color = Color.grey;
        }

        pages[tabNo].SetActive(true);
        tabImages[tabNo].color = Color.white;

        // Atualiza as estrelas ao abrir a aba correta,
        // mas só depois que o SaveSystem estiver pronto
        if (tabNo == starsTabIndex && starsPageDisplay != null)
            StartCoroutine(RefreshStarsNextFrame());
    }

    private IEnumerator RefreshStarsNextFrame()
    {
        yield return null; // espera um frame para garantir que SaveSystem.Instance existe
        starsPageDisplay.Refresh();
    }
}