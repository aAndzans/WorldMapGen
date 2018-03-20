using UnityEngine;

namespace WorldMapGen
{
    // Class containing Simplex noise functions
    // This code is based on Stefan Gustavson's Java implementation:
    // http://staffwww.itn.liu.se/~stegu/simplexnoise/simplexnoise.pdf
    public static class SimplexNoise
    {
        // Gradient array for 2D and 3D noise
        private static readonly Vector3[] grad3 =
        {
            new Vector3(1, 1, 0), new Vector3(-1, 1, 0),
            new Vector3(1, -1, 0), new Vector3(-1, -1, 0),
            new Vector3(1, 0, 1), new Vector3(-1, 0, 1),
            new Vector3(1, 0, -1), new Vector3(-1, 0, -1),
            new Vector3(0, 1, 1), new Vector3(0, -1, 1),
            new Vector3(0, 1, -1), new Vector3(0, -1, -1)
        };

        // Gradient array for 4D noise
        private static readonly int[,] grad4 =
        {
            {0, 1, 1, 1}, {0, 1, 1, -1}, {0, 1, -1, 1}, {0, 1, -1, -1},
            {0, -1, 1, 1}, {0, -1, 1, -1}, {0, -1, -1, 1}, {0, -1, -1, -1},
            {1, 0, 1, 1}, {1, 0, 1, -1}, {1, 0, -1, 1}, {1, 0, -1, -1},
            {-1, 0, 1, 1}, {-1, 0, 1, -1}, {-1, 0, -1, 1}, {-1, 0, -1, -1},
            {1, 1, 0, 1}, {1, 1, 0, -1}, {1, -1, 0, 1}, {1, -1, 0, -1},
            {-1, 1, 0, 1}, {-1, 1, 0, -1}, {-1, -1, 0, 1}, {-1, -1, 0, -1},
            {1, 1, 1, 0}, {1, 1, -1, 0}, {1, -1, 1, 0}, {1, -1, -1, 0},
            {-1, 1, 1, 0}, {-1, 1, -1, 0}, {-1, -1, 1, 0}, {-1, -1, -1, 0}
        };

        // Permutation array
        private static readonly int[] p =
        {
            151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7,
            225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6,
            148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35,
            11, 32, 57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171,
            168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158,
            231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55,
            46, 245, 40, 244, 102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73,
            209, 76, 132, 187, 208, 89, 18, 169, 200, 196, 135, 130, 116, 188,
            159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250,
            124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206,
            59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213, 119,
            248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
            129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185,
            112, 104, 218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12,
            191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107, 49, 192,
            214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45,
            127, 4, 150, 254, 138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243,
            141, 128, 195, 78, 66, 215, 61, 156, 180
        };

        // To remove the need for index wrapping, the permutation table length
        // is doubled
        private static readonly int[] perm = new int[512];

        // Constants for skewing to and from the simplex grid
        private static readonly float
            skew2D, unskew2D, skew3D, unskew3D, skew4D, unskew4D;

        // A lookup table to traverse the simplex around a given point in 4D
        // Details can be found where this table is used in the 4D noise method
        private static readonly int[,] simplex =
        {
            {0, 1, 2, 3}, {0, 1, 3, 2}, {0, 0, 0, 0}, {0, 2, 3, 1},
            {0, 0, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0}, {1, 2, 3, 0},
            {0, 2, 1, 3}, {0, 0, 0, 0}, {0, 3, 1, 2}, {0, 3, 2, 1},
            {0, 0, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0}, {1, 3, 2, 0},
            {0, 0, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0},
            {0, 0, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0},
            {1, 2, 0, 3}, {0, 0, 0, 0}, {1, 3, 0, 2}, {0, 0, 0, 0},
            {0, 0, 0, 0}, {0, 0, 0, 0}, {2, 3, 0, 1}, {2, 3, 1, 0},
            {1, 0, 2, 3}, {1, 0, 3, 2}, {0, 0, 0, 0}, {0, 0, 0, 0},
            {0, 0, 0, 0}, {2, 0, 3, 1}, {0, 0, 0, 0}, {2, 1, 3, 0},
            {0, 0, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0},
            {0, 0, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0},
            {2, 0, 1, 3}, {0, 0, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0},
            {3, 0, 1, 2}, {3, 0, 2, 1}, {0, 0, 0, 0}, {3, 1, 2, 0},
            {2, 1, 0, 3}, {0, 0, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0},
            {3, 1, 0, 2}, {0, 0, 0, 0}, {3, 2, 0, 1}, {3, 2, 1, 0}
        };

