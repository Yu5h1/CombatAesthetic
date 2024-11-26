using UnityEngine.Events;

public static class UnityEventBaseEx 
{
	public static bool IsEmpty(this UnityEventBase e)
		=> e == null || e.GetPersistentEventCount() == 0;
}
