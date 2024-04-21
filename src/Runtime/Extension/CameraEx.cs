using UnityEngine;

public static class CameraEx
{
	public static void GetOrthographicSize(this Camera camera,out float width,out float height)
	{
        height = camera.orthographicSize * 2;
        width = height * camera.aspect;
    }
}