        static SimplexNoise()
        {
            // Double the permutation table length
            for (int i = 0; i < 512; i++) perm[i] = p[i & 255];

            skew2D = 0.5f * (Mathf.Sqrt(3.0f) - 1.0f);
            unskew2D = (3.0f - Mathf.Sqrt(3.0f)) / 6.0f;

            skew3D = 1.0f / 3.0f;
            unskew3D = 1.0f / 6.0f;

            skew4D = (Mathf.Sqrt(5.0f) - 1.0f) / 4.0f;
            unskew4D = (5.0f - Mathf.Sqrt(5.0f)) / 20.0f;
        }

        // Return the dot product between the vector at the given index in
        // grad4 and the given values
        private static float Dot(int g, float x, float y, float z, float w)
        {
            return grad4[g, 0] * x + grad4[g, 1] * y + grad4[g, 2] * z +
                grad4[g, 3] * w;
        }

        // 2D simplex noise
        public static float Noise2D(float x, float y)
        {
            // Simplex corners in (x,y) coords
            Vector2[] corners = new Vector2[3];

            // Skew the input space to determine which simplex cell we're in
            float offset = (x + y) * skew2D;
            // Cell origin in (i,j) coords
            Vector2Int skewedCell = new Vector2Int(
                Mathf.FloorToInt(x + offset), Mathf.FloorToInt(y + offset));

            // Unskew the cell origin back to (x,y) space
            offset = (skewedCell.x + skewedCell.y) * unskew2D;
            // The x,y distances from the cell origin
            corners[0].x = x - skewedCell.x + offset;
            corners[0].y = y - skewedCell.y + offset;

            // For the 2D case, the simplex shape is an equilateral triangle
            // Determine which simplex we are in

            // Offsets for simplex corners in (i,j) coords
            Vector2Int[] skewedOffsets = new Vector2Int[3];
            // First corner's offsets are always (0,0)
            skewedOffsets[0] = Vector2Int.zero;
            // Second corner
            if (corners[0].x > corners[0].y)
            {
                // Lower triangle, XY order: (0,0)->(1,0)->(1,1)
                skewedOffsets[1] = Vector2Int.right;
            }
            else
            {
                // Upper triangle, YX order: (0,0)->(0,1)->(1,1)
                skewedOffsets[1] = Vector2Int.up;
            }
            // Last corner's offsets are always (1,1)
            skewedOffsets[2] = Vector2Int.one;

            // A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
            // a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y),
            // where c = (3-sqrt(3))/6

            // Offsets for middle corner in (x,y) unskewed coords
            corners[1] = corners[0] - skewedOffsets[1];
            corners[1] = new Vector2(corners[1].x + unskew2D,
                                     corners[1].y + unskew2D);
            // Offsets for last corner in (x,y) unskewed coords
            float lastCornerOffset = 2.0f * unskew2D - 1.0f;
            corners[2] = new Vector2(corners[0].x + lastCornerOffset,
                                     corners[0].y + lastCornerOffset);

            skewedCell.x &= 255;
            skewedCell.y &= 255;

            // Sum of noise contributions from the three corners
            float noise = 0.0f;

            // Add up the contribution from the three corners
            for (int i = 0; i < 3; i++)
            {
                float t = 0.5f - corners[i].x * corners[i].x -
                                 corners[i].y * corners[i].y;
                if (t >= 0.0f)
                {
                    // Corner's hashed gradient index
                    int gi =
                        perm[skewedCell.x + skewedOffsets[i].x +
                             perm[skewedCell.y + skewedOffsets[i].y]] % 12;
                    t *= t;
                    noise += t * t * Vector2.Dot(grad3[gi], corners[i]);
                }
            }

            // The result is scaled to return values in the interval [0,1]
            return 35.0f * noise + 0.5f;
        }

