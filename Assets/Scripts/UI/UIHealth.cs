using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectEpsilon {
    public class UIHealth : MonoBehaviour {
		public TextMeshProUGUI Value;
		public Image           Progress;
		public GameObject      ImmortalityIndicator;
		public GameObject      HitTakenEffect;
		public GameObject      DeathEffect;
		public Animation       HealthProgressAnimation;
		public TextMeshProUGUI HealValue;

		private int _lastHealth = -1;

		public void UpdateHealth(Health health) {
			ImmortalityIndicator.SetActive(health.IsImmortal);

			int currentHealth = Mathf.CeilToInt(health.CurrentHealth);

			if (currentHealth == _lastHealth)
				return;

			Value.text = currentHealth.ToString();

			float progress = health.CurrentHealth / health.MaxHealth;
			Progress.fillAmount = progress;
			SampleHealthProgressAnimation(progress);

			if (currentHealth < _lastHealth) {

				HitTakenEffect.SetActive(false);
				HitTakenEffect.SetActive(true);
			}

			DeathEffect.SetActive(health.IsAlive == false);

			_lastHealth = currentHealth;
		}

		public void ShowHeal(float value) {
			HealValue.text = $"+{Mathf.RoundToInt(value)} HP";

			HealValue.gameObject.SetActive(false);
			HealValue.gameObject.SetActive(true);
		}

		private void Awake() {
			HitTakenEffect.SetActive(false);
			HealValue.gameObject.SetActive(false);
		}

		private void SampleHealthProgressAnimation(float normalizedTime) {
			var animationState = HealthProgressAnimation[HealthProgressAnimation.clip.name];

			animationState.weight = 1f;
			animationState.enabled = true;

			animationState.normalizedTime = normalizedTime;
			HealthProgressAnimation.Sample();

			animationState.enabled = false;
		}
	}
}
