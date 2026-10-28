using System.Collections;
using UnityEngine;
using TMPro;

public class UITextManager : MonoBehaviour
{
    public static UITextManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private TextMeshProUGUI controlsText;
    [SerializeField] private float typeInterval = 0.035f;
    [SerializeField] private float fadeDuration = 0.35f;

    private Coroutine _routine;

    private void Awake()
    {
        Instance = this;

        if (hintText != null)
        {
            hintText.text = string.Empty;
            SetAlpha(hintText, 0f);
        }

        RefreshControls();
    }

    private void OnEnable()
    {
        Loc.LanguageChanged += RefreshControls;
    }

    private void OnDisable()
    {
        Loc.LanguageChanged -= RefreshControls;
    }

    public void ShowText(string content, float holdSeconds = 3f, bool sticky = false)
    {
        if (hintText == null)
        {
            return;
        }

        if (_routine != null)
        {
            StopCoroutine(_routine);
        }

        _routine = StartCoroutine(TypeRoutine(content, holdSeconds, sticky));
    }

    public void HideText()
    {
        if (hintText == null)
        {
            return;
        }

        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        StartCoroutine(FadeOutRoutine());
    }

    public void SetHintAlpha(float alpha)
    {
        if (hintText != null)
        {
            SetAlpha(hintText, alpha);
        }
    }

    private IEnumerator TypeRoutine(string content, float holdSeconds, bool sticky)
    {
        hintText.text = content;
        hintText.maxVisibleCharacters = 0;
        SetAlpha(hintText, 0f);

        yield return FadeRoutine(hintText, 0f, 1f);

        int total = content.Length;

        for (int i = 0; i <= total; i++)
        {
            hintText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typeInterval);
        }

        if (sticky)
        {
            _routine = null;
            yield break;
        }

        yield return new WaitForSeconds(holdSeconds);
        yield return FadeRoutine(hintText, 1f, 0f);

        hintText.text = string.Empty;
        _routine = null;
    }

    private IEnumerator FadeOutRoutine()
    {
        float start = hintText.color.a;
        yield return FadeRoutine(hintText, start, 0f);
        hintText.text = string.Empty;
    }

    private IEnumerator FadeRoutine(TextMeshProUGUI text, float from, float to)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(text, Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / fadeDuration)));
            yield return null;
        }

        SetAlpha(text, to);
    }

    private void RefreshControls()
    {
        if (controlsText == null)
        {
            return;
        }

        controlsText.text = Loc.Language == "en"
            ? "1 Courage   2 Wisdom   3 Patience   4 Hope"
            : "1 勇气   2 智慧   3 耐心   4 希望";
    }

    private static void SetAlpha(TextMeshProUGUI text, float alpha)
    {
        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }
}
