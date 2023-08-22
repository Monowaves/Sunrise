using UnityEngine;

namespace InMotion.EditorOnly.Data.Error
{
    public class InMotionErrorData
    {
        public Color32 Color { get; set; }

        public InMotionErrorData()
        {
            GenerateRandomColor();
        }

        private void GenerateRandomColor()
        {
            Color = new Color32((byte)Random.Range(175, 205), (byte)Random.Range(40, 95), (byte)Random.Range(15, 55), 255);
        }
    }
}
