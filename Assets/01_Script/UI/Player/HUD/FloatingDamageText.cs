using TMPro;
using UnityEngine;

public class FloatingDamageText : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float fadeDuration = 0.8f;

    private Color textColor;
    private float currentTimer;

    private void Awake()
    {
        if (textMesh == null)
            textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(int damageAmount, bool isCritical)
    {
        if (textMesh == null) return;

        currentTimer = 0f;

        if (isCritical)
        {
            textMesh.text = $"{damageAmount}!";
            textMesh.fontSize = 6f;
            textColor = new Color(1f, 0.85f, 0.1f, 1f);

        }
        else
        {
            textMesh.text = damageAmount.ToString();
            textMesh.fontSize = 4f;
            textColor = Color.white;
        }

        textMesh.color = textColor;
    }

    private void Update()
    {
        transform.position += Vector3.up * (moveSpeed * Time.deltaTime);

        currentTimer += Time.deltaTime;
        float alpha = Mathf.Clamp01(1f - (currentTimer / fadeDuration));
        textColor.a = alpha;
        textMesh.color = textColor;

        if (currentTimer >= fadeDuration)
            Destroy(gameObject);
    }
}
