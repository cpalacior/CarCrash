using UnityEngine;
using UnityEngine.InputSystem;

public class CatmullRomKeyboardMovement : MonoBehaviour {
    [SerializeField] private Transform[] controlPoints;
    [SerializeField] private Transform pointOnCurve;
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float friction = 2f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private bool isLooping = true;

    private float velocity = 0f;
    private float interpolationAmount = 0f;
    private int segment = 0;

    private void Update() {
        HandleInput();

        // Avanzar por segmentos si interpolationAmount excede 1
        while (interpolationAmount >= 1f) {
            interpolationAmount -= 1f;
            segment++;
            if (segment >= GetMaxSegment()) {
                if (isLooping) segment = 0;
                else {
                    segment = GetMaxSegment() - 1;
                    interpolationAmount = 1f;
                    velocity = 0f;
                }
            }
        }

        while (interpolationAmount < 0f) {
            interpolationAmount += 1f;
            segment--;
            if (segment < 0) {
                if (isLooping) segment = GetMaxSegment() - 1;
                else {
                    segment = 0;
                    interpolationAmount = 0f;
                    velocity = 0f;
                }
            }
        }

        pointOnCurve.position = GetCatmullRomPosition(segment, interpolationAmount);
    }

    private void HandleInput() {
        if (Keyboard.current.rightArrowKey.isPressed) {
            velocity += acceleration * Time.deltaTime;
        } else if (Keyboard.current.leftArrowKey.isPressed) {
            velocity -= acceleration * Time.deltaTime;
        } else {
            // Apply friction
            if (velocity > 0) {
                velocity -= friction * Time.deltaTime;
                if (velocity < 0) velocity = 0;
            } else if (velocity < 0) {
                velocity += friction * Time.deltaTime;
                if (velocity > 0) velocity = 0;
            }
        }

        // If velocity exceeds 4f, snap it to 1f (preserve direction)
        if (Mathf.Abs(velocity) > 6f) {
            velocity = Mathf.Sign(velocity) * 1f;
        }

        velocity = Mathf.Clamp(velocity, -maxSpeed, maxSpeed);
        interpolationAmount += velocity * Time.deltaTime;
    }



    private int GetMaxSegment() {
        return isLooping ? controlPoints.Length : controlPoints.Length - 3;
    }

    private Vector3 GetCatmullRomPosition(int seg, float t) {
        int p0 = (seg - 1 + controlPoints.Length) % controlPoints.Length;
        int p1 = (seg + 0) % controlPoints.Length;
        int p2 = (seg + 1) % controlPoints.Length;
        int p3 = (seg + 2) % controlPoints.Length;

        return 0.5f * (
            2f * controlPoints[p1].position +
            (-controlPoints[p0].position + controlPoints[p2].position) * t +
            (2f * controlPoints[p0].position - 5f * controlPoints[p1].position + 4f * controlPoints[p2].position - controlPoints[p3].position) * t * t +
            (-controlPoints[p0].position + 3f * controlPoints[p1].position - 3f * controlPoints[p2].position + controlPoints[p3].position) * t * t * t
        );
    }
}
