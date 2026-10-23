using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Adicione este script a um GameObject vazio chamado "PopupSetup" na sua cena
/// para gerar automaticamente o popup de confirmação caso prefira não criar manualmente.
/// 
/// ALTERNATIVA: Crie o popup manualmente no Unity seguindo o guia README abaixo.
/// </summary>

/*
 * ═══════════════════════════════════════════════════════════
 *  README — COMO CONFIGURAR O POPUP MANUALMENTE NO UNITY
 * ═══════════════════════════════════════════════════════════
 *
 * 1. ESTRUTURA DE HIERARQUIA NECESSÁRIA NO CANVAS:
 *
 *    Canvas
 *    ├── Background
 *    ├── BackButton          ← seu botão já existente
 *    ├── TransitionPanel     ← NOVO: painel preto para fade
 *    └── ConfirmPopup        ← NOVO: popup de confirmação
 *        ├── Overlay         ← fundo escuro semi-transparente
 *        ├── PopupPanel      ← painel branco/colorido central
 *        │   ├── TitleText   ← "Sair da fase?"
 *        │   ├── MessageText ← "Tem certeza que deseja sair?"
 *        │   ├── YesButton   ← "Sim"
 *        │   └── NoButton    ← "Não"
 *
 * ───────────────────────────────────────────────────────────
 * 2. CONFIGURAR "TransitionPanel":
 *    - Adicionar Image (cor preta, #000000)
 *    - Anchor: Stretch-Stretch (cobre a tela inteira)
 *    - Adicionar CanvasGroup (ou o script faz isso automaticamente)
 *    - Sort Order: deve ficar NA FRENTE de tudo (mover para baixo na hierarquia do Canvas)
 *
 * ───────────────────────────────────────────────────────────
 * 3. CONFIGURAR "ConfirmPopup":
 *    - Anchor: Stretch-Stretch
 *    - Adicionar CanvasGroup (ou o script faz automaticamente)
 *    
 *    > "Overlay" (filho do ConfirmPopup):
 *      - Image: preto com Alpha ~150 (semi-transparente)
 *      - Anchor: Stretch-Stretch
 *
 *    > "PopupPanel" (filho do ConfirmPopup):
 *      - Image: cor do seu tema (ex: azul #1A2B5A com borda)
 *      - Anchor: Center-Center
 *      - Width: 500, Height: 280 (ajuste ao gosto)
 *      - Adicionar um "Outline" ou "Shadow" para destaque
 *
 *    > "TitleText":
 *      - TextMeshPro
 *      - Texto: "Sair da fase?"
 *      - Font Size: 32, Bold
 *
 *    > "MessageText":
 *      - TextMeshPro
 *      - Texto: "Tem certeza que deseja\nabandonar a fase atual?"
 *      - Font Size: 22
 *
 *    > "YesButton" e "NoButton":
 *      - Botões normais do Unity UI
 *      - Textos: "Sim" / "Não"
 *
 * ───────────────────────────────────────────────────────────
 * 4. CONECTAR O SCRIPT "BackButtonController":
 *    - Adicione BackButtonController.cs ao GameObject "BackButton"
 *    - No Inspector, arraste:
 *        • ConfirmPopup  → campo "Confirm Popup"
 *        • YesButton     → campo "Yes Button"
 *        • NoButton      → campo "No Button"
 *        • TransitionPanel → campo "Transition Panel"
 *    - Verifique se "Target Scene" está como "MainScene"
 *    - No BackButton, no componente Button > OnClick(), adicione:
 *        BackButtonController.OnBackButtonClicked()
 *
 * ───────────────────────────────────────────────────────────
 * 5. BUILD SETTINGS:
 *    - Vá em File > Build Settings
 *    - Garanta que "MainScene" está na lista de cenas (index 0 idealmente)
 *
 * ═══════════════════════════════════════════════════════════
 */

public class PopupSetupGuide : MonoBehaviour
{
    // Este script é apenas documentação.
    // Você pode deletá-lo após configurar o popup manualmente.
    
    private void Awake()
    {
        Debug.Log("[BackButton] Leia o PopupSetupGuide.cs para instruções de configuração!");
        Destroy(this);
    }
}
