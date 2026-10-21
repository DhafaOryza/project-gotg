using UnityEngine;

public class SkillGhostPreview : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color PreviewColor = new Color(1f, 1f, 1f, 0.45f);

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (PreviewColor != null)
            spriteRenderer.color = PreviewColor;
    }

    public void SetPositionandRotation(Vector3 position, Quaternion Rotation)
    {
        transform.position = position;
        transform.rotation = Rotation;
    }

    public void Show (bool isVisible) => gameObject.SetActive(isVisible); 
}
