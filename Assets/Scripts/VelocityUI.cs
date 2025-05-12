using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VelocityUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider velocitySlider;
    [SerializeField] private Image sliderFillImage;
    [SerializeField] private TextMeshProUGUI velocityText;
    
    [Header("UI Settings")]
    [SerializeField] private Color normalColor = Color.green;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color maxColor = Color.red;
    [SerializeField] private float warningThreshold = 0.75f; // % of max when color changes to warning
    
    private UnifiedCatmullRomMovement movementController;
    
    void Start()
    {
        // Find the movement controller in the scene
        movementController = FindFirstObjectByType<UnifiedCatmullRomMovement>();
        
        if (movementController == null)
        {
            Debug.LogError("VelocityUI: Could not find UnifiedCatmullRomMovement in the scene!");
        }
        
        // Set up slider ranges
        if (velocitySlider != null)
        {
            velocitySlider.minValue = -movementController.GetMaxSpeed();
            velocitySlider.maxValue = movementController.GetMaxSpeed();
        }
    }
    
    void Update()
    {
        if (movementController == null) return;
        
        float currentVelocity = movementController.GetPlayerVelocity();
        float maxSpeed = movementController.GetMaxSpeed();
        
        // Update slider
        if (velocitySlider != null)
        {
            velocitySlider.value = currentVelocity;
        }
        
        // Update text (show with 1 decimal place)
        if (velocityText != null)
        {
            velocityText.text = $"Speed: {Mathf.Abs(currentVelocity):F1} / {maxSpeed}";
        }
        
        // Update color based on speed percentage
        if (sliderFillImage != null)
        {
            float speedPercentage = Mathf.Abs(currentVelocity) / maxSpeed;
            
            if (speedPercentage > warningThreshold && speedPercentage < 0.95f)
            {
                sliderFillImage.color = warningColor;
            }
            else if (speedPercentage >= 0.95f)
            {
                sliderFillImage.color = maxColor;
                
                // Optional: Make the text pulse when at max speed
                if (velocityText != null && speedPercentage > 0.99f)
                {
                    velocityText.color = new Color(
                        maxColor.r,
                        maxColor.g,
                        maxColor.b,
                        0.5f + 0.5f * Mathf.Sin(Time.time * 8)
                    );
                }
                else if (velocityText != null)
                {
                    velocityText.color = Color.white;
                }
            }
            else
            {
                sliderFillImage.color = normalColor;
                
                if (velocityText != null)
                {
                    velocityText.color = Color.white;
                }
            }
        }
    }
}