using UnityEngine;
using System.Collections;

public interface IObserver
{
	void onNotify(Completed.GameManager.GameEvent e);

}
