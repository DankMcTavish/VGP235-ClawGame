using UnityEngine;

public class ClawGripController : MonoBehaviour
{
    [Header("Claw Grip Settings")]
    public float gripStrength = 10.0f;
    public float gripSpeed = 5.0f;
    public float openAngle = 45.0f;
    public float closedAngle = 0.0f;

    [Header("Claw Components")]
    public GameObject leftClaw;
    public GameObject rightClaw;
    public GameObject centerClaw;

    [Header("Object Reference")]
    public GameObject grabbedObject;

    private float currentAngle;
    private float targetAngle;
    private bool isClosed = false;

    void Start()
    {
        currentAngle = openAngle;
        targetAngle = openAngle;
        UpdateClawRotation();
    }

    void Update()
    {
        if (Mathf.Abs(currentAngle - targetAngle) > 0.1f)
        {
            currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, gripSpeed * Time.deltaTime * 50f);
            UpdateClawRotation();
        }
    }

    public void OpenGrip()
    {
        targetAngle = openAngle;
        isClosed = false;
    }

    public void CloseGrip()
    {
        targetAngle = closedAngle;
        isClosed = true;
    }

    public void ToggleGrip()
    {
        if (isClosed)
            OpenGrip();
        else
            CloseGrip();
    }

    public void GripObject()
    {
        CloseGrip();
    }

    public void ReleaseObject()
    {
        OpenGrip();
    }

    private void UpdateClawRotation()
    {
        // Left (-Z) and Right (+Z) are standard side claws
        if (leftClaw) leftClaw.transform.localRotation = Quaternion.Euler(0, 0, -currentAngle);
        if (rightClaw) rightClaw.transform.localRotation = Quaternion.Euler(0, 0, currentAngle);
        
        // Center claw (at the back/front) likely rotates on X
        if (centerClaw) centerClaw.transform.localRotation = Quaternion.Euler(currentAngle, 0, 0);
    }
}
