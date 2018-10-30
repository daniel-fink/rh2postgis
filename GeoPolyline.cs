using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Collections;

using GeoAPI;
using GeoAPI.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.Converters.WellKnownText;

using NetTopologySuite.Geometries;

namespace Rh2Postgis.Geometry
{
    public class GeoPolyline : IGeometry
    {
        public Polyline RhinoPolyline { get; private set; }
        public NetTopologySuite.Geometries.LineString GeoLinestring { get; private set;}
        public ProjNet.CoordinateSystems.CoordinateSystem CoordinateReferenceSystem { get; private set; }

        public GeoPolyline()
        {
        }

        // GeoLinestring Constructor
        public GeoPolyline(Polyline polyline, ProjNet.CoordinateSystems.CoordinateSystem sourceProjection)
        {
            if (polyline.IsValid)
            {
                this.RhinoPolyline = polyline;
                this.GeoLinestring = this.ToGeoLinestring();
                this.CoordinateReferenceSystem = sourceProjection;
            }

            else
            {
                throw new ArgumentException("Polyline is not valid", "RhinoPolyline");
            }
        }


        /// <summary>
        /// Gets the Coordinate Array
        /// </summary>
        private GeoAPI.Geometries.Coordinate[] GetCoordinates()
        {
            return this.RhinoPolyline.Select(vertex => new Coordinate(vertex.X, vertex.Y, vertex.Z)).ToArray();
        }

        /// <summary>
        /// Creates a NetTopologySuite Linestring
        /// </summary>
        private NetTopologySuite.Geometries.LineString ToGeoLinestring()
        {
            return new LineString(this.GetCoordinates());
        }

        /// <summary>
        /// Reprojects a Placeful Polygon to a NetTopologySuite Polygon in a target Coordinate System
        /// </summary>
        public NetTopologySuite.Geometries.Polygon Reproject(ProjNet.CoordinateSystems.CoordinateSystem targetProjection)
        {
            CoordinateSystemFactory coordSystem = new CoordinateSystemFactory();
            var transform = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();
            var reproject = transform.CreateFromCoordinateSystems(this.CoordinateReferenceSystem, targetProjection);

            var reprojectedCoords = reproject.MathTransform.TransformList(this.GetCoordinates());
            var reprojectedRing = new LinearRing(reprojectedCoords.ToArray());

            return new NetTopologySuite.Geometries.Polygon(reprojectedRing);
        }

        public void OrientCounterClockwise()
        {
            var boundaryCurve = this.RhinoPolyline.ToPolylineCurve();
            if (boundaryCurve.ClosedCurveOrientation(Plane.WorldXY.ZAxis) == CurveOrientation.Clockwise)
            {
                this.RhinoPolyline.Reverse();
                this.GeoLinestring = this.GeoLinestring.Reverse() as LineString;
            }

            else
            {
            }
        }

        public void OrientClockwise()
        {
            var boundaryCurve = this.RhinoPolyline.ToPolylineCurve();
            if (boundaryCurve.ClosedCurveOrientation(Plane.WorldXY.ZAxis) == CurveOrientation.CounterClockwise)
            {
                this.RhinoPolyline.Reverse();
                this.GeoLinestring = this.GeoLinestring.Reverse() as LineString;
            }

            else
            {
            }
        }
    }

    public class GeoPolylineCollection : IEnumerable<GeoPolyline>
    {
        public GeoPolyline[] GeoPolylines { get; private set; }
        public ProjNet.CoordinateSystems.CoordinateSystem CoordinateReferenceSystem { get; private set; }

        public GeoPolylineCollection()
        {
        }

        // Polygon Constructor
        public GeoPolylineCollection(IEnumerable<GeoPolyline> geoPolylines)
        {
            GeoPolylines = geoPolylines.ToArray();

            var projections = geoPolylines.Select(geoPolyline => geoPolyline.CoordinateReferenceSystem);
            if (projections.Any(projection => projection != projections.First())) // Check to see if any of the CRS's for input geoms are not the same
            {
                throw new Exception("Error: Coordinate Reference Systems for input polygons are not the same");
            }
            else
            {
                this.CoordinateReferenceSystem = geoPolylines.First().CoordinateReferenceSystem;
            }
        }

        public IEnumerator<GeoPolyline> GetEnumerator()
        {
            return GeoPolylines.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() // Explicitly implement the non-generic version.
        {
            return GeoPolylines.ToList().GetEnumerator();
        }
    }
}
