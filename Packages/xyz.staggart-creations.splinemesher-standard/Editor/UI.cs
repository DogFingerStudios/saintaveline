using System;
using sc.modeling.splines.runtime;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace sc.modeling.splines.editor
{
    public static class UI
    {
        public static readonly Color RedColor = new Color(1f, 0.31f, 0.34f);
        public static readonly Color OrangeColor= new Color(1f, 0.68f, 0f);
        public static readonly Color GreenColor = new Color(0.33f, 1f, 0f);
        public static readonly Color LightBlueColor = new Color(0.4f, 0.6f, 1f); 
        public static readonly Color LighterBlueColor = new Color(0.6f, 0.8f, 1f); 
        
        public static Texture CreateIcon(string data)
        {
            byte[] bytes = System.Convert.FromBase64String(data);

            Texture2D icon = new Texture2D(32, 32, TextureFormat.RGBA32, false, false);
            icon.LoadImage(bytes, true);
            
            return icon;
        }

        public static void DrawHeader()
        {
            Rect rect = EditorGUILayout.GetControlRect();
            rect.x -= 30f;

            //Draw title
            GUIContent textContent = new GUIContent($"{AssetInfo.ASSET_NAME}");
            Vector2 titleSize = EditorStyles.boldLabel.CalcSize(textContent);
            
            Rect textRect = new Rect(rect.x + 10f, rect.y, titleSize.x, titleSize.y);
            GUI.Label(textRect, textContent, EditorStyles.boldLabel);
            float totalWidth = textRect.width + 13f;
            
            //Draw icon to the right
            Rect iconRect = new Rect(rect.x + totalWidth, rect.y + 2f, Icons.Edition.width, Icons.Edition.height);
            GUI.DrawTexture(iconRect, Icons.Edition, ScaleMode.ScaleToFit);
            totalWidth += iconRect.width + 2f;
            
            //Version
            GUIContent version = new GUIContent($"{AssetInfo.VERSION} " + (!AssetInfo.VersionChecking.UPDATE_AVAILABLE ? "(latest)" : string.Empty));
            float versionWidth = EditorStyles.miniLabel.CalcSize(version).x;

            Rect versionRect = new Rect(rect.x + totalWidth, rect.y, versionWidth, titleSize.y);
            totalWidth += versionRect.width;
            GUI.Label(versionRect, version, EditorStyles.miniLabel);

            if (AssetInfo.VersionChecking.UPDATE_AVAILABLE)
            {
                GUIContent update = new GUIContent($" Update to {AssetInfo.VersionChecking.LATEST_AVAILABLE}", EditorGUIUtility.IconContent("d_Package Manager").image);

                GUIStyle linkStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                linkStyle.normal.textColor = Color.Lerp(LightBlueColor, LightBlueColor * 1.4f,
                    Mathf.Sin((float)EditorApplication.timeSinceStartup * 5f) * 0.5f + 0.5f);
                linkStyle.hover.textColor = LighterBlueColor;
    
                float updateWidth = linkStyle.CalcSize(update).x;
    
                Rect updateRect = new Rect(rect.x + (-updateWidth * 0.5f) + totalWidth * 0.5f, rect.y + titleSize.y + 1f, updateWidth, titleSize.y);
    
                //Make it clickable
                EditorGUIUtility.AddCursorRect(updateRect, MouseCursor.Link);
    
                if (GUI.Button(updateRect, update, linkStyle))
                {
                    AssetInfo.OpenInPackageManager();
                }
            }
        }
        
        public class Section
        {
            public bool Expanded
            {
                get => SessionState.GetBool(id, false);
                set => SessionState.SetBool(id, value);
            }
            public readonly AnimBool anim;

            private readonly string id;
            private readonly GUIContent title;

            public Section(Editor owner, string id, GUIContent title)
            {
                this.id = $"SM_{id}_SECTION";
                this.title = title;

                anim = new AnimBool(true);
                anim.valueChanged.AddListener(owner.Repaint);
                anim.speed = 12f;
                anim.target = Expanded;
            }
                
            public void DrawHeader(Action clickAction)
            {
                UI.DrawSectionHeader(title, Expanded, clickAction);
                anim.target = Expanded;
            }
        }
        
        public static void DrawH2(string text)
        {
            Rect backgroundRect = EditorGUILayout.GetControlRect();
            backgroundRect.height = 25f;
            
            var labelRect = backgroundRect;

            // Background rect should be full-width
            backgroundRect.xMin = 0f;

            // Background
            float backgroundTint = (EditorGUIUtility.isProSkin ? 0.1f : 1f);
            EditorGUI.DrawRect(backgroundRect, new Color(backgroundTint, backgroundTint, backgroundTint, 0.2f));

            // Title
            EditorGUI.LabelField(labelRect, new GUIContent(text), Styles.H2);
            
            EditorGUILayout.Space(backgroundRect.height * 0.5f);
        }

        public static void DrawSplitter(bool isBoxed = false)
        {
            var rect = GUILayoutUtility.GetRect(1f, 1f);
            float xMin = rect.xMin;

            // Splitter rect should be full-width
            rect.xMin = 0f;
            rect.width += 4f;
            
            if (isBoxed)
            {
                rect.xMin = xMin == 7.0f ? 4.0f : EditorGUIUtility.singleLineHeight;
                rect.width -= 1;
            }

            if (Event.current.type != EventType.Repaint)
                return;

            EditorGUI.DrawRect(rect, !EditorGUIUtility.isProSkin
                ? new Color(0.6f, 0.6f, 0.6f, 1.333f)
                : new Color(0.12f, 0.12f, 0.12f, 1.333f));
        }

        private const float HeaderHeight = 22f;
        public static bool DrawSectionHeader(GUIContent content, bool isExpanded, Action clickAction)
        {
            DrawSplitter();

            Rect backgroundRect = GUILayoutUtility.GetRect(1f, HeaderHeight);
            
            var labelRect = backgroundRect;
            labelRect.xMin += 8f;
            labelRect.xMax -= 20f + 16 + 5;

            var foldoutRect = backgroundRect;
            foldoutRect.xMin -= 8f;
            foldoutRect.y += 0f;
            foldoutRect.width = HeaderHeight;
            foldoutRect.height = HeaderHeight;

            // Background rect should be full-width
            backgroundRect.xMin = 0f;
            backgroundRect.width += 4f;

            // Background
            float backgroundTint = (EditorGUIUtility.isProSkin ? 0.1f : 1f);
            if (backgroundRect.Contains(Event.current.mousePosition)) backgroundTint *= EditorGUIUtility.isProSkin ? 1.5f : 0.9f;
            
            EditorGUI.DrawRect(backgroundRect, new Color(backgroundTint, backgroundTint, backgroundTint, 0.2f));

            // Title
            EditorGUI.LabelField(labelRect, content, EditorStyles.boldLabel);

            // Foldout
            GUI.Label(foldoutRect, new GUIContent(isExpanded ? "−" : "≡"), EditorStyles.boldLabel);
            
            // Handle events
            var e = Event.current;

            if (e.type == EventType.MouseDown)
            {
                if (backgroundRect.Contains(e.mousePosition))
                {
                    if (e.button == 0)
                    {
                        isExpanded = !isExpanded;
                        if(clickAction != null) clickAction.Invoke();
                    }

                    e.Use();
                }
            }
            
            return isExpanded;
        }

        public static class Icons
        {
            public static string prefix => EditorGUIUtility.isProSkin ? "d_" : string.Empty;

            private static Texture2D LoadFromResources(string path)
            {
                string absolutePath = $"{SplineMesher.kPackageRoot}/Editor/Resources/{path}";
                Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(absolutePath);

                if (!icon)
                {
                    Debug.LogError($"Failed to load icon {absolutePath}");
                    return EditorGUIUtility.IconContent("_Help").image as Texture2D;
                }

                return icon;
            }

            private static Texture2D LoadNative(string path) => EditorGUIUtility.IconContent(path).image as Texture2D;

            public static Texture2D LoadFromData(string data, int size = 32)
            {
                byte[] bytes = Convert.FromBase64String(data);

                Texture2D icon = new Texture2D(size, size, TextureFormat.RGBA32, false, false);
                icon.LoadImage(bytes, true);

                return icon;
            }

            private static Texture2D _Edition;
            public static Texture2D Edition => _Edition = LoadFromResources("spline-mesher-edition-icon.psd");
        }

        public static class Styles
        {
            private static GUIStyle _Section;
            public static GUIStyle Section
            {
                get
                {
                    if (_Section == null)
                    {
                        _Section = new GUIStyle()
                        {
                            margin = new RectOffset(0, 0, -5, 5),
                            padding = new RectOffset(10, 10, 5, 5),
                            clipping = TextClipping.Clip,
                            wordWrap = true
                        };
                    }

                    return _Section;
                }
            }
            
            private static GUIStyle _H2;
            public static GUIStyle H2
            {
                get
                {
                    if (_H2 == null)
                    {
                        _H2 = new GUIStyle(GUI.skin.label)
                        {
                            richText = true,
                            alignment = TextAnchor.MiddleLeft,
                            wordWrap = true,
                            fontSize = 14,
                            fontStyle = FontStyle.Bold,
                            padding = new RectOffset(10, 0, 0, 0)
                        };
                    }

                    return _H2;
                }
            }
            
            private static GUIStyle _Button;
            public static GUIStyle Button
            {
                get
                {
                    if (_Button == null)
                    {
                        _Button = new GUIStyle(GUI.skin.button)
                        {
                            alignment = TextAnchor.MiddleLeft,
                            stretchWidth = true,
                            richText = true,
                            wordWrap = true,
                            padding = new RectOffset()
                            {
                                left = 14,
                                right = 14,
                                top = 8,
                                bottom = 8
                            }
                        };
                    }

                    return _Button;
                }
            }
            
            private static GUIStyle _UpdateText;
            public static GUIStyle UpdateText
            {
                get
                {
                    if (_UpdateText == null)
                    {
                        _UpdateText = new GUIStyle("Button")
                        {
                            //fontSize = 10,
                            alignment = TextAnchor.MiddleLeft,
                            stretchWidth = false,
                        };
                    }

                    return _UpdateText;
                }
            }

            private static GUIStyle _Header;
            public static GUIStyle Header
            {
                get
                {
                    if (_Header == null)
                    {
                        _Header = new GUIStyle(GUI.skin.label)
                        {
                            richText = true,
                            alignment = TextAnchor.MiddleCenter,
                            wordWrap = true,
                            fontSize = 18,
                            fontStyle = FontStyle.Normal
                        };
                    }

                    return _Header;
                }
            }
            
            private static GUIStyle _Footer;
            public static GUIStyle Footer
            {
                get
                {
                    if (_Footer == null)
                    {
                        _Footer = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                        {
                            richText = true,
                            alignment = TextAnchor.MiddleCenter,
                            wordWrap = true,
                            fontSize = 12
                        };
                    }

                    return _Footer;
                }
            }
        }
    }
}