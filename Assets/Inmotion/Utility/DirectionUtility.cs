using UnityEngine;

namespace InMotion.Utilities
{
    public static class DirectionUtility
    {
        public static (string, Vector2Int) DefineDirection(MotionDirections dirMode, int idx)
        {
            if (dirMode == MotionDirections.Simple)
            {
                return ("Right", new Vector2Int(1, 0));
            }

            else if (dirMode == MotionDirections.Platformer)
            {
                switch (idx)
                {
                    case 0:
                        return ("Right", new Vector2Int(1, 0));
        
                    case 1:
                        return ("Left", new Vector2Int(-1, 0));
                }
            }

            else if (dirMode == MotionDirections.FourDirectional)
            {
                switch (idx)
                {
                    case 0:
                        return ("Right", new Vector2Int(1, 0));
        
                    case 1:
                        return ("Up", new Vector2Int(0, 1));
        
                    case 2:
                        return ("Left", new Vector2Int(-1, 0));
        
                    case 3:
                        return ("Down", new Vector2Int(0, -1));
                }
            }

            else
            {
                switch (idx)
                {
                    case 0:
                        return ("Right-up", new Vector2Int(1, 1));
        
                    case 1:
                        return ("Up", new Vector2Int(0, 1));
        
                    case 2:
                        return ("Left-up", new Vector2Int(-1, 1));
        
                    case 3:
                        return ("Left", new Vector2Int(-1, 0));
        
                    case 4:
                        return ("Left-down", new Vector2Int(-1, -1));
        
                    case 5:
                        return ("Down", new Vector2Int(0, -1));
        
                    case 6:
                        return ("Right-down", new Vector2Int(1, -1));
        
                    case 7:
                        return ("Right", new Vector2Int(1, 0));
                }
            }

            return new ("Right", new Vector2Int(1, 0));
        }

        public static int DefineDirectionIndex(Vector2Int direction)
        {
            int x = direction.x;
            int y = direction.y;

            switch (x, y)
            {
                case (1, 1):
                    return 0;
                
                case (0, 1):
                    return 1;
                
                case (-1, 1):
                    return 2;
                
                case (-1, 0):
                    return 3;
                
                case (-1, -1):
                    return 4;

                case (0, -1):
                    return 5;
                
                case (1, -1):
                    return 6;

                case (1, 0):
                    return 7;       
            }   

            return 0;       
        }
    }
}
