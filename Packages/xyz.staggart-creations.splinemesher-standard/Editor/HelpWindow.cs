using System;
using sc.modeling.splines.runtime;
using UnityEditor;
using UnityEngine;

namespace sc.modeling.splines.editor
{
    public class HelpWindow : EditorWindow
    {
        private static readonly int width = 530;
        private static int height = 440;
        
        #if !SM2
        [MenuItem("Help/Spline Mesher", false, 0)]
        #endif
        public static void ShowWindow()
        {
            HelpWindow editorWindow = EditorWindow.GetWindow<HelpWindow>(true, "Help", true);
            editorWindow.titleContent = new GUIContent("Help");

            #if !SM2
            height += 200;
            #endif
            
            //Open somewhat in the center of the screen
            editorWindow.position = new Rect((Screen.currentResolution.width / 2f) - (width * 0.5f), (Screen.currentResolution.height / 2f) - (height * 0.5f), (width * 2), height);

            //Fixed size
            editorWindow.maxSize = new Vector2(width, height);
            editorWindow.minSize = new Vector2(width, 200);

            //Init
            AssetInfo.VersionChecking.CheckForUpdate();

            editorWindow.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("<size=24>Spline Mesher - Standard</size>", UI.Styles.Header);

            GUILayout.Label("by Staggart Creations", UI.Styles.Footer);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            
            DrawProInfo();
            
            Color defaultColor = GUI.contentColor;

            using (new EditorGUILayout.VerticalScope(UI.Styles.Section))
            {
                #if !SM2
                DrawReviewButton();

                EditorGUILayout.Space();

                #endif
                //Version
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Asset version");

                    using (new EditorGUILayout.HorizontalScope(EditorStyles.textField))
                    {
                        if (AssetInfo.VersionChecking.UPDATE_AVAILABLE == false)
                        {
                            GUI.contentColor = UI.GreenColor;
                            EditorGUILayout.LabelField(AssetInfo.VERSION + " (Latest)");
                            GUI.contentColor = defaultColor;
                        }
                        else
                        {
                            GUI.contentColor = UI.OrangeColor;
                            EditorGUILayout.LabelField(AssetInfo.VERSION + $" (Update available)", EditorStyles.boldLabel);
                            GUI.contentColor = defaultColor;
                        }
                    }
                }

                using (new EditorGUILayout.VerticalScope())
                {
                    if (AssetInfo.VersionChecking.UPDATE_AVAILABLE)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            //GUILayout.Label("");
                            GUILayout.Space(EditorGUIUtility.labelWidth + 80f);

                            if (GUILayout.Button(new GUIContent($"  Download v{AssetInfo.VersionChecking.LATEST_AVAILABLE}", EditorGUIUtility.IconContent("Asset Store").image)))
                            {
                                AssetInfo.OpenInPackageManager();
                            }
                        }
                    }
                }

                EditorGUILayout.Space();

