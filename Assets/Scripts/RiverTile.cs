using UnityEngine;
using UnityEngine.Tilemaps;

namespace WorldMapGen
{
    // Tile containing a section of a river
    public class RiverTile : UnityEngine.Tilemaps.Tile
    {
        // Binary flags for each direction
        [System.Flags]
        public enum Directions
        {
            Up =    1,
            Down =  1 << 1,
            Left =  1 << 2,
            Right = 1 << 3
        }

        // Parameters used for world generation
        public static MapParameters Parameters { get; set; }

        // Metres above sea level (average of surrounding Tiles)
        public float Elevation { get; private set; }

        // Directions in which this RiverTile is connected to other RiverTiles
        public Directions Connections { get; set; }

        // Return whether the Tile at the given position is an ocean tile
        // Automatically wrap position on wrapping dimensions
        protected static bool CheckOcean(Vector3Int position, ITilemap tilemap)
        {
            // position is outside map on X
            if (position.x < 0 || position.x >= Parameters.Width)
            {
                // Wrap the coordinate
                if (Parameters.WrapX) position.x %= Parameters.Width;
                // Not on map
                else return false;
            }

            // position is outside map on Y
            if (position.y < 0 || position.x >= Parameters.Height)
            {
                if (Parameters.WrapY) position.y %= Parameters.Height;
                else return false;
            }

            return tilemap.GetTile<Tile>(position).Elevation < 0.0f;
        }

        // Set the correct sprite for a tile connected in 1 direction
        // The Sprite arguments should show variations for the same direction
        // depending on adjacent ocean tiles
        // checkOceanLeft and checkOceanRight specify offsets to use when
        // checking for ocean tiles
        protected virtual void Set1ConnectionSprite(
            Vector3Int position, Vector2Int checkOceanLeft,
            Vector2Int checkOceanRight, ITilemap tilemap, Sprite source,
            Sprite mouthStraight, Sprite mouthLeft, Sprite mouthRight)
        {
            // Is there an ocean tile on the left bank?
            Vector3Int checkOceanPos = new Vector3Int(
                position.x + checkOceanLeft.x,
                position.y + checkOceanLeft.y, 0);
            bool leftOcean = CheckOcean(checkOceanPos, tilemap);

            // Is there an ocean tile on the right bank?
            checkOceanPos = new Vector3Int(
                position.x + checkOceanLeft.x,
                position.y + checkOceanLeft.y, 0);
            bool rightOcean = CheckOcean(checkOceanPos, tilemap);

            if (leftOcean && rightOcean)
            {
                // Straight river mouth
                sprite = mouthStraight;
            }
            else if (leftOcean)
            {
                // River mouth turning left
                sprite = mouthLeft;
            }
            else if (rightOcean)
            {
                // River mouth turning right
                sprite = mouthRight;
            }
            else
            {
                // River source
                sprite = source;
            }
        }

        // Set the correct sprite for a tile connected in 2 adjacent directions
        // The Sprite arguments should show variations for the same directions
        // depending on whether an ocean tile is adjacent
        // checkOcean specifies offset to use when checking for ocean tile
        protected virtual void SetLSprite(
            Vector3Int position, Vector2Int checkOcean, ITilemap tilemap,
            Sprite bend, Sprite mouth)
        {
            Vector3Int checkOceanPos = new Vector3Int(
                position.x + checkOcean.x, position.y + checkOcean.y, 0);
            bool ocean = CheckOcean(checkOceanPos, tilemap);
            if (ocean)
            {
                // Two rivers' mouth
                sprite = mouth;
            }
            else
            {
                // River bend
                sprite = bend;
            }
        }

        public override void RefreshTile(Vector3Int position, ITilemap tilemap)
        {
            // Set the correct sprite based on which connections the tile has
            switch (Connections)
            {
                // Connection in 1 direction
                case Directions.Up:
                    Set1ConnectionSprite(
                        position, Vector2Int.zero,
                        new Vector2Int(-1, 0), tilemap,
                        Parameters.RiverSourceUpSprite,
                        Parameters.RiverMouthStraightUpSprite,
                        Parameters.RiverMouthLeftUpSprite,
                        Parameters.RiverMouthRightUpSprite);
                    break;
                case Directions.Left:
                    Set1ConnectionSprite(
                        position, new Vector2Int(0, -1),
                        Vector2Int.zero, tilemap,
                        Parameters.RiverSourceLeftSprite,
                        Parameters.RiverMouthStraightLeftSprite,
                        Parameters.RiverMouthLeftLeftSprite,
                        Parameters.RiverMouthRightLeftSprite);
                    break;
                case Directions.Down:
                    Set1ConnectionSprite(
                        position, new Vector2Int(-1, -1),
                        new Vector2Int(0, -1), tilemap,
                        Parameters.RiverSourceDownSprite,
                        Parameters.RiverMouthStraightDownSprite,
                        Parameters.RiverMouthLeftDownSprite,
                        Parameters.RiverMouthRightDownSprite);
                    break;
                case Directions.Right:
                    Set1ConnectionSprite(
                        position, new Vector2Int(-1, 0),
                        new Vector2Int(-1, -1), tilemap,
                        Parameters.RiverSourceRightSprite,
                        Parameters.RiverMouthStraightRightSprite,
                        Parameters.RiverMouthLeftRightSprite,
                        Parameters.RiverMouthRightRightSprite);
                    break;

                // 2 connections in L shape
                case Directions.Right | Directions.Up:
                    SetLSprite(
                        position, new Vector2Int(-1, 0), tilemap,
                        Parameters.RiverLRightUpSprite,
                        Parameters.RiverMouthLRightUpSprite);
                    break;
                case Directions.Up | Directions.Left:
                    SetLSprite(
                        position, Vector2Int.zero, tilemap,
                        Parameters.RiverLUpLeftSprite,
                        Parameters.RiverMouthLUpLeftSprite);
                    break;
                case Directions.Left | Directions.Down:
                    SetLSprite(
                        position, new Vector2Int(0, -1), tilemap,
                        Parameters.RiverLLeftDownSprite,
                        Parameters.RiverMouthLLeftDownSprite);
                    break;
                case Directions.Down | Directions.Right:
                    SetLSprite(
                        position, new Vector2Int(-1, -1), tilemap,
                        Parameters.RiverLDownRightSprite,
                        Parameters.RiverMouthLDownRightSprite);
                    break;

                // 2 opposite connections
                case Directions.Up | Directions.Down:
                    sprite = Parameters.RiverStraightVerticalSprite;
                    break;
                case Directions.Left | Directions.Right:
                    sprite = Parameters.RiverStraightHorizontalSprite;
                    break;

                // 3 connections in T shape
                case Directions.Left | Directions.Down | Directions.Right:
                    sprite = Parameters.RiverTDownSprite;
                    break;
                case Directions.Down | Directions.Right | Directions.Up:
                    sprite = Parameters.RiverTRightSprite;
                    break;
                case Directions.Right | Directions.Up | Directions.Left:
                    sprite = Parameters.RiverTUpSprite;
                    break;
                case Directions.Up | Directions.Left | Directions.Down:
                    sprite = Parameters.RiverTLeftSprite;
                    break;

                // 4 connections in cross shape
                case Directions.Up | Directions.Down |
                     Directions.Left | Directions.Right:
                    sprite = Parameters.RiverCrossSprite;
                    break;

                default:
                    sprite = null;
                    break;
            }

            base.RefreshTile(position, tilemap);
        }
    }
}