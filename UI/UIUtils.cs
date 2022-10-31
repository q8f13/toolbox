using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIUtils
{
    // screen position to local point in rectTransform
	public static Vector2 ScreenToRectPos(Vector2 screen_pos, RectTransform rtm, Canvas canvas)
	{
        Vector2 anchorPos;
		if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && canvas.worldCamera != null)
		{
			//Canvas is in Camera mode
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rtm, screen_pos, canvas.worldCamera, out anchorPos);
		}
		else
		{
			//Canvas is in Overlay mode
			anchorPos = screen_pos - new Vector2(rtm.position.x, rtm.position.y);
			anchorPos = new Vector2(anchorPos.x / rtm.lossyScale.x, anchorPos.y / rtm.lossyScale.y);
		}
        return anchorPos;
	}
}
