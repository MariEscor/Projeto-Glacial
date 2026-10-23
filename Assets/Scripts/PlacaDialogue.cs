using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Adicione este script em cada Placa, junto com o PathBlocker.
/// Quando o jogador se aproxima, abre o painel de diálogo com a mensagem da placa.
/// Fecha automaticamente quando o jogador se afasta, ou ao pressionar E.
/// 
/// No Inspector configure:
///   - signMessage: texto que aparece no balão
///   - signName: nome exibido no campo de nome (ex: "Placa", "Aviso")
///   - signPortrait: sprite da placa/ícone (opcional)
/// </summary>
public class PlacaDialogue : MonoBehaviour
{
    [Header("Conteúdo")]
    [TextArea(2, 5)]
    public string signMessage = "Para passar por aqui, você precisa completar o desafio do pinguim!";

    [Tooltip("Nome exibido no painel de diálogo")]
    public string signName = "Placa";

    [Tooltip("Ícone/portrait da placa (opcional)")]
    public Sprite signPortrait;

    [Header("Detecção")]
    [Tooltip("Raio de detecção do jogador")]
    public float detectionRadius = 1.5f;

    private bool _playerInRange = false;
    private bool _isShowing = false;
    private Transform _player;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;
    }

    private void Update()
    {
        if (_player == null) return;

        float dist = Vector2.Distance(transform.position, _player.position);
        bool inRange = dist <= detectionRadius;

        // Jogador entrou no raio
        if (inRange && !_playerInRange)
        {
            _playerInRange = true;
            ShowMessage();
        }

        // Jogador saiu do raio
        if (!inRange && _playerInRange)
        {
            _playerInRange = false;
            HideMessage();
        }

        // Fecha ao pressionar E enquanto estiver mostrando
        if (_isShowing && Input.GetKeyDown(KeyCode.E))
        {
            HideMessage();
        }
    }

    private void ShowMessage()
    {
        if (DialogueController.Instance == null) return;

        _isShowing = true;
        DialogueController.Instance.SetNPCInfo(signName, signPortrait);
        DialogueController.Instance.SetDialogueText(signMessage);
        DialogueController.Instance.ClearChoices();
        DialogueController.Instance.ShowDialogueUI(true);
    }

    private void HideMessage()
    {
        if (!_isShowing) return;
        _isShowing = false;

        if (DialogueController.Instance != null)
            DialogueController.Instance.ShowDialogueUI(false);
    }

    // Desenha o raio de detecção no editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}