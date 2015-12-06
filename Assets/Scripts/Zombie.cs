using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Zombie : MonoBehaviour {

	public enum ZombieState {Alive, Dead};
	public enum ZombieType {First, Second, Third};

	private ZombieState _ZombieState = ZombieState.Alive;
	private ZombieType _ZombieType;

	public event Action<Zombie> StopEvent; 
	public event Action<Zombie> ChangeZombieStateEvent;

	public ZombieState State {
		set {
			_ZombieState = value;
			if (value == ZombieState.Dead) {
				gameObject.GetComponent<Animator>().SetBool("back_fall", true);
			}
			if (ChangeZombieStateEvent != null)
				ChangeZombieStateEvent.Invoke(this);
		}
		get {
			return _ZombieState;
		}
	}

	public ZombieType Type {
		get {
			return _ZombieType;
		}
		set {
			_ZombieType = value;
		}
	}

	private void OnMouseDown() {
		State = ZombieState.Dead;
	}

	public void Move(List<Vector3> endPos) {
		StartCoroutine (StartMove(endPos, UnityEngine.Random.Range(2f,5f)));
	}

	private void InvokeStopEvent() {
		if (StopEvent != null)
			StopEvent.Invoke (this);
	}

	private IEnumerator StartMove(List<Vector3> endPos, float time){
		for (int k = 0; k < endPos.Count; k++) {
			float i = 0.0f;
			float rate = 1.0f / time / endPos.Count;
			Vector3 startPos = transform.position;
			while ((i < 1.0f) && (State == ZombieState.Alive)) {
				i += Time.deltaTime * rate;
				transform.position = Vector3.Lerp (startPos, endPos[k], i);
				yield return null; 
			}
		}

		if (State == ZombieState.Alive)
			InvokeStopEvent ();
	} 
}
