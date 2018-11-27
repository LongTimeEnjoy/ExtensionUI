using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
public class EmojiBuildTool : Editor {
    
    [MenuItem("Tools/EmojiAtlasBuild")]
    public static void CreateEmojiAtlas()
    {
        var rSpritesDic= GetALLSprites();
        var rConfig= GenerateConfig(rSpritesDic);
        GenerateAtlas(rSpritesDic, rConfig);
        SaveConfig(rConfig);
    }
    private static void SaveConfig(Dictionary<string, EmojiInfo>rEmojiInfoDic)
    {
        string rConfigSavePath = Application.dataPath + "/Resources/Emoji/EmojiConfig.txt";
        string rJson= LitJson.JsonMapper.ToJson(rEmojiInfoDic);
        using (FileStream rFs = new FileStream(rConfigSavePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            var rBytes = Encoding.UTF8.GetBytes(rJson);
            rFs.Write(rBytes, 0, rBytes.Length);
        }
    }
    private static void GenerateAtlas(Dictionary<string, List<Texture2D>> rSpriteDic, Dictionary<string, EmojiInfo>rEmojiInfoDic)
    {
        int rSpriteSize = 32;//所有的表情都32的大小，规定死的
        Texture2D rAtlas = new Texture2D(1024, 1024, TextureFormat.ARGB32, false);
        int rCurrentWidth = 0;
        int rCurrentHeight = 0;
        foreach (var sprite in rSpriteDic)
        {
            for (int i = 0; i < sprite.Value.Count; i++)
            {
                rEmojiInfoDic[sprite.Key].mUV_X = ((float)rCurrentWidth / 1024f).ToString();
                rEmojiInfoDic[sprite.Key].mUV_Y = ((float)rCurrentHeight / 1024f).ToString();
                var rTex = sprite.Value[i];
                for (int width = 0; width < rSpriteSize; width++)
                {
                    for (int height = 0; height < rSpriteSize; height++)
                    {
                        rAtlas.SetPixel(rCurrentWidth + width, rCurrentHeight + height, rTex.GetPixel(width, height));
                    }
                }
                if (rCurrentWidth + rSpriteSize >= 1024)
                {
                    rCurrentWidth = 0;
                    rCurrentHeight += rSpriteSize;
                }
                else
                    rCurrentWidth += rSpriteSize;
               
            }
        }
        rAtlas.Apply();
        AssetDatabase.Refresh();
        using (FileStream rFs = new FileStream(Application.dataPath + "/Resources/Emoji/EmojiAtlas.png", FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            var rBytes = rAtlas.EncodeToPNG();
            rFs.Write(rBytes, 0, rBytes.Length);
        }
    }
    private static Dictionary<string, EmojiInfo> GenerateConfig(Dictionary<string, List<Texture2D>> rSpriteDic)
    {
        Dictionary<string, EmojiInfo> rEmojiInfoDic = new Dictionary<string, EmojiInfo>();
        foreach (var sprite in rSpriteDic)
        {
            EmojiInfo rInfo = new EmojiInfo();
            rInfo.mKey = sprite.Key;
            rInfo.mFrame = sprite.Value.Count;
            rEmojiInfoDic.Add(sprite.Key, rInfo);
        }
        return rEmojiInfoDic;
    }
    private static Dictionary<string, List<Texture2D>> GetALLSprites()
    {
        Dictionary<string, List<Texture2D>> rSpritesDic = new Dictionary<string, List<Texture2D>>();
        string[]rGUID= AssetDatabase.FindAssets("t:texture", new string[] { "Assets/Textures/EmojiSprites" });
        for (int i = 0; i < rGUID.Length; i++)
        {
            string rPath= AssetDatabase.GUIDToAssetPath(rGUID[i]);
            var rTex= AssetDatabase.LoadAssetAtPath<Texture2D>(rPath);
            string[]rNameSplit= rTex.name.Split('_');
            if (rSpritesDic.ContainsKey(rNameSplit[0]))
                rSpritesDic[rNameSplit[0]].Add(rTex);
            else
            {
                List<Texture2D> rSprites = new List<Texture2D>();
                rSprites.Add(rTex);
                rSpritesDic.Add(rNameSplit[0], rSprites);
            }
        }
        return rSpritesDic;
    }
    
}
