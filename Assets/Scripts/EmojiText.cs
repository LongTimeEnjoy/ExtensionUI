using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System;
using LitJson;

public class EmojiText : Text {
	private Material mEmojiMaterial;
	private string mReplaceString="方";
	private Dictionary<string,EmojiInfo>mEmojiInfos;

	protected override void OnPopulateMesh(VertexHelper rHelper)
	{
		if (mEmojiMaterial==null)
			mEmojiMaterial=Resources.Load<Material>("/Emoji/EmojiMaterial");
		LoadConfig();
		if (supportRichText)
		{
			Dictionary<int,EmojiInfo>rFindEmojis=new Dictionary<int, EmojiInfo>();
			int rLastIndex=0;
			//利用正则表达式找到符合约定的占位符
			MatchCollection rMatches=Regex.Matches(text,"\\[Image=[0-9]+\\]");
			StringBuilder rTempString=new StringBuilder();
			for (int i = 0; i < rMatches.Count; i++)
			{
				EmojiInfo rInfo;
				if (mEmojiInfos.TryGetValue(rMatches[i].Value,out rInfo))
				{
					//因为会把“[]”去掉,[]是不需要生成顶点的
					rFindEmojis.Add(rMatches[i].Index-i*2,rInfo);
					//从上一个匹配的位置截取到下一个匹配的位置
					rTempString.Append(text.Substring(rLastIndex,rMatches[i].Index-rLastIndex));
					//然后把[Image=**]替换成一个中文字符
					rTempString.Append(mReplaceString);
					//记录下索引的位置
					rLastIndex=rMatches[i].Index+rMatches[i].Length;
				}
			}
			if (rLastIndex<text.Length)
				rTempString.Append(text.Substring(rLastIndex,text.Length));
			//这里是直接复制的UGUI的Text生成定点的代码
			Vector2 extent=rectTransform.rect.size;
			var settings= GetGenerationSettings(extent);
			cachedTextGenerator.Populate(text, settings);
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
			rHelper.Clear();
			UIVertex[] m_TempVerts=new UIVertex[4];
        	if (roundingOffset != Vector2.zero)
        	{
            	for (int i = 0; i < vertCount; ++i)
            	{
                	int tempVertsIndex = i & 3;
                	m_TempVerts[tempVertsIndex] = verts[i];
                	m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                	m_TempVerts[tempVertsIndex].position.x += roundingOffset.x;
                	m_TempVerts[tempVertsIndex].position.y += roundingOffset.y;
                	if (tempVertsIndex == 3)
                    	rHelper.AddUIVertexQuad(m_TempVerts);
            	}
        	}
			else
			{
				for (int i = 0; i < verts.Count; i++)
				{
					EmojiInfo rInfo;
					if (rFindEmojis.TryGetValue(i,out rInfo))
					{
						float charDis = (verts[i + 1].position.x - verts[i].position.x);
        				m_TempVerts[3] = verts[i];//1
        				m_TempVerts[2] = verts[i + 1];//2
        				m_TempVerts[1] = verts[i + 2];//3
        				m_TempVerts[0] = verts[i + 3];//4
        				m_TempVerts[2].position += new Vector3(charDis, 0, 0);
        				m_TempVerts[1].position += new Vector3(charDis, 0, 0);

        				//让emoji长宽相等
        				float fixValue = (m_TempVerts[2].position.x - m_TempVerts[3].position.x - (m_TempVerts[2].position.y - m_TempVerts[1].position.y));
        				m_TempVerts[2].position -= new Vector3(fixValue, 0, 0);
        				m_TempVerts[1].position -= new Vector3(fixValue, 0, 0);

        				m_TempVerts[0].position *= unitsPerPixel;
        				m_TempVerts[1].position *= unitsPerPixel;
        				m_TempVerts[2].position *= unitsPerPixel;
        				m_TempVerts[3].position *= unitsPerPixel;

        				//计算Emoji的UV，利用uv0传递帧数，uv1是emoji的纹理坐标
        				m_TempVerts[0].uv1 = new Vector2(float.Parse(rInfo.mUV_X), float.Parse(rInfo.mUV_Y));
        				m_TempVerts[1].uv1 = new Vector2(float.Parse(rInfo.mUV_X + 32), float.Parse(rInfo.mUV_Y));
        				m_TempVerts[2].uv1 = new Vector2(float.Parse(rInfo.mUV_X + 32), float.Parse(rInfo.mUV_Y+ 32));
        				m_TempVerts[3].uv1 = new Vector2(float.Parse((rInfo.mUV_X)), float.Parse(rInfo.mUV_Y + 32));
        				m_TempVerts[0].uv0 = new Vector2(rInfo.mFrame, 0);
        				m_TempVerts[1].uv0 = new Vector2(rInfo.mFrame, 0);
        				m_TempVerts[2].uv0 = new Vector2(rInfo.mFrame, 0);
        				m_TempVerts[3].uv0 = new Vector2(rInfo.mFrame, 0);

        				rHelper.AddUIVertexQuad(m_TempVerts);
						i+=3;
					}
					else
					{
						int tempVertsIndex = i & 3;
                    	m_TempVerts[tempVertsIndex] = verts[i];
                    	m_TempVerts[tempVertsIndex].position.x += roundingOffset.x;
                    	m_TempVerts[tempVertsIndex].position.y += roundingOffset.y;
                    	m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                    	m_TempVerts[tempVertsIndex].uv1 = Vector2.zero;
						if (tempVertsIndex == 3)
                            rHelper.AddUIVertexQuad(m_TempVerts);

					}
				}
			}
		}
	}
	private void LoadConfig()
	{
		if (mEmojiInfos==null)
		{
			string rConfigJson= File.ReadAllText(Application.dataPath+"/Resources/Emoji/EmojiConfig");
            mEmojiInfos=JsonMapper.ToObject<Dictionary<string, EmojiInfo>>(rConfigJson);
		}
	}
}
public class EmojiInfo
{
    public string mKey;
    public int mFrame;
    public string mUV_X;
    public string mUV_Y;
}

