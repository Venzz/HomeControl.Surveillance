using OpenCv;
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

        public event TypedEventHandler<ContourAnalyzer, IEnumerable<Windows.Foundation.Point>> MotionStarted = delegate { };
        public event TypedEventHandler<ContourAnalyzer, Object> MotionFinished = delegate { };



        public ContourAnalyzer(IEnumerable<(Windows.Foundation.Point, Windows.Foundation.Point)> exclusions)
        {
            Exclusions.AddRange(exclusions);
        }

        public void Process(IEnumerable<Mat> contours)
        {
            Contours = Contours.Where(a => !a.Expired).ToList();
            foreach (var contour in contours)
            {
                var contourArea = Cv2.ContourArea(contour);
                if (contourArea < 50)
                    continue;

                var moments = Cv2.Moments(contour);
                var contourCenter = new Windows.Foundation.Point(moments.M10 / moments.M00, moments.M01 / moments.M00);
                var previousContour = TryFindPreviousContour(Contours, contourCenter);
                if (previousContour != null)
                {
                    previousContour.Update(contourCenter);
                }
                else if ((previousContour == null) && !ContainsInExclusions(contourCenter))
                {
                    Contours.Add(new Contour() { Center = contourCenter, Value = contour });
                }
            }

            var detectedContours = Contours.Where(a => a.Occurrences > 10).ToList();
            if (detectedContours.Count > 0)
            {
                if (!MotionDetected)
                {
                    MotionDetected = true;
                    MotionStarted(this, detectedContours.Select(a => new Windows.Foundation.Point(a.Center.X, a.Center.Y)));
                }
            }
            else
            {
                if (MotionDetected)
                {
                    MotionDetected = false;
                    MotionFinished(this, null);
                }
            }
        }

        private Boolean ContainsInExclusions(Windows.Foundation.Point point)
        {
            foreach (var exclusion in Exclusions)
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
                if ((offsetLength > 0) && (offsetLength < 50))
                    return previousPoint;
            }
            return null;
        }

        private class Contour
        {
            private DateTime Created = DateTime.Now;

            public Windows.Foundation.Point Center { get; set; }
            public UInt32 Occurrences { get; private set; } = 1;
            public Mat Value { get; set; }
            public Boolean Expired => (DateTime.Now - Created) > TimeSpan.FromSeconds(5);

            public void Update(Windows.Foundation.Point center)
            {
                Center = center;
                Created = DateTime.Now;
                Occurrences++;
            }
        }
    }
}
