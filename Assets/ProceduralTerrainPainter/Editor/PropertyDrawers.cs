// Procedural Terrain Painter by Staggart Creations http://staggart.xyz
// Copyright protected under Unity Asset Store EULA

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace sc.terrain.proceduralpainter
{
    public class PropertyDrawers
    {
        [CustomPropertyDrawer(typeof(Attributes.ResolutionDropdown))]
        public class ResolutionDropdownAttributeDrawer : PropertyDrawer
        {
            private static GUIContent[] reslist =
            {
                new GUIContent("16x16"), 
                new GUIContent("32x32"), 
                new GUIContent("64x64"), 
                new GUIContent("128x128"), 
                new GUIContent("256x256"), 
                new GUIContent("512x512"), 
                new GUIContent("1024x1024"), 
                new GUIContent("2048x2048"), 
                new GUIContent("4096x4096")
            };

            private GUIContent[] options;
            
            private static int resolution = 0;

            private void CreateOptions(int minRes, int maxRes)
            {
                List<GUIContent> contents = new List<GUIContent>();

                int max = minRes;

                while (max <= maxRes)
                {
                    contents.Add(new GUIContent(max + "x" + max));
                    max *= 2;
                }

                options = contents.ToArray();
            }

            private int ResToIndex(int resolution)
            {
                int index = 0;
                for (int i = 0; i < options.Length; i++)
                {
                    if (options[i].text.Contains(resolution.ToString())) index = i;
                }

                return index;
            }

            private int IndexToRes(int index)
            {
                string resString = options[index].text;
                
                return int.Parse(resString.Substring(0, resString.IndexOf("x")));
            }
            
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                int index = 0;
                
                Attributes.ResolutionDropdown range = attribute as Attributes.ResolutionDropdown;

                CreateOptions(range.min, range.max);

                index = ResToIndex(property.intValue);

                EditorGUI.BeginProperty(position, label, property);
                position.width = EditorGUIUtility.labelWidth + 100f;
                index = EditorGUI.Popup(position, label, index, options);
                EditorGUI.EndProperty();

                resolution = IndexToRes(index);

                property.intValue = resolution;

                property.serializedObject.ApplyModifiedProperties();
            }
        }

        [CustomPropertyDrawer(typeof(Attributes.ChannelPicker))]
        public class ChannelPickerAttributeDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);
                
                // Draw label
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

                Rect rect = position;
                rect.width = 150f;
                rect.x = position.width - 75f;
                
                property.intValue = GUI.Toolbar(rect, property.intValue , new GUIContent[] { new GUIContent("R"), new GUIContent("G"), new GUIContent("B"), new GUIContent("A") });
                
                EditorGUI.EndProperty();
            }
        }

        [CustomPropertyDrawer(typeof(Attributes.MinMaxSlider))]
        public class SliderDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                if (property.propertyType != SerializedPropertyType.Vector2) return;
                
                Attributes.MinMaxSlider range = attribute as Attributes.MinMaxSlider;

                position.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.BeginProperty(position, label, property);
                
                // Draw label
                //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
                
                var sliderRect = new Rect(position.x, position.y, 200, position.height);
                
                float minVal = property.vector2Value.x;
                float maxVal = property.vector2Value.y;

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth));

                    minVal = EditorGUILayout.FloatField(minVal, GUILayout.Width(40f));
                    EditorGUILayout.MinMaxSlider(ref minVal, ref maxVal, range.min, range.max);
                    maxVal = EditorGUILayout.FloatField(maxVal, GUILayout.Width(40f));
                }

                property.vector2Value = new Vector2(minVal, maxVal);

                EditorGUI.EndProperty();
            }
        }
    }
}