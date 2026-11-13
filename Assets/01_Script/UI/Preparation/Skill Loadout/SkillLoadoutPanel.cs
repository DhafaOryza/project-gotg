using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillLoadoutPanel : MonoBehaviour
{
    [Header("References")]
    [Header("Skill Card")]
    [SerializeField] private Transform skillContainer;
    [SerializeField] private SkillCardUI skillCardPrefab;

    [Header("Skill Slot")]
    [SerializeField] private Transform skillSlotContainer;
    [SerializeField] private SkillSlotUI skillSlotPrefab;

    [Header("Detail Skill")]
    [SerializeField] private GameObject detailContainer;
    [SerializeField] private TextMeshProUGUI txtSkillname;
    [SerializeField] private Image imgSkillIcon;
    [SerializeField] private TextMeshProUGUI txtSkillLevel;
    [SerializeField] private TextMeshProUGUI txtSkillDesc;

    private SkillDataSO selectedSkill;
    private Coroutine focusCardRoutine;
    private Coroutine cardNavigationRoutine;

    private static readonly string[] DefaultSkillKeys = { "Q", "W", "E", "R" };

    private void OnEnable()
    {
        SubscribeToSkillsManager();
        Initialize();
    }

    private void OnDisable()
    {
        if (focusCardRoutine != null)
        {
            StopCoroutine(focusCardRoutine);
            focusCardRoutine = null;
        }

        if (cardNavigationRoutine != null)
        {
            StopCoroutine(cardNavigationRoutine);
            cardNavigationRoutine = null;
        }

        var skillsManager = GameManager.Instance?.skillsManager;
        if (skillsManager != null)
        {
            skillsManager.OnChosenSkillsChanged -= GenerateSkillCard;
            skillsManager.OnChosenSkillsChanged -= GenerateSkillSlot;
        }
    }

    public void Initialize()
    {
        GenerateSkillCard();
        GenerateSkillSlot();
    }

    #region Helper

    private void GenerateSkillCard()
    {
        var session = GameSessionData.GetOrCreate();

        if (session == null || skillContainer == null || skillCardPrefab == null) return;

        ClearContainer(skillContainer);
        var skills = session.skills;
        if (skills == null) return;

        var chosenSkills = GameManager.Instance?.skillsManager?.choosenSkills;

        foreach (var skill in skills)
        {
            if (skill == null || skill.IsLocked) continue;
            if (chosenSkills != null && chosenSkills.Contains(skill)) continue;

            SkillCardUI skillCard = Instantiate(skillCardPrefab, skillContainer);
            skillCard.Initialize(skill);
        }

        RefreshSkillCardNavigation();
        GetComponentInParent<PreparePanel>()?.RefreshNavigation();
    }

    private void RefreshSkillCardNavigation()
    {
        if (cardNavigationRoutine != null)
            StopCoroutine(cardNavigationRoutine);

        cardNavigationRoutine = StartCoroutine(RefreshSkillCardNavigationAfterLayout());
    }

    private IEnumerator RefreshSkillCardNavigationAfterLayout()
    {
        // Grid/layout groups update card positions at the end of the frame.
        yield return null;
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();

        if (skillContainer == null)
        {
            cardNavigationRoutine = null;
            yield break;
        }

        var cards = skillContainer.GetComponentsInChildren<SkillCardUI>(false);
        var selectables = new Selectable[cards.Length];
        var positions = new Vector2[cards.Length];

        for (int i = 0; i < cards.Length; i++)
        {
            selectables[i] = cards[i].GetComponent<Selectable>();
            if (selectables[i] == null)
                selectables[i] = cards[i].GetComponentInChildren<Selectable>(false);

            var rect = selectables[i] != null ? selectables[i].transform as RectTransform : null;
            positions[i] = rect != null ? rect.TransformPoint(rect.rect.center) : cards[i].transform.position;
        }

        for (int i = 0; i < selectables.Length; i++)
        {
            var selectable = selectables[i];
            if (selectable == null) continue;

            Navigation navigation = selectable.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnUp = FindNearestCard(i, selectables, positions, Vector2.up)
                                    ?? navigation.selectOnUp;
            navigation.selectOnDown = FindNearestCard(i, selectables, positions, Vector2.down)
                                      ?? navigation.selectOnDown;
            navigation.selectOnLeft = FindNearestCard(i, selectables, positions, Vector2.left)
                                      ?? navigation.selectOnLeft;
            navigation.selectOnRight = FindNearestCard(i, selectables, positions, Vector2.right)
                                       ?? navigation.selectOnRight;
            selectable.navigation = navigation;
        }

        cardNavigationRoutine = null;
    }

    private static Selectable FindNearestCard(
        int sourceIndex,
        Selectable[] selectables,
        Vector2[] positions,
        Vector2 direction)
    {
        if (selectables[sourceIndex] == null) return null;

        Selectable nearest = null;
        float bestScore = float.PositiveInfinity;
        Vector2 origin = positions[sourceIndex];

        for (int i = 0; i < selectables.Length; i++)
        {
            if (i == sourceIndex || selectables[i] == null || !selectables[i].isActiveAndEnabled)
                continue;

            Vector2 offset = positions[i] - origin;
            float distance = offset.magnitude;
            if (distance <= Mathf.Epsilon) continue;

            float alignment = Vector2.Dot(offset / distance, direction);
            if (alignment <= 0.35f) continue;

            // Prefer candidates directly in the requested direction, then the closest one.
            float score = distance / alignment;
            if (score < bestScore)
            {
                bestScore = score;
                nearest = selectables[i];
            }
        }

        return nearest;
    }

    /// <summary>
    /// Selects the card that shifts into the submitted card's index after rebuilding the list.
    /// </summary>
    public void FocusNextSkillCard(int submittedCardIndex)
    {
        if (focusCardRoutine != null)
            StopCoroutine(focusCardRoutine);

        focusCardRoutine = StartCoroutine(FocusNextSkillCardAfterRefresh(submittedCardIndex));
    }

    private IEnumerator FocusNextSkillCardAfterRefresh(int submittedCardIndex)
    {
        // Allow the manager event and deferred Destroy calls to finish rebuilding the list.
        yield return null;
        yield return null;
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();

        Selectable target = null;
        if (skillContainer != null)
        {
            var cards = skillContainer.GetComponentsInChildren<SkillCardUI>(false);
            if (cards.Length > 0)
            {
                int nextIndex = Mathf.Clamp(submittedCardIndex, 0, cards.Length - 1);
                target = cards[nextIndex].GetComponent<Selectable>();
                if (target == null)
                    target = cards[nextIndex].GetComponentInChildren<Selectable>(false);
            }
        }

        // If the final card was submitted, move focus to the first equipped skill slot.
        if (target == null && skillSlotContainer != null)
        {
            var slotSelectables = skillSlotContainer.GetComponentsInChildren<Selectable>(false);
            if (slotSelectables.Length > 0)
                target = slotSelectables[0];
        }

        SetSelectedObject(target);

        focusCardRoutine = null;
    }

    /// <summary>
    /// Selects the card for a skill returned from a slot after the card list is rebuilt.
    /// </summary>
    public void FocusSkillCard(SkillDataSO skill)
    {
        if (focusCardRoutine != null)
            StopCoroutine(focusCardRoutine);

        focusCardRoutine = StartCoroutine(FocusSkillCardAfterRefresh(skill));
    }

    /// <summary>
    /// Keeps selection on the same slot after its skill is returned to the loadout.
    /// </summary>
    public void FocusSkillSlot(int slotIndex)
    {
        if (focusCardRoutine != null)
            StopCoroutine(focusCardRoutine);

        focusCardRoutine = StartCoroutine(FocusSkillSlotAfterRefresh(slotIndex));
    }

    private IEnumerator FocusSkillSlotAfterRefresh(int slotIndex)
    {
        // The manager event rebuilds the slots and Unity destroys the old slot at frame end.
        yield return null;
        yield return null;
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();

        Selectable target = null;
        if (skillSlotContainer != null)
        {
            var slots = skillSlotContainer.GetComponentsInChildren<SkillSlotUI>(false);
            if (slotIndex >= 0 && slotIndex < slots.Length)
            {
                target = slots[slotIndex].GetComponent<Selectable>();
                if (target == null)
                    target = slots[slotIndex].GetComponentInChildren<Selectable>(false);
            }
        }

        SetSelectedObject(target);
        focusCardRoutine = null;
    }

    private IEnumerator FocusSkillCardAfterRefresh(SkillDataSO skill)
    {
        // Wait for the skills-changed event to rebuild the card list and update its layout.
        yield return null;
        yield return null;
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();

        Selectable target = null;
        if (skillContainer != null)
        {
            var cards = skillContainer.GetComponentsInChildren<SkillCardUI>(false);
            foreach (var card in cards)
            {
                if (card.SkillData != skill) continue;

                target = card.GetComponent<Selectable>();
                if (target == null)
                    target = card.GetComponentInChildren<Selectable>(false);
                break;
            }

            // Fallback if the returned skill is not present in the rebuilt loadout.
            if (target == null && cards.Length > 0)
            {
                target = cards[0].GetComponent<Selectable>();
                if (target == null)
                    target = cards[0].GetComponentInChildren<Selectable>(false);
            }
        }

        SetSelectedObject(target);
        focusCardRoutine = null;
    }

    private static void SetSelectedObject(Selectable target)
    {
        if (target != null && target.isActiveAndEnabled && target.IsInteractable() && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(target.gameObject);
    }

    public void GenerateSkillSlot()
    {
        if (skillSlotContainer == null || skillSlotPrefab == null) return;

        var skillsManager = GameManager.Instance?.skillsManager;
        if (skillsManager == null) return;

        ClearContainer(skillSlotContainer);
        var playerController = GameManager.Instance?.playerController;

        for (int i = 0; i < skillsManager.maxSkills; i++)
        {
            SkillSlotUI skillSlot = Instantiate(skillSlotPrefab, skillSlotContainer);
            string keyBind = i < DefaultSkillKeys.Length
                ? (playerController != null ? playerController.GetSkillDisplayName(i) : DefaultSkillKeys[i])
                : string.Empty;

            skillSlot.Initialize(keyBind, skillsManager.GetSkill(i), i);
        }
    }

    private void SubscribeToSkillsManager()
    {
        var skillsManager = GameManager.Instance?.skillsManager;
        if (skillsManager == null) return;

        skillsManager.OnChosenSkillsChanged -= GenerateSkillCard;
        skillsManager.OnChosenSkillsChanged += GenerateSkillCard;
        skillsManager.OnChosenSkillsChanged -= GenerateSkillSlot;
        skillsManager.OnChosenSkillsChanged += GenerateSkillSlot;
    }

    private static void ClearContainer(Transform container)
    {
        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);
    }

    #endregion   
}
