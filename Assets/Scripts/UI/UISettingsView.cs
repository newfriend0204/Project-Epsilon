using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ProjectEpsilon {
    public class UISettingsView : MonoBehaviour
	{
		public Slider Volume;
		public Slider Sensitivity;
		public Slider ZoomSensitivity;
        public TextMeshProUGUI SensitivityValue;
        public TextMeshProUGUI ZoomSensitivityValue;

        public void LoadSettings() {
			float volume = PlayerPrefs.GetFloat("Volume", 1f);
			Volume.value = volume;

            float sensitivity = PlayerPrefs.GetFloat("Sensitivity", 3f);
            Sensitivity.value = sensitivity;

            float zoomSensitivity = PlayerPrefs.GetFloat("ZoomSensitivity", 50f);
            ZoomSensitivity.value = zoomSensitivity;

            AudioListener.volume = volume;
			PlayerInput.LookSensitivity = sensitivity;
		}

        public void SensitivityChanged(float value) {
            PlayerInput.LookSensitivity = value;
            PlayerPrefs.SetFloat("Sensitivity", value);

            SensitivityValue.text = $"{value:F1}";
        }

        public void ZoomSensitivityChanged(float value) {
            PlayerInput.LookZoomSensitivity = value;
            PlayerPrefs.SetFloat("ZoomSensitivity", value);

            ZoomSensitivityValue.text = $"{value:F1}" + "%";
        }

        public void VolumeChanged(float value) {
			AudioListener.volume = value;
			PlayerPrefs.SetFloat("Volume", value);
		}

		public void CloseView() {
			gameObject.SetActive(false);
			GetComponentInParent<GameUI>().MenuView.SetActive(false);
		}

		private void Update() {
			if (Keyboard.current.escapeKey.wasPressedThisFrame) {
				gameObject.SetActive(false);
			}
		}
	}
}
