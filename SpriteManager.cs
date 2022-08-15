using System.Reflection;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ResourceHelper
{

    // code from Flowaria-ExtraEnemyCustomization
    // https://github.com/GTFO-Modding/ExtraEnemyCustomization
    public class SpriteManager
    {
        public static string BaseSpritePath { get; private set; }

        private static readonly Dictionary<string, Texture2D> _textureCache = new();
        private static readonly Dictionary<string, Sprite> _spriteCache = new();
        
        public static void Initialize()
        {
            string BasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); 
            BaseSpritePath = Path.Combine(BasePath, "icons");

            var files = Directory.GetFiles(BaseSpritePath, "*.png", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                TryCacheTexture2D(file,true);
            }


            //Load extra icons from custom rundown path
            if(MTFOUtil.IsLoaded)
            {
                string ExtraPath = Path.Combine(MTFOUtil.CustomPath, "ExtraResourceHelper");

                if (!Directory.Exists(ExtraPath))
                {
                    Directory.CreateDirectory(ExtraPath);
                }
                else
                {
                    var extra_files = Directory.GetFiles(ExtraPath, "*.png", SearchOption.AllDirectories);
                    foreach (var file in extra_files)
                    {
                        TryCacheTexture2D(file,false);
                    }
                }
            }
            
        }

        public static void TryCacheTexture2D(string file,bool alsoCacheSprite)
        {
            if (!File.Exists(file))
                return;
            
            
            var fileNameWOExt = Path.GetFileNameWithoutExtension(file);
            
            if(!Res_Manager.InitializeResData(fileNameWOExt))
            {
                return;
            }
            string fileName = fileNameWOExt.ToLower();
            if (!_textureCache.ContainsKey(fileName))
            {
                var fileData = File.ReadAllBytes(file);
                var texture2D = new Texture2D(2, 2);
                if (!ImageConversion.LoadImage(texture2D, fileData))
                    return;
                
                texture2D.name = fileName;
                texture2D.hideFlags = HideFlags.HideAndDontSave;

                Logs.Debug($"GeneratedTexture: {fileName}");
                _textureCache.Add(fileName, texture2D);
                if(alsoCacheSprite)
                {
                    SpriteManager.GenerateSprite(fileName, 128.0f);
                }
                
                
            }
        }

        public static Sprite GenerateSprite(string fileName, float pixelsPerUnit = 128.0f)
        {
          
            // SetFilenameFormat(ref fileName);

            if (!_textureCache.TryGetValue(fileName, out var texture2D))
                return null;

            var spriteKey = GetSpriteKey(fileName, pixelsPerUnit);
            if (_spriteCache.TryGetValue(spriteKey, out var sprite))
                return sprite;

            var newSprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            newSprite.name = spriteKey;
            newSprite.hideFlags = HideFlags.HideAndDontSave;

            Logs.Debug($"GeneratedSprite: {spriteKey}");
            _spriteCache.Add(spriteKey, newSprite);

            return newSprite;
        }

        public static bool TryGetSpriteCache(string fileName, float pixelsPerUnit, out Sprite sprite)
        {
            // SetFilenameFormat(ref fileName);
            return _spriteCache.TryGetValue(GetSpriteKey(fileName, pixelsPerUnit), out sprite);
        }

        private static string GetSpriteKey(string name, float pixelsPerUnit)
        {
            var spriteKey = $"{name}__{pixelsPerUnit:0.##}";
            return spriteKey;
        }

        // private static void SetFilenameFormat(ref string fileName)
        // {
        //     fileName = fileName.ToLower();

        //     if (Path.HasExtension(fileName))
        //         fileName = Path.GetFileNameWithoutExtension(fileName);
        // }
    }
}