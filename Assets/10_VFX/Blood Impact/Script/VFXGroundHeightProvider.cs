using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class VFXGroundHeightProvider : MonoBehaviour
{
    [Header("Referensi")]
    [SerializeField] private VisualEffect _visualEffect;

    [Header("Ground Raycast")]
    [Tooltip("Layer Collider yang dianggap ground. Pilih layer 'Ground' setelah layer dibuat.")]
    [SerializeField] private LayerMask _groundLayer;

    [Tooltip("Tinggi awal raycast di atas posisi Visual Effect.")]
    [SerializeField] private float _rayOriginHeight = 5f;

    [Tooltip("Jarak maksimal raycast ke bawah.")]
    [SerializeField] private float _maxRayDistance = 100f;

    [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;

    [Header("VFX Graph")]
    [Tooltip("Nama exposed property bertipe Vector3/Position di VFX Graph.")]
    [SerializeField] private string _groundPositionProperty = "Ground Position";

    [SerializeField] private bool _updateEveryFrame = true;

    [Header("Debug Gizmos")]
    [Tooltip("Tampilkan visualisasi raycast di Scene view.")]
    [SerializeField] private bool _drawDebugRay = true;

    [Tooltip("Tampilkan gizmo walaupun GameObject tidak dipilih.")]
    [SerializeField] private bool _drawGizmoAlways = true;

    [SerializeField] private Color _gizmoHitColor = new Color(0.20f, 1f, 0.35f, 1f);
    [SerializeField] private Color _gizmoMissColor = new Color(1f, 0.35f, 0.20f, 1f);
    [SerializeField] private float _gizmoMarkerSize = 0.08f;

    private string _resolvedPropertyName;

    private void Awake()
    {
        ResolveVisualEffect();
    }

    private void OnEnable()
    {
        ResolveVisualEffect();
    }

    private void OnValidate()
    {
        _resolvedPropertyName = null;
    }

    private void Update()
    {
        if (_updateEveryFrame)
            UpdateGroundPosition();
    }

    public void UpdateGroundPosition()
    {
        ResolveVisualEffect();

        if (_visualEffect == null)
            return;

        Vector3 origin = GetRayOrigin();
        float distance = Mathf.Max(0f, _maxRayDistance);

        RaycastHit hit;
        bool hitGround = Physics.Raycast(
            origin,
            Vector3.down,
            out hit,
            distance,
            _groundLayer.value,
            _triggerInteraction);

        if (!hitGround)
            return;

        string propertyName = ResolveGroundPositionProperty();
        _visualEffect.SetVector3(propertyName, hit.point);
    }

    private string ResolveGroundPositionProperty()
    {
        if (!string.IsNullOrEmpty(_resolvedPropertyName))
            return _resolvedPropertyName;

        string[] candidates =
        {
            _groundPositionProperty,
            "Ground Posistion",
            "Ground Position",
            "GroundPosition"
        };

        VisualEffectAsset asset = _visualEffect != null ? _visualEffect.visualEffectAsset : null;
        if (asset != null)
        {
            List<VFXExposedProperty> properties = new List<VFXExposedProperty>();
            asset.GetExposedProperties(properties);

            foreach (string candidate in candidates)
            {
                string wanted = Normalize(candidate);
                if (string.IsNullOrEmpty(wanted))
                    continue;

                foreach (VFXExposedProperty property in properties)
                {
                    if (property.type != typeof(Vector3))
                        continue;

                    string actual = Normalize(property.name);
                    if (actual == wanted)
                    {
                        _resolvedPropertyName = property.name;
                        return property.name;
                    }

                    const string PositionSuffix = "position";
                    if (actual.EndsWith(PositionSuffix))
                    {
                        string bare = actual.Substring(0, actual.Length - PositionSuffix.Length);
                        if (bare == wanted)
                        {
                            _resolvedPropertyName = property.name;
                            return property.name;
                        }
                    }
                }
            }
        }

        _resolvedPropertyName = _groundPositionProperty;
        return _resolvedPropertyName;
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value.Replace(" ", string.Empty).Replace("_", string.Empty).ToLowerInvariant();
    }

    private void ResolveVisualEffect()
    {
        if (_visualEffect != null)
            return;

        _visualEffect = GetComponentInChildren<VisualEffect>(true);
    }

    private Transform GetSourceTransform()
    {
        if (_visualEffect != null)
            return _visualEffect.transform;

        VisualEffect child = GetComponentInChildren<VisualEffect>(true);
        return child != null ? child.transform : transform;
    }

    private Vector3 GetRayOrigin()
    {
        Transform source = GetSourceTransform();
        return source.position + Vector3.up * Mathf.Max(0f, _rayOriginHeight);
    }

    private void OnDrawGizmos()
    {
        if (!_drawDebugRay || !_drawGizmoAlways)
            return;

        DrawRayGizmo(false);
    }

    private void OnDrawGizmosSelected()
    {
        if (!_drawDebugRay)
            return;

        DrawRayGizmo(true);
    }

    private void DrawRayGizmo(bool showLabel)
    {
        Vector3 origin = GetRayOrigin();
        float distance = Mathf.Max(0f, _maxRayDistance);

        RaycastHit hit;
        bool hitGround = Physics.Raycast(
            origin,
            Vector3.down,
            out hit,
            distance,
            _groundLayer.value,
            _triggerInteraction);

        Vector3 end = hitGround ? hit.point : origin + Vector3.down * distance;
        Gizmos.color = hitGround ? _gizmoHitColor : _gizmoMissColor;

        Gizmos.DrawLine(origin, end);
        Gizmos.DrawWireSphere(origin, _gizmoMarkerSize * 0.75f);
        Gizmos.DrawWireSphere(end, _gizmoMarkerSize);

        if (distance > 0f)
        {
            Vector3 back = Vector3.up * Mathf.Min(0.25f, distance * 0.1f);
            Vector3 side = Vector3.right * (_gizmoMarkerSize * 1.5f);
            Vector3 depth = Vector3.forward * (_gizmoMarkerSize * 1.5f);

            Gizmos.DrawLine(end, end + back + side);
            Gizmos.DrawLine(end, end + back - side);
            Gizmos.DrawLine(end, end + back + depth);
            Gizmos.DrawLine(end, end + back - depth);
        }

#if UNITY_EDITOR
        if (showLabel)
        {
            string label = hitGround
                ? string.Format(
                    "Ground: {0} | {1:0.00}m",
                    LayerMask.LayerToName(hit.collider.gameObject.layer),
                    hit.distance)
                : "No ground hit";

            Handles.Label(origin + Vector3.up * 0.2f, label);
        }
#endif
    }
}
