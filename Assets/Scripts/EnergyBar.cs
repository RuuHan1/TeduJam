using UnityEngine;
using UnityEngine.UI;

public class EnergyBar : MonoBehaviour
{
    public static EnergyBar Instance { get; private set; }

    [SerializeField] private Image fill;

    private void Awake()
    {
        Instance = this;
    }

    public void SetValue(float value)
    {
        fill.fillAmount = value;
    }
}
