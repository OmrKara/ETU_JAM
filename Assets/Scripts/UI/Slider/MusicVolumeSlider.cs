using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public enum VolumeType
    {
        Music,
        SFX
    }

    [SerializeField] private VolumeType volumeType;
    [SerializeField] private Slider slider;

    private void Awake()
    {
        if (slider == null)
            slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        // 🔁 Panel her açıldığında static değerden senkronla
        float value = volumeType == VolumeType.Music
            ? AudioSettings.MusicVolume
            : AudioSettings.SfxVolume;

        slider.SetValueWithoutNotify(value);

        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        if (volumeType == VolumeType.Music)
            AudioSettings.MusicVolume = value;
        else
            AudioSettings.SfxVolume = value;
    }
}