                UI.DrawH2("Documentation");

                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button(new GUIContent("<b><size=12>  Documentation</size></b>\n<i><size=11>  Usage instructions</size></i>", DocIcon), UI.Styles.Button))
                    {
                        Application.OpenURL(AssetInfo.DOC_URL);
                    }
                    if (GUILayout.Button(new GUIContent("<b><size=12>  FAQ/Troubleshooting</size></b>\n<i><size=11>  Common issues and solutions</size></i>", FaqIcon), UI.Styles.Button))
                    {
                        Application.OpenURL(AssetInfo.DOC_URL + "?section=troubleshooting-faq-2");
                    }
                    EditorGUILayout.EndHorizontal();
                }

                //EditorGUILayout.HelpBox("If you have any questions, or ran into issues, please get in touch.", MessageType.Info);

                EditorGUILayout.Space();

                UI.DrawH2("Support");

                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.HelpBox("  Be sure to consult the documentation, it already covers many common topics", MessageType.Info);

                    EditorGUILayout.Space();

                    //Buttons box
                    EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button(new GUIContent("<b><size=12>  Discord</size></b>\n<i><size=11>  Access support</size></i>", DiscordIcon), UI.Styles.Button))
                    {
                        Application.OpenURL(AssetInfo.DISCORD_INVITE_URL);
                    }
                    if (GUILayout.Button(new GUIContent("<b><size=12>  Forum</size></b>\n<i><size=11>  Join the discussion</size></i>", ForumIcon), UI.Styles.Button))
                    {
                        Application.OpenURL(AssetInfo.FORUM_URL);
                    }
                    if (GUILayout.Button(new GUIContent("<b><size=12>  X</size></b>\n<i><size=11>  Follow developments</size></i>", TwitterIcon), UI.Styles.Button))
                    {
                        Application.OpenURL("https://twitter.com/search?q=staggart%20creations&f=user");
                    }

                    EditorGUILayout.EndHorizontal(); //Buttons box
                }
            }
        }

        public static void DrawReviewButton()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("<b><size=12>  Leave a review</size></b>\n<i>  This goes a long way!</i>", ReviewIcon), UI.Styles.Button, GUILayout.Height(45f), GUILayout.Width(240f)))
                {
                    AssetInfo.OpenReviewsPage();
                }
                GUILayout.FlexibleSpace();
            }
        }

        public static void DrawProInfo()
        {
#if !SM2
            using (new EditorGUILayout.VerticalScope(UI.Styles.Section))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(
                            new GUIContent("<b><size=12>  Spline Mesher Pro</size></b>\n<i>  Open Asset Store</i>",
                                EditorGUIUtility.IconContent("Asset Store").image), UI.Styles.Button,
                            GUILayout.Height(45f), GUILayout.Width(240f)))
                    {
                        Application.OpenURL(
                            $"https://assetstore.unity.com/packages/slug/338468?aid=1011l7Uk8&pubref=smeditor_upgrade");

                    }

                    GUILayout.FlexibleSpace();
                }

                EditorGUILayout.LabelField("Upgrade to the Pro version for advanced features, at a discount.", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("• Fast multithreaded mesh generation, x50 faster");
                EditorGUILayout.LabelField("• Fill mesh generation.");
                EditorGUILayout.LabelField("• Segmented output");
                EditorGUILayout.LabelField("• Procedural input shapes");
                EditorGUILayout.LabelField("• MeshLOD support");
                EditorGUILayout.LabelField("• Many QoL improvements");
                
                EditorGUILayout.Space();
            }
