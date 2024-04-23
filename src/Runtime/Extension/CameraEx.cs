using UnityEngine;

public static class CameraEx
{
	public static void GetOrthographicSize(this Camera camera,out float width,out float height)
	{
        height = camera.orthographicSize * 2;
        width = height * camera.aspect;
    }
	public static Vector2 GetNormalizedCoordinates(this Camera cam,Vector2 screenPoint)
	{
        var result = cam.ScreenToViewportPoint(screenPoint);
        return new Vector3(2 * result.x - 1, 2 * result.y - 1, result.z);
    }
}
