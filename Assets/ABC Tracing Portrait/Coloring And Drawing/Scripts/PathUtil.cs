using System.IO;
using UnityEngine;

namespace KGF.Coloring
{
    public enum TextureSaveFormat
{
    Png,
    Jpg,
    Raw
}
public static class PathUtil
    {
        public static string Combine(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
            {
                return "";
            }

            if (paths.Length == 1)
            {
                return paths[0];
            }

            string result = Path.Combine(paths[0], paths[1]);
            for (int i = 2; i < paths.Length; i++)
            {
                result = Path.Combine(result, paths[i]);
            }

            return result;
        }

        public static string AddPrefixToStreamingAssetPathIfNeeded(string path)
        {
#if !UNITY_EDITOR
			if (!path.Contains("://"))
			{
				path = "file://" + path;
			}
#endif
            return path;
        }

        public static void CreateDirectoryIfNoExistsForFileName(string filePath)
        {
            string dir = Path.GetDirectoryName(filePath);
            CreateDirectoryIfNoExists(dir);
        }

        public static void CreateDirectoryIfNoExists(string directoryPath)
        {

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        public static bool Exist(string path)
        {
        
        return File.Exists(path);
         }

        public static void SaveRenderTexture(this Texture2D rt, string path, TextureSaveFormat saveFormat)
        {
            byte[] png = rt.Encode(saveFormat);

            new System.Threading.Thread(() =>
            {
                PathUtil.CreateDirectoryIfNoExistsForFileName(path);
                //MonoBehaviour.print(path);
                File.WriteAllBytes(path, png);
            }).Start();
        }

        public static void SaveTexture(string Path,byte[] data)
        {

        new System.Threading.Thread(() =>
        {
            PathUtil.CreateDirectoryIfNoExistsForFileName(Path);
            //MonoBehaviour.print(path);
            File.WriteAllBytes(Path, data);
        }).Start();
    }

        public static Texture2D ReadAllByt(string path)
        {
            if(Exist(path))
            {
            Texture2D mtexture = new Texture2D(10, 10);
            mtexture.LoadImage(File.ReadAllBytes(path));
            mtexture.name = "uniqueNameToSearch#########################################################################";
            return mtexture;
            }
            else
            {
                return null;
            }
        
        }
        public static Sprite ReadAllByteForSprite(string path)
        {
            if(Exist(path))
            {
            Texture2D mtexture = new Texture2D(10, 10);
            mtexture.LoadImage(File.ReadAllBytes(path));
            var sp =  Sprite.Create(mtexture, new Rect(0, 0, mtexture.width, mtexture.height), Vector2.zero, 100, 1, SpriteMeshType.FullRect);
            Object.Destroy(mtexture);
            return sp;
            }
            else
            {
                return null;
            }
        
        }

        public static byte[] Encode(this Texture2D texture, TextureSaveFormat saveFormat)
        {
            if (saveFormat == TextureSaveFormat.Jpg)
            {
                return texture.EncodeToPNG();
            }
            else if (saveFormat == TextureSaveFormat.Png)
            {
                return texture.EncodeToPNG();
            }
            else if (saveFormat == TextureSaveFormat.Raw)
            {
                return texture.GetRawTextureData();
            }

            return null;
        }

    public static void SaveRenderTexture(this RenderTexture rt,out Texture2D result)
    {
        RenderTexture active = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D textureToSave = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false)
        {
            name = "RT Save "
        };
        textureToSave.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);
        textureToSave.Apply();
        RenderTexture.active = active;
        result = textureToSave;
        //byte[] png = textureToSave.Encode(saveFormat);
        Object.Destroy(textureToSave);
        //textureToSave = null;
        //new System.Threading.Thread(() =>
        //{
        //    PathUtil.CreateDirectoryIfNoExistsForFileName(path);
        //    File.WriteAllBytes(path, png);
        //}).Start();
    }
}
}
