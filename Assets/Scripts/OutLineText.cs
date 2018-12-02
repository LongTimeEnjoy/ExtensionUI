using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class OutLineText : Text {

	public bool m_OutLine=true;
	public Color m_OutLineColor=Color.black;
	public float m_OutLineOffsetX=0f;
	public float m_OutLineOffsetY=0f;
	protected override void OnPopulateMesh(VertexHelper toFill)
	{
		//这里是直接复制的UGUI的Text生成定点的代码
			Vector2 extent=rectTransform.rect.size;
			var settings= GetGenerationSettings(extent);
			cachedTextGenerator.Populate(this.text, settings);
			Rect inputRect = rectTransform.rect;
        	// get the text alignment anchor point for the text in local space
        	Vector2 textAnchorPivot = GetTextAnchorPivot(alignment);
        	Vector2 refPoint = Vector2.zero;
        	refPoint.x = Mathf.Lerp(inputRect.xMin, inputRect.xMax, textAnchorPivot.x);
        	refPoint.y = Mathf.Lerp(inputRect.yMin, inputRect.yMax, textAnchorPivot.y);

        	// Determine fraction of pixel to offset text mesh.
        	Vector2 roundingOffset = PixelAdjustPoint(refPoint) - refPoint;

        	// Apply the offset to the vertices
        	IList<UIVertex> verts = cachedTextGenerator.verts;
        	float unitsPerPixel = 1 / pixelsPerUnit;
        	//Last 4 verts are always a new line...
        	int vertCount = verts.Count - 4;
			toFill.Clear();
			UIVertex[] rVertex=new UIVertex[4];
        	if (roundingOffset != Vector2.zero)
        	{
            	for (int i = 0; i < vertCount; ++i)
            	{
                	int tempVertsIndex = i & 3;
                	rVertex[tempVertsIndex] = verts[i];
                	rVertex[tempVertsIndex].position *= unitsPerPixel;
                	rVertex[tempVertsIndex].position.x += roundingOffset.x;
                	rVertex[tempVertsIndex].position.y += roundingOffset.y;
                	if (tempVertsIndex == 3)
                    	toFill.AddUIVertexQuad(rVertex);
            	}
        	}
			else
			{
				for (int i = 0; i < verts.Count-4; i++)
				{
					int tempVertsIndex = i & 3;
                    rVertex[tempVertsIndex] = verts[i];
                    rVertex[tempVertsIndex].position.x += roundingOffset.x;
                    rVertex[tempVertsIndex].position.y += roundingOffset.y;
                    rVertex[tempVertsIndex].position *= unitsPerPixel;
                    rVertex[tempVertsIndex].uv1 = Vector2.zero;
                    if (m_OutLine && tempVertsIndex == 3)
                    {
						ApplyShadowZeroAlloc(ref rVertex, m_OutLineColor, m_OutLineOffsetX, m_OutLineOffsetY, toFill);
                        ApplyShadowZeroAlloc(ref  rVertex, m_OutLineColor, m_OutLineOffsetX, -m_OutLineOffsetY, toFill);
                        ApplyShadowZeroAlloc(ref  rVertex, m_OutLineColor, -m_OutLineOffsetX, m_OutLineOffsetY, toFill);
                        ApplyShadowZeroAlloc(ref rVertex, m_OutLineColor, -m_OutLineOffsetX, -m_OutLineOffsetY, toFill);
                        toFill.AddUIVertexQuad(rVertex);
                    }
				}
			}
	}
	private void ApplyShadowZeroAlloc(ref UIVertex[] rVertex, Color rEffectColor, float rEffectDistanceX, float rEffectDistanceY, VertexHelper rHelper)
    {
        for (int i = 0; i < rVertex.Length; i++)
        {
            Vector3 rPosition = rVertex[i].position;
            rPosition.x += rEffectDistanceX;
            rPosition.y += rEffectDistanceY;
            rVertex[i].position = rPosition;
            rVertex[i].color = rEffectColor;
        }
        rHelper.AddUIVertexQuad(rVertex);
        for (int i = 0; i < rVertex.Length; i++)
        {
            Vector3 rPosition = rVertex[i].position;
            rPosition.x -= rEffectDistanceX;
            rPosition.y -= rEffectDistanceY;
            rVertex[i].color = color;
            rVertex[i].position = rPosition;
        }
    }
}
