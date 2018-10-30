using System;
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
    // Point Class
    public class GeoPoint : IGeometry
    {
        public Point3d RhinoPoint { get; private set; }
        public Coordinate GeoCoordinate { get; private set; }
        public ProjNet.CoordinateSystems.CoordinateSystem CoordinateReferenceSystem { get; private set; }

        public GeoPoint()
        {
        }

        public GeoPoint(Point3d point, ProjNet.CoordinateSystems.CoordinateSystem sourceProjection)
        {
            this.RhinoPoint = point;
            this.CoordinateReferenceSystem = sourceProjection;
            this.GeoCoordinate = new Coordinate(point.X, point.Y, point.Z);
        }

        public GeoPoint(Coordinate coordinate, ProjNet.CoordinateSystems.CoordinateSystem sourceProjection)
        {
            this.RhinoPoint = new Point3d(coordinate.X, coordinate.Y, coordinate.Z);
            this.GeoCoordinate = coordinate;
            this.CoordinateReferenceSystem = sourceProjection;
        }

        /// <summary>
        /// Transforms a GeoPoint to a NetTopologySuite Point
        /// </summary>
        public NetTopologySuite.Geometries.Point ToGeospatialPoint()
        {
            return new NetTopologySuite.Geometries.Point(this.GeoCoordinate);
        }

        /// <summary>
        /// Reprojects a GeoPoint to a NetTopologySuite Point in a target Coordinate System
        /// </summary>
        public NetTopologySuite.Geometries.Point Reproject(ProjNet.CoordinateSystems.CoordinateSystem targetProjection)
        {
            CoordinateSystemFactory coordSystem = new CoordinateSystemFactory();
            var transform = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();
            var reproject = transform.CreateFromCoordinateSystems(this.CoordinateReferenceSystem, targetProjection);

            var reprojectedPoint = reproject.MathTransform.Transform(new double[2] { this.GeoCoordinate.X, this.GeoCoordinate.Y });
            var reprojectedCoord = new GeoAPI.Geometries.Coordinate(reprojectedPoint[0], reprojectedPoint[1]);

            return new NetTopologySuite.Geometries.Point(reprojectedCoord);
        }

    }

}
