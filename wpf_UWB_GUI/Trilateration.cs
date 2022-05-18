using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace wpf_UWB_GUI
{
    class Trilateration
    {
        // Trilaterate.
        // Throw an exception if there is a problem.
        public PointF[] Trilaterate(RectangleF circle1,
            RectangleF circle2, RectangleF circle3)
        {
            // Convert the circles from bounding rectangles
            // to centers and radii.
            float cx1 = (float)(circle1.X + circle1.Width / 2f);
            float cy1 = (float)(circle1.Y + circle1.Height / 2f);
            float cx2 = (float)(circle2.X + circle2.Width / 2f);
            float cy2 = (float)(circle2.Y + circle2.Height / 2f);
            float cx3 = (float)(circle3.X + circle3.Width / 2f);
            float cy3 = (float)(circle3.Y + circle3.Height / 2f);
            float r1 = (float)(circle1.Width / 2f);
            float r2 = (float)(circle2.Width / 2f);
            float r3 = (float)(circle3.Width / 2f);

            // Find the points of intersection.
            PointF
                intersection12a, intersection12b,
                intersection23a, intersection23b,
                intersection31a, intersection31b;

            //if (FindCircleCircleIntersections(
            //        cx1, cy1, r1, cx2, cy2, r2,
            //        out intersection12a, out intersection12b) == 0)
            //    throw new Exception("circle1 and circle2 do not intersect.");

            FindCircleCircleIntersections(
                    cx1, cy1, r1, cx2, cy2, r2,
                    out intersection12a, out intersection12b);

            FindCircleCircleIntersections(
                    cx2, cy2, r2, cx3, cy3, r3,
                    out intersection23a, out intersection23b);

            //if (FindCircleCircleIntersections(
            //        cx2, cy2, r2, cx3, cy3, r3,
            //        out intersection23a, out intersection23b) == 0)
            //    throw new Exception("circle2 and circle3 do not intersect.");

            FindCircleCircleIntersections(
                    cx3, cy3, r3, cx1, cy1, r1,
                    out intersection31a, out intersection31b);

            //if (FindCircleCircleIntersections(
            //        cx3, cy3, r3, cx1, cy1, r1,
            //        out intersection31a, out intersection31b) == 0)
            //    throw new Exception("circle3 and circle1 do not intersect.");

            // Find the points that make up the target area.
            PointF[] triangle = new PointF[3];
            PointF center1 = new PointF(cx1, cy1);
            PointF center2 = new PointF(cx2, cy2);
            PointF center3 = new PointF(cx3, cy3);
            if (distance(intersection12a, center3) <
                    distance(intersection12b, center3))
                triangle[0] = intersection12a;
            else
                triangle[0] = intersection12b;
            if (distance(intersection23a, center1) <
                    distance(intersection23b, center1))
                triangle[1] = intersection23a;
            else
                triangle[1] = intersection23b;
            if (distance(intersection31a, center2) <
                    distance(intersection31b, center2))
                triangle[2] = intersection31a;
            else
                triangle[2] = intersection31b;

            return triangle;
        }

        private double distance(PointF point1, PointF point2)
        {
            double distance_data = Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2));
            return distance_data;
        }

        // Find the points where the two circles intersect.
        private int FindCircleCircleIntersections(
            float cx0, float cy0, float radius0,
            float cx1, float cy1, float radius1,
            out PointF intersection1, out PointF intersection2)
        {
            // Find the distance between the centers.
            float dx = cx0 - cx1;
            float dy = cy0 - cy1;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            // See how many solutions there are.
            if (dist > radius0 + radius1)
            {
                // No solutions, the circles are too far apart.
                intersection1 = new PointF(float.NaN, float.NaN);
                intersection2 = new PointF(float.NaN, float.NaN);
                return 0;
            }
            else if (dist < Math.Abs(radius0 - radius1))
            {
                // No solutions, one circle contains the other.
                intersection1 = new PointF(float.NaN, float.NaN);
                intersection2 = new PointF(float.NaN, float.NaN);
                return 0;
            }
            else if ((dist == 0) && (radius0 == radius1))
            {
                // No solutions, the circles coincide.
                intersection1 = new PointF(float.NaN, float.NaN);
                intersection2 = new PointF(float.NaN, float.NaN);
                return 0;
            }
            else
            {
                // Find a and h.
                double a = (radius0 * radius0 -
                    radius1 * radius1 + dist * dist) / (2 * dist);
                double h = Math.Sqrt(radius0 * radius0 - a * a);

                // Find P2.
                double cx2 = cx0 + a * (cx1 - cx0) / dist;
                double cy2 = cy0 + a * (cy1 - cy0) / dist;

                // Get the points P3.
                intersection1 = new PointF(
                    (float)(cx2 + h * (cy1 - cy0) / dist),
                    (float)(cy2 - h * (cx1 - cx0) / dist));
                intersection2 = new PointF(
                    (float)(cx2 - h * (cy1 - cy0) / dist),
                    (float)(cy2 + h * (cx1 - cx0) / dist));

                // See if we have 1 or 2 solutions.
                if (dist == radius0 + radius1) return 1;
                return 2;
            }
        }
    }
}