        // 3D simplex noise
        public static float Noise3D(float x, float y, float z)
        {
            // Simplex corners in (x,y,z) coords
            Vector3[] corners = new Vector3[4];

            // Skew the input space to determine which simplex cell we're in
            float offset = (x + y + z) * skew3D;
            // Cell origin in (i,j,k) coords
            Vector3Int skewedCell = new Vector3Int(
                Mathf.FloorToInt(x + offset), Mathf.FloorToInt(y + offset),
                Mathf.FloorToInt(z + offset));

            // Unskew the cell origin back to (x,y,z) space
            offset = (skewedCell.x + skewedCell.y + skewedCell.z) * unskew3D;
            // The x,y,z distances from the cell origin
            corners[0].x = x - skewedCell.x + offset;
            corners[0].y = y - skewedCell.y + offset;
            corners[0].z = z - skewedCell.z + offset;

            // For the 3D case, the simplex shape is a slightly irregular
            // tetrahedron
            // Determine which simplex we are in

            // Offsets for simplex corners in (i,j,k) coords
            Vector3Int[] skewedOffsets = new Vector3Int[4];
            // First corner's offsets are always (0,0,0)
            skewedOffsets[0] = Vector3Int.zero;
            // Second and third corners
            if (corners[0].x >= corners[0].y)
            {
                if (corners[0].y >= corners[0].z)
                {
                    // X Y Z order
                    skewedOffsets[1] = Vector3Int.right;
                    skewedOffsets[2] = new Vector3Int(1, 1, 0);

                }
                else if (corners[0].x >= corners[0].z)
                {
                    // X Z Y order
                    skewedOffsets[1] = Vector3Int.right;
                    skewedOffsets[2] = new Vector3Int(1, 0, 1);
                }
                else
                {
                    // Z X Y order
                    skewedOffsets[1] = new Vector3Int(0, 0, 1);
                    skewedOffsets[2] = new Vector3Int(1, 0, 1);
                }
            }
            else
            {
                // corners[0].x < corners[0].y
                if (corners[0].y < corners[0].z)
                {
                    // Z Y X order
                    skewedOffsets[1] = new Vector3Int(0, 0, 1);
                    skewedOffsets[2] = new Vector3Int(0, 1, 1);
                }
                else if (corners[0].x < corners[0].z)
                {
                    // Y Z X order
                    skewedOffsets[1] = Vector3Int.up;
                    skewedOffsets[2] = new Vector3Int(0, 1, 1);
                }
                else
                {
                    // Y X Z order
                    skewedOffsets[1] = Vector3Int.up;
                    skewedOffsets[2] = new Vector3Int(1, 1, 0);
                }
            }
            // Last corner's offsets are always (1,1,1)
            skewedOffsets[3] = Vector3Int.one;

            // A step of (1,0,0) in (i,j,k) means a step of (1-c,-c,-c) in
            // (x,y,z),
            // a step of (0,1,0) in (i,j,k) means a step of (-c,1-c,-c) in
            // (x,y,z), and
            // a step of (0,0,1) in (i,j,k) means a step of (-c,-c,1-c) in
            // (x,y,z), where c = 1/6

            // Second to fourth corners in (x,y,z) coords
            for (int i = 1; i < 4; i++)
            {
                float unskewOffset = unskew3D * i;
                corners[i] = corners[0] - skewedOffsets[i];
                corners[i] = new Vector3(
                    corners[i].x + unskewOffset, corners[i].y + unskewOffset,
                    corners[i].z + unskewOffset);
            }

            // Work out the hashed gradient indices of the four simplex corners
            skewedCell = new Vector3Int(skewedCell.x & 255, skewedCell.y & 255,
                                        skewedCell.z & 255);

            // Sum of noise contributions from the four corners
            float noise = 0.0f;

            // Add up the contribution from the four corners
            for (int i = 0; i < 4; i++)
            {
                float t = 0.6f - corners[i].x * corners[i].x -
                                 corners[i].y * corners[i].y -
                                 corners[i].z * corners[i].z;
                if (t >= 0.0f)
                {
                    // Corner's hashed gradient index
                    int gi = perm[
                        skewedCell.x + skewedOffsets[i].x +
                        perm[skewedCell.y + skewedOffsets[i].y +
                             perm[skewedCell.z + skewedOffsets[i].z]]] % 12;
                    t *= t;
                    noise += t * t * Vector3.Dot(grad3[gi], corners[i]);
                }
            }

            // The result is scaled to stay just inside [0,1]
            return 16.0f * noise + 0.5f;
        }

