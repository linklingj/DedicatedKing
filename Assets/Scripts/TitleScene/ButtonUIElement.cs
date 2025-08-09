using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;

[HideMonoScript]
[Title("Button UI Element", "Customizable button with highlight animation", TitleAlignments.Centered)]
public class ButtonUIElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [BoxGroup("References")]
    [LabelText("Button")]
    [SerializeField] private Button button;

    [BoxGroup("References")]
    [LabelText("Button Text")]
    [SerializeField] private TMP_Text buttonText;
    
    [BoxGroup("Appearance")]
    [LabelText("Highlight Scale")]
    [SerializeField] private float highlightScale = 1.3f;

    [BoxGroup("Animation")]
    [LabelText("Duration")]
    [SerializeField] private float duration = 0.25f;

    [BoxGroup("Animation")]
    [LabelText("Ease")]
    [SerializeField] private Ease ease = Ease.OutQuad;

    [BoxGroup("Appearance")]
    [LabelText("Normal Text Color")]
    [SerializeField] [ColorPalette] private Color normalTextColor = Color.black;

    [BoxGroup("Appearance")]
    [LabelText("Highlight Text Color")]
    [SerializeField] [ColorPalette] private Color highlightTextColor = Color.yellow;

    [ReadOnly]
    [ShowInInspector]
    private Vector3 initialScale;

    private void Awake()
    {
        initialScale = transform.localScale;
        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TMP_Text>();
    }

    private void OnEnable()
    {
        AnimateHighlight(false);
    }

    // highlight 애니메이션을 처리하는 메서드
    // on이 true면 강조 표시, false면 원래 상태로 돌아감
    private void AnimateHighlight(bool on)
    {
        if (buttonText != null)
        {
            buttonText.DOKill();
            buttonText.DOColor(on ? highlightTextColor : normalTextColor, duration).SetEase(ease);
        }
        transform.DOKill();
        transform.DOScale(on ? Vector3.one * highlightScale : initialScale, duration).SetEase(ease);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        AnimateHighlight(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
        {
            AnimateHighlight(true);
        }
        else
        {
            AnimateHighlight(false);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        AnimateHighlight(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        AnimateHighlight(false);
    }

    public void SetButtonText(string text)
    {
        if (buttonText != null)
        {
            buttonText.text = text;
        }
    }

    public void SetButtonInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    public void AddOnClickListener(UnityAction action)
    {
        if (button != null)
        {
            button.onClick.AddListener(action);
        }
    }
}
