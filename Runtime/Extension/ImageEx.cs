using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ImageEx 
{
	public static void SetAlpha(this Image img,float alpha)
	{
        if (img.color.a == alpha)
            return;
        var c = img.color;
        c.a = alpha;
        img.color = c;
    }
}
