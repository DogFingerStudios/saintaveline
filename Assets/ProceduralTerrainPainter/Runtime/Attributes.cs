using UnityEngine;

namespace sc.terrain.proceduralpainter
{
    public class Attributes
    {
        public class ResolutionDropdown : PropertyAttribute
        {
            public int min;
            public int max;
            
            public ResolutionDropdown(int min, int max)
            {
                this.min = min;
                this.max = max;
            }
        }
        
        public class MinMaxSlider : PropertyAttribute
        {
            public float min;
            public float max;

            public MinMaxSlider(float min, float max)
            {
                this.min = min;
                this.max = max;
            }

        }
        
        public class ChannelPicker : PropertyAttribute
        {

        }
    }
}