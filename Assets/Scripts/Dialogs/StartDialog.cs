using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class StartDialog : BaseDialog {

	[SerializeField]
	protected Button playButton;

	public event Action OnPlayClick;

	private void Start () {
		playButton.onClick.AddListener (() => {
			if (OnPlayClick != null)
				OnPlayClick.Invoke();
		});
	}
}
