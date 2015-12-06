using UnityEngine;
using System.Collections;

public class BaseDialog : MonoBehaviour {

	public virtual void Close() {
		Object.Destroy (this.gameObject);
	}
}
