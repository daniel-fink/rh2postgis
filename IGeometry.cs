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

namespace Rh2Postgis.Geometry
{
    // IGeometry Interface
    public interface IGeometry
    {
        ProjNet.CoordinateSystems.CoordinateSystem CoordinateReferenceSystem { get; }
    }

}
