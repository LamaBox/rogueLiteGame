using UnityEngine;

// ������ �������� ���������� ���
[DisallowMultipleComponent]
public class UnityCharacterModel2D : MonoBehaviour
{
    [Header("General")]
    [Tooltip("������������� ��������� ��������� ��� ������ (Editor/Play)")]
    public bool applyOnStart = true;

    [Header("Rigidbody2D")]
    [Tooltip("����� � ������ �� ������� ���������")]
    public float mass = 80f;
    [Tooltip("������������ ���������� ������������ ��� �������� ��������")]
    public RigidbodyInterpolation2D interpolation = RigidbodyInterpolation2D.Interpolate;
    [Tooltip("��� �������� ������������ ��� ��������")]
    public CollisionDetectionMode2D collisionDetection = CollisionDetectionMode2D.Continuous;
    [Tooltip("���������� ��������")]
    public bool freezeRotation = true;

    [Header("Collider2D (����� ���� ���� �������)")]
    [Tooltip("������ �������")]
    public float capsuleHeight = 1.8f;
    [Tooltip("������ �������")]
    public float capsuleRadius = 0.35f;
    [Tooltip("�������� ������ ���������� �� ��������� Y")]
    public float capsuleOffsetY = 0.9f;
    [Tooltip("������������ trigger ������ ����������� ����������")]
    public bool useTrigger = false;

    [Header("Physics Material 2D")]
    [Tooltip("������")]
    public float friction = 0.0f;
    [Tooltip("������")]
    public float bounciness = 0f;

    [Header("Advanced")]
    [Tooltip("���� �������� ����������� ����� Rigidbody2D, �������� ���." +
        " ��� ��������� ������������ �������� false")]
    public bool useRigidbodyMovement = true;

    private Rigidbody2D rb;
    private CapsuleCollider2D capsule;
    private PhysicsMaterial2D createdMaterial;

    void Start()
    {
        if (applyOnStart)
            Apply();
    }


   
    [ContextMenu("Apply Physical Model 2D")]
    public void Apply()
    {
        SetupRigidbody2D();
        SetupCollider2D();
        SetupPhysicsMaterial2D();
        ApplyConstraints();
    }

    private void SetupRigidbody2D()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        rb.mass = Mathf.Max(0.0001f, mass);
        rb.drag = Mathf.Max(0f, linearDrag);
        rb.angularDrag = Mathf.Max(0f, angularDrag);
        rb.interpolation = interpolation;
        rb.collisionDetectionMode = collisionDetection;
        rb.gravityScale = 1f;
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    private void SetupCollider2D()
    {
        capsule = GetComponent<CapsuleCollider2D>();
        if (capsule == null)
            capsule = gameObject.AddComponent<CapsuleCollider2D>();

        capsule.size = new Vector2(Mathf.Max(0.01f, capsuleRadius * 2f), Mathf.Max(0.01f, capsuleHeight));
        capsule.offset = new Vector2(0f, capsuleOffsetY);
        capsule.isTrigger = useTrigger;
    }

    private void SetupPhysicsMaterial2D()
    {
        if (createdMaterial != null)
        {
            createdMaterial.friction = friction;
            createdMaterial.bounciness = bounciness;
        }
        else
        {
            createdMaterial = new PhysicsMaterial2D(name + "_PhysicsMaterial2D");
            createdMaterial.friction = friction;
            createdMaterial.bounciness = bounciness;
        }

        if (capsule != null)
            capsule.sharedMaterial = createdMaterial;
    }

    private void ApplyConstraints()
    {
        if (rb == null) return;

        if (freezeRotation)
            rb.freezeRotation = true;
    }
}