        // 4D simplex noise
        public static float Noise4D(float x, float y, float z, float w)
        {
            // Noise contributions from the five corners
            float n0, n1, n2, n3, n4;

            // Skew the (x,y,z,w) space to determine which cell of 24 simplices
            // we're in
            float s = (x + y + z + w) * skew4D;
            int i = Mathf.FloorToInt(x + s);
            int j = Mathf.FloorToInt(y + s);
            int k = Mathf.FloorToInt(z + s);
            int l = Mathf.FloorToInt(w + s);

            // Unskew the cell origin back to (x,y,z,w) space
            float t = (i + j + k + l) * unskew4D;
            // The x,y,z,w distances from the cell origin
            float x0 = x - i + t;
            float y0 = y - j + t;
            float z0 = z - k + t;
            float w0 = w - l + t;

            // For the 4D case, the simplex is a 4D shape I won't even try to
            // describe.
            // To find out which of the 24 possible simplices we're in, we need
            // to determine the magnitude ordering of x0, y0, z0 and w0.
            // The method below is a good way of finding the ordering of
            // x,y,z,w and then find the correct traversal order for the
            // simplex we’re in.
            // First, six pair-wise comparisons are performed between each
            // possible pair of the four coordinates, and the results are used
            // to add up binary bits for an integer index.
            int c1 = (x0 > y0) ? 32 : 0;
            int c2 = (x0 > z0) ? 16 : 0;
            int c3 = (y0 > z0) ? 8 : 0;
            int c4 = (x0 > w0) ? 4 : 0;
            int c5 = (y0 > w0) ? 2 : 0;
            int c6 = (z0 > w0) ? 1 : 0;
            int c = c1 + c2 + c3 + c4 + c5 + c6;

            int i1, j1, k1, l1; // Integer offsets for the 2nd simplex corner
            int i2, j2, k2, l2; // Integer offsets for the 3rd simplex corner
            int i3, j3, k3, l3; // Integer offsets for the 4th simplex corner

            // simplex[c] is a 4-vector with the numbers 0, 1, 2 and 3 in some
            // order.
            // Many values of c will never occur, since e.g. x>y>z>w makes x<z,
            // y<w and x<w impossible. Only the 24 indices which have non-zero
            // entries make any sense.
            // We use a thresholding to set the coordinates in turn from the
            // largest magnitude.

            // 3 is at the position of the largest coordinate
            i1 = simplex[c, 0] >= 3 ? 1 : 0;
            j1 = simplex[c, 1] >= 3 ? 1 : 0;
            k1 = simplex[c, 2] >= 3 ? 1 : 0;
            l1 = simplex[c, 3] >= 3 ? 1 : 0;
            // 2 is at the second largest coordinate
            i2 = simplex[c, 0] >= 2 ? 1 : 0;
            j2 = simplex[c, 1] >= 2 ? 1 : 0;
            k2 = simplex[c, 2] >= 2 ? 1 : 0;
            l2 = simplex[c, 3] >= 2 ? 1 : 0;
            // 1 is at the second smallest coordinate
            i3 = simplex[c, 0] >= 1 ? 1 : 0;
            j3 = simplex[c, 1] >= 1 ? 1 : 0;
            k3 = simplex[c, 2] >= 1 ? 1 : 0;
            l3 = simplex[c, 3] >= 1 ? 1 : 0;
            // The fifth corner has all coordinate offsets = 1, so no need to
            // look that up

            // Offsets for second corner in (x,y,z,w) coords
            float x1 = x0 - i1 + unskew4D;
            float y1 = y0 - j1 + unskew4D;
            float z1 = z0 - k1 + unskew4D;
            float w1 = w0 - l1 + unskew4D;
            // Offsets for third corner in (x,y,z,w) coords
            float x2 = x0 - i2 + 2.0f * unskew4D;
            float y2 = y0 - j2 + 2.0f * unskew4D;
            float z2 = z0 - k2 + 2.0f * unskew4D;
            float w2 = w0 - l2 + 2.0f * unskew4D;
            // Offsets for fourth corner in (x,y,z,w) coords
            float x3 = x0 - i3 + 3.0f * unskew4D;
            float y3 = y0 - j3 + 3.0f * unskew4D;
            float z3 = z0 - k3 + 3.0f * unskew4D;
            float w3 = w0 - l3 + 3.0f * unskew4D;
            // Offsets for last corner in (x,y,z,w) coords
            float x4 = x0 - 1.0f + 4.0f * unskew4D;
            float y4 = y0 - 1.0f + 4.0f * unskew4D;
            float z4 = z0 - 1.0f + 4.0f * unskew4D;
            float w4 = w0 - 1.0f + 4.0f * unskew4D;

            // Work out the hashed gradient indices of the five simplex corners
            int ii = i & 255;
            int jj = j & 255;
            int kk = k & 255;
            int ll = l & 255;
            int gi0 = perm[ii + perm[jj + perm[kk + perm[ll]]]] % 32;
            int gi1 = perm[
                ii + i1 + perm[jj + j1 + perm[kk + k1 + perm[ll + l1]]]] % 32;
            int gi2 = perm[
                ii + i2 + perm[jj + j2 + perm[kk + k2 + perm[ll + l2]]]] % 32;
            int gi3 = perm[
                ii + i3 + perm[jj + j3 + perm[kk + k3 + perm[ll + l3]]]] % 32;
            int gi4 = perm[
                ii + 1 + perm[jj + 1 + perm[kk + 1 + perm[ll + 1]]]] % 32;

            // Calculate the contribution from the five corners
            float t0 = 0.6f - x0 * x0 - y0 * y0 - z0 * z0 - w0 * w0;
            if (t0 < 0) n0 = 0.0f;
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * Dot(gi0, x0, y0, z0, w0);
            }

            float t1 = 0.6f - x1 * x1 - y1 * y1 - z1 * z1 - w1 * w1;
            if (t1 < 0) n1 = 0.0f;
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * Dot(gi1, x1, y1, z1, w1);
            }

            float t2 = 0.6f - x2 * x2 - y2 * y2 - z2 * z2 - w2 * w2;
            if (t2 < 0) n2 = 0.0f;
            else
            {
                t2 *= t2;
                n2 = t2 * t2 * Dot(gi2, x2, y2, z2, w2);
            }

            float t3 = 0.6f - x3 * x3 - y3 * y3 - z3 * z3 - w3 * w3;
            if (t3 < 0) n3 = 0.0f;
            else
            {
                t3 *= t3;
                n3 = t3 * t3 * Dot(gi3, x3, y3, z3, w3);
            }

            float t4 = 0.6f - x4 * x4 - y4 * y4 - z4 * z4 - w4 * w4;
            if (t4 < 0) n4 = 0.0f;
            else
            {
                t4 *= t4;
                n4 = t4 * t4 * Dot(gi4, x4, y4, z4, w4);
            }

            // Sum up and scale the result to cover the range [0,1]
            return 13.5f * (n0 + n1 + n2 + n3 + n4) + 0.5f;
        }
    }
}