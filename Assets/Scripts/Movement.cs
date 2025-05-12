using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UnifiedCatmullRomMovement : MonoBehaviour {
    [Header("Player Controlled Object")]
    [SerializeField] private Transform playerControlledObject;
    [SerializeField] private Transform[] playerControlPoints;
    [SerializeField] private bool playerCurveLooping = true;
    [SerializeField] private float acceleration = 1f;
    [SerializeField] private float friction = 2f;
    [SerializeField] private float maxSpeed = 7f;

    [Header("Auto Moving Object")]
    [SerializeField] private Transform autoMovingObject;
    [SerializeField] private Transform[] autoControlPoints;
    [SerializeField] private bool autoCurveLooping = true;
    [SerializeField] private float autoMoveSpeed = 4f;
    [SerializeField] private bool autoObjectActive = true;

    [Header("UI")]
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private TMP_Text playerLapsText;
    [SerializeField] private TMP_Text autoLapsText;

    private bool gameEnded = false;

    private float playerVelocity = 0f;
    private float playerInterpolationAmount = 0f;
    private int playerSegment = 0;

    private float autoInterpolationAmount = 0f;
    private int autoSegment = 0;
    private int playerLaps = 0;
    private int autoLaps = 0;
    [SerializeField] private int maxLaps = 15;

    private void Start() {
        if (playerControlledObject != null && playerControlPoints.Length >= 4) {
            playerSegment = 0;
            playerInterpolationAmount = 0f;
            playerVelocity = 0f;
            playerControlledObject.position = GetCatmullRomPosition(0, 0, playerControlPoints);
        }

        if (autoMovingObject != null && autoControlPoints.Length >= 4) {
            autoSegment = 0;
            autoInterpolationAmount = 0f;
            autoMovingObject.position = GetCatmullRomPosition(0, 0, autoControlPoints);
        }
        gameOverText.text = "";
    }

    private void Update() {
        if (playerControlledObject != null && playerControlPoints.Length >= 4) {
            HandlePlayerInput();
            UpdatePlayerPosition();
        }

        if (autoMovingObject != null && autoControlPoints.Length >= 4 && autoObjectActive) {
            UpdateAutoPosition();
        }

        if (playerLapsText != null){
            playerLapsText.text = $"Player: {playerLaps} / {maxLaps}";
        }

        if (autoLapsText != null){
            autoLapsText.text = $"Rival:  {autoLaps} / {maxLaps}";
        }
    }
    private void Awake() {
        Time.timeScale = 0f;  // Game starts paused
        startButton.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(false);

        startButton.onClick.AddListener(StartGame);
        restartButton.onClick.AddListener(RestartGame);

    }

    private void StartGame() {
        Time.timeScale = 1f;
        startButton.gameObject.SetActive(false);
    }

    private void RestartGame() {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void HandlePlayerInput() {
        if (Keyboard.current.rightArrowKey.isPressed) {
            playerVelocity += acceleration * Time.deltaTime;
        } else {
            if (playerVelocity > 0) {
                playerVelocity -= friction * Time.deltaTime;
                if (playerVelocity < 0) playerVelocity = 0;
            }
        }

        if (Mathf.Abs(playerVelocity) > maxSpeed) {
            playerVelocity = 1f;
        }

        playerVelocity = Mathf.Clamp(playerVelocity, 0f, maxSpeed);
        playerInterpolationAmount += playerVelocity * Time.deltaTime;
    }

    private void UpdatePlayerPosition() {
        AdjustSegmentAndInterpolation(ref playerSegment, ref playerInterpolationAmount, ref playerVelocity, playerControlPoints, playerCurveLooping, true);
        Vector3 currentPosition = GetCatmullRomPosition(playerSegment, playerInterpolationAmount, playerControlPoints);

        float lookAheadAmount = 0.01f;
        float lookAheadT = playerInterpolationAmount + lookAheadAmount;
        int lookAheadSegment = playerSegment;

        if (lookAheadT >= 1.0f) {
            lookAheadT -= 1.0f;
            lookAheadSegment++;
            if (lookAheadSegment >= GetMaxSegment(playerControlPoints, playerCurveLooping)) {
                lookAheadSegment = playerCurveLooping ? 0 : playerSegment;
            }
        }

        Vector3 nextPosition = GetCatmullRomPosition(lookAheadSegment, lookAheadT, playerControlPoints);
        if (Vector3.Distance(nextPosition, currentPosition) > 0.001f) {
            Vector3 direction = currentPosition - nextPosition;
            playerControlledObject.rotation = Quaternion.LookRotation(direction.normalized);
        }

        playerControlledObject.position = currentPosition;
        if (!gameEnded && playerLaps >= maxLaps) {
            gameEnded = true;
            Time.timeScale = 0f;
            if (gameOverText != null) {
                gameOverText.text = "You Win!";  // Show the winning message
            }
            if (restartButton != null){
                restartButton.gameObject.SetActive(true);
            }
}
    }

    private void UpdateAutoPosition() {
        autoInterpolationAmount += autoMoveSpeed * Time.deltaTime;
        float dummySpeed = autoMoveSpeed;
        int lapDifference = playerLaps - autoLaps;

        autoMoveSpeed = Mathf.Clamp(4f + lapDifference * 2f, 4f, maxSpeed);

        AdjustSegmentAndInterpolation(ref autoSegment, ref autoInterpolationAmount, ref dummySpeed, autoControlPoints, autoCurveLooping, false);

        Vector3 currentPosition = GetCatmullRomPosition(autoSegment, autoInterpolationAmount, autoControlPoints);

        float lookAheadAmount = 0.01f;
        float lookAheadT = autoInterpolationAmount + lookAheadAmount;
        int lookAheadSegment = autoSegment;

        if (lookAheadT >= 1.0f) {
            lookAheadT -= 1.0f;
            lookAheadSegment++;
            if (lookAheadSegment >= GetMaxSegment(autoControlPoints, autoCurveLooping)) {
                lookAheadSegment = autoCurveLooping ? 0 : autoSegment;
            }
        }

        Vector3 nextPosition = GetCatmullRomPosition(lookAheadSegment, lookAheadT, autoControlPoints);
        if (Vector3.Distance(nextPosition, currentPosition) > 0.001f) {
            Vector3 direction = currentPosition - nextPosition;
            autoMovingObject.rotation = Quaternion.LookRotation(direction.normalized);
        }

        autoMovingObject.position = currentPosition;
        if (!gameEnded && autoLaps >= maxLaps) {
            gameEnded = true;
            Time.timeScale = 0f;
            if (gameOverText != null) {
                gameOverText.text = "You Lose!";  // Show the losing message
            }
            if (restartButton != null){
                restartButton.gameObject.SetActive(true);
            }
}
    }

    private void AdjustSegmentAndInterpolation(ref int segment, ref float interpolationAmount, ref float velocity, Transform[] points, bool looping, bool isPlayer = false) {
        int maxSegment = GetMaxSegment(points, looping);

        while (interpolationAmount >= 1f) {
            interpolationAmount -= 1f;
            segment++;
            if (segment >= maxSegment) {
                if (looping) {
                    segment = 0;
                    if (isPlayer) {
                        playerLaps++;
                        Debug.Log($"Player completed lap: {playerLaps}");
                    } else {
                        autoLaps++;
                        Debug.Log($"Auto object completed lap: {autoLaps}");
                    }
                } else {
                    segment = maxSegment - 1;
                    interpolationAmount = 1f;
                    velocity = 0f;
                }
            }
        }

        while (interpolationAmount < 0f) {
            interpolationAmount += 1f;
            segment--;
            if (segment < 0) {
                if (looping) {
                    segment = maxSegment - 1;

                    // 🔄 (Optional: count laps backward)
                    if (isPlayer) playerLaps--;
                    else autoLaps--;
                } else {
                    segment = 0;
                    interpolationAmount = 0f;
                    velocity = 0f;
                }
            }
        }
    }

    private int GetMaxSegment(Transform[] points, bool looping) {
        return looping ? points.Length : points.Length - 3;
    }

    private Vector3 GetCatmullRomPosition(int seg, float t, Transform[] points) {
        int count = points.Length;

        if (count < 4) {
            Debug.LogError("At least 4 control points are needed for Catmull-Rom spline.");
            return Vector3.zero;
        }

        int p0 = (seg - 1 + count) % count;
        int p1 = seg % count;
        int p2 = (seg + 1) % count;
        int p3 = (seg + 2) % count;

        return 0.5f * (
            2f * points[p1].position +
            (-points[p0].position + points[p2].position) * t +
            (2f * points[p0].position - 5f * points[p1].position + 4f * points[p2].position - points[p3].position) * t * t +
            (-points[p0].position + 3f * points[p1].position - 3f * points[p2].position + points[p3].position) * t * t * t
        );
    }

    // 🟢 Public getters for VelocityUI
    public float GetPlayerVelocity() {
        return playerVelocity;
    }

    public float GetMaxSpeed() {
        return maxSpeed;
    }
    public int GetPlayerLaps() {
    return playerLaps;
}

    public int GetAutoLaps() {
        return autoLaps;
    }
}
