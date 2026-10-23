using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizEditorManager : MonoBehaviour
{
    [Header("Referência ao QuizData (ScriptableObject)")]
    [SerializeField] private QuizData quizData;

    [Header("Container onde ficam as linhas de perguntas (objeto vazio, sem filhos)")]
    [SerializeField] private Transform questionsContainer;

    [Header("Painel de edição (EditPanelRight)")]
    [SerializeField] private GameObject editPanel;
    [SerializeField] private TMP_InputField questionInputField;
    [SerializeField] private TMP_InputField[] answerInputFields;
    [SerializeField] private Toggle[] correctToggles;
    [SerializeField] private Button saveEditButton;
    [SerializeField] private Button cancelEditButton;

    [Header("Botões gerais")]
    [SerializeField] private Button addNewButton;
    [SerializeField] private Button saveAllButton;
    [SerializeField] private Button clearFieldsButton;

    [Header("Máximo de perguntas permitidas")]
    [SerializeField] private int maxQuestions = 8;

    [Header("Sprites dos itens da lista")]
    [SerializeField] private Sprite rowBackgroundSprite;
    [SerializeField] private Sprite pencilSprite;

    [Header("Estilo dos campos de texto")]
    [SerializeField] private TMP_FontAsset vt323Font;

    [System.Serializable]
    public class QuestionEntry
    {
        public string question;
        public string[] answers = new string[4];
        public int correctIndex;
    }

    [System.Serializable]
    private class QuizSaveData
    {
        public List<QuestionEntry> entries = new List<QuestionEntry>();
    }

    private List<QuestionEntry> _entries = new List<QuestionEntry>();
    private int _editingIndex = -1;
    private int _selectedToggleIndex = -1;
    private RectTransform _contentRT;

    private static string SavePath =>
        Path.Combine(Application.persistentDataPath, "quiz_questions.json");

    private void Awake()
    {
        ConfigureToggles();
        BuildScrollRect();
        SetEditPanelVisible(false);
    }

    private void Start()
    {
        saveEditButton.onClick.AddListener(SaveCurrentEdit);
        cancelEditButton.onClick.AddListener(CancelEdit);
        addNewButton.onClick.AddListener(AddNewQuestion);
        saveAllButton.onClick.AddListener(SaveAllToFile);
        clearFieldsButton.onClick.AddListener(ClearEditPanel);

        // Aplica fonte aqui — os campos estão ativos pois editPanel ainda não foi desativado neste frame
        if (vt323Font != null)
        {
            ApplyInputFieldStyle(questionInputField);
            foreach (var f in answerInputFields)
                ApplyInputFieldStyle(f);
        }
    }

    private void ApplyInputFieldStyle(TMP_InputField field)
    {
        field.lineType = TMP_InputField.LineType.MultiLineNewline;
        var textComp = field.textComponent;
        textComp.fontSize = 40;
        textComp.color = Color.white;
        textComp.fontStyle = FontStyles.Normal;
        textComp.textWrappingMode = TextWrappingModes.Normal;
        textComp.overflowMode = TextOverflowModes.Overflow;
        textComp.font = vt323Font;

        if (field.placeholder != null)
        {
            var ph = field.placeholder.GetComponent<TMP_Text>();
            if (ph != null)
            {
                ph.fontSize = 40;
                ph.color = new Color(1f, 1f, 1f, 0.5f);
                ph.textWrappingMode = TextWrappingModes.Normal;
                ph.font = vt323Font;
            }
        }
    }

    // OnEnable: só recarrega dados e lista — sem abrir painel, sem SetActive
    private void OnEnable()
    {
        ClearToggles();
        LoadFromFile();
        RefreshList();
        SetEditPanelVisible(false);
    }

    private void OnDisable()
    {
        SetEditPanelVisible(false);
        ClearToggles();
    }

    private void ConfigureToggles()
    {
        var tg = editPanel.GetComponent<ToggleGroup>();
        if (tg != null) Destroy(tg);

        for (int i = 0; i < correctToggles.Length; i++)
        {
            var t = correctToggles[i];
            t.group = null;
            t.transition = Selectable.Transition.None;
            t.SetIsOnWithoutNotify(false);
            t.onValueChanged.RemoveAllListeners();
            int idx = i;
            t.onValueChanged.AddListener((isOn) =>
            {
                if (isOn) SelectToggle(idx);
                else if (_selectedToggleIndex == idx)
                    correctToggles[idx].SetIsOnWithoutNotify(true);
            });
        }
    }

    private void SelectToggle(int idx)
    {
        _selectedToggleIndex = idx;
        for (int i = 0; i < correctToggles.Length; i++)
            correctToggles[i].SetIsOnWithoutNotify(i == idx);
    }

    private void ClearToggles()
    {
        _selectedToggleIndex = -1;
        foreach (var t in correctToggles)
            t.SetIsOnWithoutNotify(false);
    }

    // Mostra/esconde o painel de edição. InputAnswer e BolaVazia são filhos
    // do editPanel, então SetActive no pai já os controla automaticamente.
    private void SetEditPanelVisible(bool visible)
    {
        editPanel.SetActive(visible);
    }

    private void BuildScrollRect()
    {
        int uiLayer = LayerMask.NameToLayer("UI");

        for (int i = questionsContainer.childCount - 1; i >= 0; i--)
            Destroy(questionsContainer.GetChild(i).gameObject);

        var oldSR = questionsContainer.GetComponent<ScrollRect>();
        if (oldSR != null) Destroy(oldSR);

        var containerImg = questionsContainer.GetComponent<Image>();
        if (containerImg == null) containerImg = questionsContainer.gameObject.AddComponent<Image>();
        containerImg.color = new Color(0, 0, 0, 0.35f);
        questionsContainer.gameObject.layer = uiLayer;

        var viewportGO = new GameObject("Viewport");
        viewportGO.layer = uiLayer;
        viewportGO.transform.SetParent(questionsContainer, false);
        var viewportRT = viewportGO.AddComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.offsetMin = new Vector2(0, 8);
        viewportRT.offsetMax = new Vector2(0, -8);
        var viewportImg = viewportGO.AddComponent<Image>();
        viewportImg.color = new Color(0, 0, 0, 0.01f);
        var mask = viewportGO.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        MakeLine("TopLine", questionsContainer, uiLayer,
            new Vector2(0.05f, 1), new Vector2(0.95f, 1), new Vector2(0.5f, 1f),
            new Vector2(0, 2f), new Vector2(0, -4f));
        MakeLine("BotLine", questionsContainer, uiLayer,
            new Vector2(0.05f, 0), new Vector2(0.95f, 0), new Vector2(0.5f, 0f),
            new Vector2(0, 2f), new Vector2(0, 4f));

        var contentGO = new GameObject("Content");
        contentGO.layer = uiLayer;
        contentGO.transform.SetParent(viewportGO.transform, false);
        _contentRT = contentGO.AddComponent<RectTransform>();
        _contentRT.anchorMin = new Vector2(0, 1);
        _contentRT.anchorMax = new Vector2(1, 1);
        _contentRT.pivot = new Vector2(0.5f, 1f);
        _contentRT.anchoredPosition = Vector2.zero;
        _contentRT.sizeDelta = Vector2.zero;

        var scrollbarGO = new GameObject("Scrollbar");
        scrollbarGO.layer = uiLayer;
        scrollbarGO.transform.SetParent(questionsContainer, false);
        scrollbarGO.AddComponent<Image>().color = new Color(1, 1, 1, 0.1f);
        var sbRT = scrollbarGO.GetComponent<RectTransform>();
        sbRT.anchorMin = new Vector2(1, 0); sbRT.anchorMax = new Vector2(1, 1);
        sbRT.pivot = new Vector2(1, 0.5f);
        sbRT.sizeDelta = new Vector2(10, -16f);
        sbRT.anchoredPosition = new Vector2(-2, 0);

        var haGO = new GameObject("HandleArea");
        haGO.layer = uiLayer;
        haGO.transform.SetParent(scrollbarGO.transform, false);
        var haRT = haGO.AddComponent<RectTransform>();
        haRT.anchorMin = Vector2.zero; haRT.anchorMax = Vector2.one;
        haRT.sizeDelta = Vector2.zero;

        var hGO = new GameObject("Handle");
        hGO.layer = uiLayer;
        hGO.transform.SetParent(haGO.transform, false);
        hGO.AddComponent<Image>().color = new Color(1, 1, 1, 0.7f);
        var hRT = hGO.GetComponent<RectTransform>();
        hRT.anchorMin = Vector2.zero; hRT.anchorMax = Vector2.one;
        hRT.sizeDelta = Vector2.zero;

        var sb = scrollbarGO.AddComponent<Scrollbar>();
        sb.handleRect = hRT;
        sb.direction = Scrollbar.Direction.BottomToTop;

        var sr = questionsContainer.gameObject.AddComponent<ScrollRect>();
        sr.viewport = viewportRT;
        sr.content = _contentRT;
        sr.verticalScrollbar = sb;
        sr.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        sr.verticalScrollbarSpacing = -3f;
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Clamped;
        sr.scrollSensitivity = 30f;
        sr.inertia = true;
        sr.decelerationRate = 0.135f;
    }

    private void MakeLine(string name, Transform parent, int layer,
        Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 size, Vector2 pos)
    {
        var go = new GameObject(name);
        go.layer = layer;
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = new Color(1, 1, 1, 0.3f);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = pivot;
        rt.sizeDelta = size; rt.anchoredPosition = pos;
    }

    private void LoadFromFile()
    {
        _entries.Clear();
        if (File.Exists(SavePath))
        {
            var saveData = JsonUtility.FromJson<QuizSaveData>(File.ReadAllText(SavePath));
            if (saveData?.entries != null)
            {
                _entries = saveData.entries;
                Debug.Log($"[QuizEditor] {_entries.Count} entradas carregadas do JSON.");
            }
        }
        else if (quizData != null)
        {
            foreach (var q in quizData.questions)
            {
                var e = new QuestionEntry
                {
                    question = q.GetQuestion(),
                    answers = new string[4],
                    correctIndex = q.GetCorrectAnswerIndex()
                };
                for (int i = 0; i < 4; i++) e.answers[i] = q.GetAnswer(i);
                _entries.Add(e);
            }
        }
        else Debug.LogError("[QuizEditor] Nenhuma fonte de dados disponível.");
    }

    public void SaveAllToFile()
    {
        File.WriteAllText(SavePath, JsonUtility.ToJson(new QuizSaveData { entries = _entries }, true));
        Debug.Log($"[QuizEditor] Salvo em: {SavePath}");
    }

    public static void ApplySavedDataToQuizData(QuizData target)
    {
        if (!File.Exists(SavePath)) return;
        var saveData = JsonUtility.FromJson<QuizSaveData>(File.ReadAllText(SavePath));
        if (saveData?.entries == null) return;
    }

    private void RefreshList()
    {
        if (_contentRT == null) return;

        int uiLayer = LayerMask.NameToLayer("UI");
        for (int i = _contentRT.childCount - 1; i >= 0; i--)
            Destroy(_contentRT.GetChild(i).gameObject);

        float itemH = 100f, spacing = 8f;
        _contentRT.sizeDelta = new Vector2(0, _entries.Count * (itemH + spacing));

        for (int i = 0; i < _entries.Count; i++)
        {
            int index = i;

            var row = new GameObject($"Row_{i}", typeof(RectTransform));
            row.layer = uiLayer;
            row.transform.SetParent(_contentRT, false);

            var bgImg = row.AddComponent<Image>();
            if (rowBackgroundSprite != null)
            { bgImg.sprite = rowBackgroundSprite; bgImg.type = Image.Type.Sliced; bgImg.color = Color.white; }
            else bgImg.color = new Color(0.3f, 0.3f, 0.5f, 1f);

            var btn = row.AddComponent<Button>();
            btn.targetGraphic = bgImg;
            var cb = btn.colors;
            cb.highlightedColor = new Color(0.8f, 0.8f, 1f, 1f);
            cb.pressedColor = new Color(0.6f, 0.6f, 0.9f, 1f);
            btn.colors = cb;
            btn.onClick.AddListener(() => OpenEditPanel(index));

            var rt = row.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(-16f, itemH);
            rt.anchoredPosition = new Vector2(0, -i * (itemH + spacing) - spacing);

            var textObj = new GameObject("Label", typeof(RectTransform));
            textObj.layer = uiLayer;
            textObj.transform.SetParent(row.transform, false);
            var label = textObj.AddComponent<TextMeshProUGUI>();
            label.text = $"{index + 1}. {Truncate(_entries[i].question, 50)}";
            label.fontSize = 26; label.color = Color.white;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            if (vt323Font != null) label.font = vt323Font;
            var lRT = textObj.GetComponent<RectTransform>();
            lRT.anchorMin = Vector2.zero; lRT.anchorMax = Vector2.one;
            lRT.offsetMin = new Vector2(20, 8); lRT.offsetMax = new Vector2(-80, -8);

            if (pencilSprite != null)
            {
                var pObj = new GameObject("Pencil", typeof(RectTransform));
                pObj.layer = uiLayer;
                pObj.transform.SetParent(row.transform, false);
                var pImg = pObj.AddComponent<Image>();
                pImg.sprite = pencilSprite; pImg.preserveAspect = true;
                var pRT = pObj.GetComponent<RectTransform>();
                pRT.anchorMin = new Vector2(1, 0.5f); pRT.anchorMax = new Vector2(1, 0.5f);
                pRT.pivot = new Vector2(1, 0.5f);
                pRT.sizeDelta = new Vector2(55, 55);
                pRT.anchoredPosition = new Vector2(-12, 0);
            }
        }
    }

    public void OpenEditPanel(int index)
    {
        _editingIndex = index;
        var entry = _entries[index];
        questionInputField.text = entry.question;
        for (int i = 0; i < 4; i++)
            answerInputFields[i].text = entry.answers[i];

        _selectedToggleIndex = entry.correctIndex;
        for (int i = 0; i < correctToggles.Length; i++)
            correctToggles[i].SetIsOnWithoutNotify(i == entry.correctIndex);

        SetEditPanelVisible(true);
    }

    public void AddNewQuestion()
    {
        if (_entries.Count >= maxQuestions)
        { Debug.LogWarning($"[QuizEditor] Limite de {maxQuestions} atingido."); return; }
        _editingIndex = -1;
        ClearEditPanel();
        SetEditPanelVisible(true);
    }

    public void SaveCurrentEdit()
    {
        if (string.IsNullOrWhiteSpace(questionInputField.text))
        { Debug.LogWarning("[QuizEditor] Pergunta vazia."); return; }
        if (_selectedToggleIndex < 0)
        { Debug.LogWarning("[QuizEditor] Selecione a alternativa correta."); return; }

        var entry = new QuestionEntry
        {
            question = questionInputField.text.Trim(),
            answers = new string[4],
            correctIndex = _selectedToggleIndex
        };
        for (int i = 0; i < 4; i++)
            entry.answers[i] = answerInputFields[i].text.Trim();

        if (_editingIndex >= 0 && _editingIndex < _entries.Count)
            _entries[_editingIndex] = entry;
        else
            _entries.Add(entry);

        SetEditPanelVisible(false);
    }

    public void CancelEdit()
    {
        SetEditPanelVisible(false);
        ClearToggles();
        ClearEditPanel();
    }

    public void ClearEditPanel()
    {
        questionInputField.text = "";
        foreach (var f in answerInputFields) f.text = "";
        ClearToggles();
    }

    private string Truncate(string text, int max)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text.Length > max ? text[..max] + "..." : text;
    }
}