#endif
        }
        
        private static Texture m_ReviewIcon;
        private static Texture ReviewIcon 
        { 
            get
            {
                if (m_ReviewIcon == null)
                    m_ReviewIcon = UI.CreateIcon("iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAD0ElEQVRYCb1XaWxMURT+ph2GqUpF2libNtQyQlClIQSR1JYgokSkPyxVS/1C/KA/CJFUxBaRWEIJIVEk+FGSirYSZUgItYWOEkuVGqP7tL7zdKZ3xps3ozPpSb73zj33nu/cd+57595naruDiIhpemg0ajzxMamG0Ci8o9KolRE1hOgfiP8W8397dDgcoNqN6Ff0AGUZWzCBenVHt76mPnA4GZhO+mI1xLRN2Mf2FtWm6ncPAW1tTLlJtQKdzUCuLw2QlYHMNfnYT/sn/z5P2z94ZzMwnoR2D6nnXtcAd2o2trItk/CRioK/zUhlYJMPe3vD2gPRGxYhJ/cgjtP0S29MJDKQSGKHHrnYqmvRmJiJHKqnpS1SX/T3Lle9DER1dIekrTMaFR8Hy+al2NjQhCgCAlXCzcAgkr0ieqqk/nrlZ9QlL8MS2m+qn5yM08uAXiGSAMnEYGIoMbZdl++8LxFUPtXA2b8vznDgS+I1UUW8IxoIr3i+Agst6cQ2Io6wEb2JiEprK5xcgmdEJYm/ETJBu2RgJ5W1RALRZeKqhzN2DtLkJaz+7kRLl0VuD1T1FSaqe6LLK1BeWALXwqnI6G5GdFdM5MsPNE7MwSMWL4fpQp435KQFU1Da09Lp8uwlMlJqnGgauwoPP1Rru+cq7/fKb/b+xWLMbGqG24ggnL5aF5pT12jBZb+Q984VZbUACkqulmJ2ixut4QTS8/1Vh+bJG2B3fMFX9mcTtTJOzYBWuZiJ2+dvY4EUjUhJfSPc6ethr3Bo5wWppt893FEWHil0cP1SMRZ7BoVz55K2Ts2F/XklfpBH0v5Z5TNPkrKjL4Usq8eS+mmbi/6IEKyHC+Gwv9LSLZuUrL2PmBnASGTWYUlTi/Y+yYngvR6RFCIjCZwfIy+lz5aEGDYDFrpgExiicHVKTUxADzomBXI2OhPK4owK5BiqPbk/Ynhakp1VV4wyMJIeUq/Dkrhe6JZug41l10zAH0YTSAkW+elbOF+81z//qb5DBmg7bbJq8+hGExjtGeR///kbzavz8WzMSjwamYWCvFO4wycLWMJHJMJKjgH+PNI2msA/6y8levdZvImbh7KTN/CG/rKVbdxVgBkxszH3yBXtyCa8PjIuBbE06L4HRhOIV1nO3cLHPvNRuv0kKmk/SiwkSgjsyJIrig5dxvBhK5Bzrcy32sVatW1+uDbK/yIHxwDYRntb6RF8GzoQ9+gnv2LyR2QhgknMrFTkPz6BWuHgX9MTOvzzwxKMRPqXExJ4L+GTEba90p4BpPDcLFBEWvwr1DgmK3av+gf86jED3McCCAAAAABJRU5ErkJggg==");
                return m_ReviewIcon; 
            } 
        }
		private Texture m_DocIcon;
        private Texture DocIcon 
        { 
            get
            {
                if (m_DocIcon == null)
                    m_DocIcon = UI.CreateIcon("iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAEzklEQVRYCbVXTUhjVxR+L8nExtHEn6KtadMZ7DCLQVtShC5MRYrVYkFEQUUQxYWLrkRdjGhVxFIcLUPp1k0XrlwquKiVUFroDJV2OiJjFVRSicaf4kz+TGLmO7fvPl7eezeNZXrhcs89f/fknPPd+yLv7+9LZiOdTjO2LMtmYgOP63N/DocjQ2dtbS1jzzcyN+AMZf0eDutB4/ycIngOfRv0/ZifKj5yWsjIbLyJcy3RaDR5dXVFUZjpMB4OTttstpt5eXkWMJr29vYCWN8SGugEogBeJz2v1/tzJBJ5AfIKUxjF9PT0uz09PXeXlpYCDQ0NZeFw+AT6zAfWrEMUQJSshoeHPV1dXQ/z8/MzC6pxiezsIUudYN1dX18/Gxoa+gpl/fbo6Ohv8Io0qqakDRk0E7ygtPf399+C8KGZAueRPS+R2+3OA/8R9nfKy8v/PDk5ucDeyXXNVlEGmNHc3Nx2MBi8dLlcIj1pZ2cn2tvb666vry+zWCxUpjKPx/N4YGDg/dHR0d9CoVDWIGw8el109EukhYWFIHrga5CnmIXEMxsTExPN4H+u+GK9gv75fXV1tbqpqenJ8fHxc8hN7WV0rZnPp3B2z0xwTV4QJXLAl2txcfEJbN/T24syQJCSUMNYKpWSkFq9nbqntBNcz8/PE8XFxXZcQFY0JWusRCLhQgPLRUVFUjKZdI6Pj/fA8DvVGISotqWk5PP5HqEEQZApTHEU//ghuJ5jlmDexExiSlNTU3fQI15kgkpjJZ52iAKIkNLY2Nittra21dcwtEZEwyEdgOzKeqfUdHz8BT3qJ6/CMNwlIhiG4Vjq7u72wPBL7k27kjyXwfUQCJXFgHlRBtgFMjg4+Mfh4eElaqvqAVaJyspKx+zs7D30SBwIeFZYWGi12+2GEm1vb0f6+vrcjY2NbyAQitgYAI9Q92tu0B4wOkcP3AdJV6sKI9wLdDVvxGKx5PLy8jPQDzDzMZkdVnXMzMx8gs2IwjCkTVSCEAIr29ra+giGP6nedMTFxQX1AfXLY52IbRWIf6CRGQMQZICl8/T0NK5/DamUVqtVLikpsSs3H3U/G4I7hYtNV7W2OimDYW1t7S8oQQiyDBiWlpZaNzY2WgsKClQEZDmcyiUcogAYDNFgt1tbW39Ag9m5B2QsDJqwLjmdTvv8/Lyvvb39ATKjBqPRPQZNZRQOUQ8wGHZ2dr4Ny0mRNQK4gcOrIa8WlFI1/U8wxPfA00AgEEfKDd3Nr1ulD9SDtMTm5mZ4ZGTknebm5goEeH0YrqysEAxH4TQDhppDOK4N3c11ampqPgP9hbI36IlKwGHog+GP3FkO6yVSza5oRZcakD3tSgmMAQhqxxrq4OAgjBctDdhlPRvOJbya6YqKCgdeP7qQRMOACBEKWJfjCv0VJdiFN/pV2aPANYtbsaGqqsojOt2MLwqAwXBycvJ2S0uLH4+hCkMzJ8gA/TJ6ij80k2fjiXqAwbCjo4NgOJ7NAckEZfw3MyY3vGA5Wb1CJdEnWYzOiMfjqVdwlkz/mvBJxiGb4dIGQQZD2XxcV1f3Db71KnHRxM0UrsPDXzf57OyMmtjwgwy41Dim7zpqUgN0NDrXISkAekcSWiN5d3dX3eNXS36/n+3x1aPy/0/iJfpMtnfwrhsgAAAAAElFTkSuQmCC");
				return m_DocIcon; 
            } 
        }
		private Texture m_FaqIcon;
        private Texture FaqIcon 
        { 
            get
            {
                if (m_FaqIcon == null)
                    m_FaqIcon = UI.CreateIcon("iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAFRklEQVRYCa1XXyykVxT/vjEo1qKxSUuxiSUxTUtS3T5oMgm7NmM3EtKkL0v6sjw0G2lVmj548lCLCtG0kRINb4ZGttXSbiTYJbPjT2xaFqNhsBjs6CzKjD/9nc93ufPNmD/Sk5w5/++937nnnntHnJ+fFwgSEhIkyn7MZrMgiqIkHh0dCUFBQcLBwYFAvBKY3/HxsaBSqYSdnR0hNDRUcqOYmZkZp5CRkZFTWaSJ4uLihIWFBWkRPCUvGlCtVhN7BYPdwQK04DXAGGAI8BAL2ASdwwIM8O8Bb6TFrK+vC1FRURAFobe3V6Lsp6KiQmJFcmSwuLgoLYZk4mW4hkm/AH8XE4UxpSe6u7s7igw8gI+ez0BnZ6dTWHl5uSDSFlD6+S9nMry/BH6NiU/2wincu4AMGJGBj+BpPs/bKQOcUwC25mfIOk53IRYZcKAG7iK43d0AUgbcGAz46Otu9BdWdXd3f4bgOuUAIqVeAX9AvqHQOYlra2t7JpPp1crKyj7gKCIiQp2cnHwJeNnJUSGg8D6GyikTygV8DodvFHGSaLPZHIODg+sdHR2Wvr6+LShXgCbgGtAODNfpdBmlpaU3k5KSwiG7AGpiPz09PQmG0wrnt+BNpP2FS5SsmJub287MzHwE8TGQsvSMCpjVJ9dHNKOjo0PR0dERcqgTaW1t/ROKd5iSX0ALBvuEGdxRfN1v0OfU1taSWYvjmYoYahIDQKm7yMf6NvS/kJMSDg8PjzMyMoqh/4Fs4tTUFNHwkJCQLQSpSDgPkIVX2PMfU1JSdPC9xvsNDAzcg9yUlZVF6isOh8PC23m+oaHBVFNT8y50/6qoZQLzvE1OAyQmJoZrNJr7ysnJNjk5+VVBQcH1mJgYAVk4625kVEBOTs4bUN0hNftij1WviHcRNzY29pqbm6koE8mINL/t4sQp4uPjQyFmk4otQMPZ/WKnp6dt+KJRi8WyjMBuCkZtVHsaBHbK0FXyUVG2gJQSvwEXzGp2dvYY+sIEgouANpyMh9ii9z0NhgyRWfqhCiagW80vMBqNm0VFRc8RRCejamlpKRkD94NP9XGgkwVgteR/4GOQ5EYpLCkpmYZgAFahm76HyR+Df01y8OOH1YDVjxi65/eXl5f3EVMlx9WD+j05xbIFUEv1GQICAkStVmvHmyEJ+AEC43wOhmNgYCCbV1DLR5Zuv9u+DoI2G4yWegtbccvXGN5ve3vbATmIdOwU6HkHX3m8/fyqHTbu8PDwJngbySp6MgGf2+32SebgjW5tbdkLCwvH0tLSnmArHo2Pj/tVQ01NTdQzhmkeFb12ZaQHg0/Q1dX1or+/fwGL/hTnXre6uip1NV+CabG4Lbfh+xP5q/E1LO53ME8jIyO9voRQL3R2HwL/kh+v/6Ae2DgeaVlZGR3fMeDf5KjGLUiUwU1shxXP8NMqZQae5ufnx+BFJN2GKOLXgd/x9vP4+vp60+zs7B7sNcxHHBoaYjyjN2JjY+nB4RVwNZuDg4Mj4XjZmzO1bblzfgvfTuYvNjY2Mp6nxejx3+NPhtQmecNF+J6enpXi4mJKfQ/wAT+GWn6Q8DriG6G35ubmNuMNcElp9EeWHx9LiPkV6HJLevvCq5WVlXosJDUsLCzQn4lx1jeqq6vn5YqnGnHba0R8odDW1uY0dnt7uyTX1dXRK4j4ory8vPs4829RRuRT4BRDgtVq3TcYDC/1er0FtUWNhi6oFqBU8aAucJoBZS1gz5TOYVBQ6/0QSCcgAOgOXkL5FEiFTNe1RxAnJugtcQItLbTYM+AyIOBBemb4H7n/AIKWOUx2b5M6AAAAAElFTkSuQmCC");
				return m_FaqIcon; 
            } 
        }
        private Texture m_DiscordIcon;
        private Texture DiscordIcon 
        { 
            get
            {
                if (m_DiscordIcon == null)
                    m_DiscordIcon = UI.CreateIcon("iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAElElEQVRYCb1WbUxbZRS+t7TMQfnoYMIYA8rKwkLWaSTMqH80+7FFs8wM94FfhGXZD0O2337Fjx9qdFmMxsQtatTMMZdoZh1uEswWzOLClH1RGbD2Im3pQCylXytbW59z6VvfW+6FgoyTPL3ve85z3ve85557+oq7900K85XBPz8XLOtb5nSLx6cEnS47xTv97UOpMRuICwmAOc/n6Ru/KtPPfveYwk2nmGU4oQxkIpQBJqYiq0CgA5uKNwp5BTUyljwDLCDKBAWhZ4r5PBdSA3R6vgZo84B/QMg0A/kIcBPwBNAB/AJoyXswuIBOwM5IWjUwVwA7sMAeYAuQS4vdvRPySYMnDuIr+AfTAoDq6DZwC6gDPgFkQQ0M4iv4BpPjQN+0Vvk7WwAXQW1Q0hc+QwZegncqOLaSVg28AsKibU6bFa6o+7j9ZIMDwzM0Z6KWgWIYxxhhMZ8u6UfPrx1Na7EmvTJZ1DLwLjMu9rO86qky9IBXsS5BlvRGZIJ2b9J2Tx7rrQeafX9fKQIEQnoA93RzOtEa8/ayZfcVtQACYckDEHV6sXJt4wvR2+PLAEUAFgRYq5V3x42vhn87t/9af++nkhZncqI/0N3V2tvddaA34L8Z1OKtqthyP2zyvxJfhJu1HPqufui4fPG1v2BvkwaOXzMYjHvN655t4fnhkCvSfrL+d+j6gWM3+77QPf2804Y0G3kejVcUP0AN7FGgkw+gnozpEo9FY8nNj8JGXU2otDxzAV2uEV2OWrQszhvH3Bi4gP2kSP7Nv40htWaFZGcXGKCwkpIPwKxgJSeJRJxGUUDenLs/hKFLBRCLRYl4CuCF/GZIPH4nASVlQVEDZTOYUGTpl2fVWltLAXYqotF/QykNmFRZdq7CWL4AcEHO9lXJXD4D1ANUZWPDW1bP8M+HYTwCPJh8Krj5ptq8zds6DheXbJJgEIE3gA3ArMIH8N/lLc1FFLPE1RVbD0JN0BRsXg3jFU2CioHvA/Rellz4AOT/+/QIgpOOkHuo3Zuun2se8A8G3UOn4TfzXFRXzJ9/Bc2xu5GvYeR1eJs6wevqvD4+2n02x7jm8ZWlDxfmGity9AajkickEpGwN0rNaNTTNTE+dilgrmnCRmIJNqOaSMktz3n6t80hhbiy9JGUAYPtqPaPVlc+Wc4raRwKDNlsbRvewbBOb8itN+aZLWgyhaIuS8QtKR4OuqbCITd9dl7YnY3NIy+THy+R8EjE3nPIOWA/Mgp9F/C6IrIkOb+8atubVTW7dqHwStC75dd0/syOnpHhDuoFHyR51Ezo7sBfy+iaNrF7n58oP+F89LkKwYAUcvR96bZfPuTBdAT4DKA74/SltO1ovsAyMea9QHqSanxaL1ave65pasoft/e8T86twHUyagnXA0wuydYuDZxY7pJ+8IHvBL4HbLyv4kZEgZAgAwKcGK8Qg60Adb5TTJnhMw+8PcAl4A81n/QaELgMCMiAMOmbeZnFO6bbsdp6so5lgR1Ik0gGRuZJlIH/I8kayGgJucAoCKoBVgcs/ZSB+cr0gUTVg6mtlUkNqPktmu5fRzyST440lD0AAAAASUVORK5CYII=");
                return m_DiscordIcon; 
            } 
        }
        private Texture m_ForumIcon;
        private Texture ForumIcon 
        { 
            get
            {
                if (m_ForumIcon == null)
                    m_ForumIcon = UI.CreateIcon("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");
                return m_ForumIcon; 
            } 
        }
        private Texture m_TwitterIcon;
        private Texture TwitterIcon 
        { 
            get
            {
                if (m_TwitterIcon == null)
                    m_TwitterIcon = UI.CreateIcon("iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAE5ElEQVRYCZ3VW2iWdRzA8b1LnTs4U9FVzra5bB6W6WblYaQkREE3ItGBJLyNCC8CFSHspi666EZCKUiHnYSioReeJhqIWrrNLRPncu82dWpzraZTc4e+35f3Eba9h2f+4MOz932e9////f7/3/NfJCMjIxM5mIAIgvDvobj7XG9jEA8TjuX42XC+u7gDx88owCeI4kZcD9d/4PUKfsJCPIKxhpNPwXrUowUfYSYyHfA/WN1KlCAXfj8OZj0Jfj8HDehCLHOuYcLVfRkf4yk4nyv5N9qdyA9WqmfhMrWiCZfQh2lwpVy+39GLsGHyn6EMzuHvj2IvuoMlNSsrm4F5uIUvsB2/oDTuca4XYWL2RbpwFefidYzHOdRiF65jVDjRfrg8ZlgOV2QV6mDznMBSOHiqsLhF2IMo/N2nKEIEsQhWIPhsZ3biBbhfrkoTzNYKHLAQ7utZuKSJwgmexgaswnlYgJW7vW57LEYm0M+3dn8WSjAZZn4ZUdi5JmYV3eiADTwEw4lt2mV4B69hIqL4ChYzbOtGJsD9WNM1ch3ASviDP3ENJuOWVMJ7VuK9XphEPt7ENlShDTbdbziMfxE6yniyAZ4D78PBDRv1c7j8nhubMBVWuhYu9z24OjVYB1clYSRageBBB3QbKmAyF9AOq7XquXE+Y6VPYDPmweT+wD78iKSVp0qgjx9ehWeDkxXAxrMxfU1vYjGKUQ4b16tvSjMO4nv4XNJIlYB76kSeEQ4+Gw7eCCsykSwsgfeehA3Zglp8A5vXcZJGqgT8kZPbA4/CiXy1/OwkJuGB5JsxB07k94fwHdymAaSMdAn4Y6t27518PmbB/e2EFXt9Bo/BV/NbnIbJp40wCViZE9l8z6MU41AHT0wbzsm8Z5/4nPe8po0wCThIP64hggqUwS1wJbx2IA9uk/c8zM7D1UsZYRNwEAfzrbDzfSuKYA+4PTarveE2mYAN2QwTM/mkMZYEPAGr8CpsvCmwavfbV80lt3LfmGJ4zxPQbUoaYRPIYoTl+BCVsLvHw8Yz6uFWuE2DWApfTatvRB8SRpgEbLiF8JR7EW5FK9wOE3Ar/kILTKITNqNvhvfcBp/3f8qoSJdAJr+wki14BVZ0EUdQg0KUYjpcheuwH7rgSvmfcyrOwO+GMCzSJWAlG7EGJuPhcgq7cBL+M6qCSebiV/haOpnXFbAh7Y1zGLUVqRKwqg+wDjaUy+j7/TUacAcu/QRYrUm4PU5kQ5rcRLgV9kwtTGRYJEsgn6es+l24pDaXe7wTrkBwyvlv1y1xYA+iJTDxswj6wt8vg8l4bpj4g0iUQA53V+M93EcHfkA1rNwqg3BPrdatuQ23w4Z1O+x+G9UJ34AJ9uMCHiQxMgGX0we3YgHacBA1cBIHSBR9fOlZUIr5mIXLuAR/4/ZUoBBBYgP8PSxMxuwPwAwdcA8cMBPpwtfVsyLoj2P8vRieIc/hONyqn2GisQhWIMKnYmzFSzA7l8pX6yhc5nQxyAOeej1wFa26AM24AYvy+xI4n81qf8Q++HA1HMC9tIrtKIeVhQ0HLsIOdMNJbeCO+NXmtTiTegt5rsA0vI21yEY7mvAlzNJGHEu4Wm1wO2ciPy6Hq03rSvlaz0CTCdyDzeL+eL63YjdOwwrGGk7ShRPwbxvUVbgCVyIavzp29v/0qmv/9is4ZAAAAABJRU5ErkJggg==");
                return m_TwitterIcon; 
            } 
        }

    }
}