using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Model
{
    public class ContourAnalyzer
    {
        private Boolean MotionDetected = false;
        private List<Contour> Contours = new List<Contour>();
        private List<(Windows.Foundation.Point TopLeft, Windows.Foundation.Point BottomRight)> Exclusions = new List<(Windows.Foundation.Point, Windows.Foundation.Point)>();

        public event TypedEventHandler<ContourAnalyzer, Object> MotionStarted = delegate { };
        public event TypedEventHandler<ContourAnalyzer, Object> MotionFinished = delegate { };



        public ContourAnalyzer(IEnumerable<(Windows.Foundation.Point, Windows.Foundation.Point)> exclusions)
        {
            Exclusions.AddRange(exclusions);
        }

        public void Process(IEnumerable<Mat> contours)
        {
            var previousContours = new List<Contour>(Contours);
            Contours.Clear();
            foreach (var contour in contours)
            {
                var contourArea = Cv2.ContourArea(contour);
                if (contourArea < 500)
                    continue;

                var moments = Cv2.Moments(contour);
                var contourCenter = new Windows.Foundation.Point(moments.M10 / moments.M00, moments.M01 / moments.M00);
                var previousContour = TryFindPreviousContour(previousContours, contourCenter);
                if ((previousContour != null) && !ContainsInExclusions(previousContour.Center))
                    Contours.Add(previousContour);
                else if ((previousContour == null) && !ContainsInExclusions(contourCenter))
                    Contours.Add(new Contour() { Center = contourCenter, Value = contour });
            }

            if (Contours.Count(a => a.Occurrences > 10) > 0)
            {
                if (!MotionDetected)
                {
                    MotionDetected = true;
                    MotionStarted(this, null);
                }
            }
            else
            {
                MotionDetected = false;
                MotionFinished(this, null);
            }
        }

        private Boolean ContainsInExclusions(Windows.Foundation.Point point)
        {
            foreach(var exclusion in Exclusions)
            {
                if ((point.X >= exclusion.TopLeft.X) && (point.X <= exclusion.BottomRight.X) && (point.Y >= exclusion.TopLeft.Y) && (point.Y <= exclusion.BottomRight.Y))
                    return true;
            }
            return false;
        }

        private Contour TryFindPreviousContour(IEnumerable<Contour> previousContours, Windows.Foundation.Point point)
        {
            foreach (var previousPoint in previousContours)
            {
                var offsetLength = Math.Sqrt(Math.Pow(Math.Abs(previousPoint.Center.X - point.X), 2) + Math.Pow(Math.Abs(previousPoint.Center.Y - point.Y), 2));
                if ((offsetLength > 0) && (offsetLength < 100))
                    return new Contour() { Center = point, Occurrences = ++previousPoint.Occurrences, Value = previousPoint.Value };
            }
            return null;
        }

        private class Contour
        {
            public Windows.Foundation.Point Center { get; set; }
            public UInt32 Occurrences { get; set; }
            public Mat Value { get; set; }
        }
    }
}
