#region Copyright (C) 2007 Team MediaPortal

/*
    Copyright (C) 2007 Team MediaPortal
    http://www.team-mediaportal.com
 
    This file is part of MediaPortal II

    MediaPortal II is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal II is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal II.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Core.Properties;
using MediaPortal.Core.InputManager;
using SkinEngine;
using SkinEngine.DirectX;

using RectangleF = System.Drawing.RectangleF;
using PointF = System.Drawing.PointF;
using SizeF = System.Drawing.SizeF;
using Matrix = Microsoft.DirectX.Matrix;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace SkinEngine.Controls.Visuals
{
  public class Rectangle : Shape
  {

    Property _radiusXProperty;
    Property _radiusYProperty;

    public Rectangle()
    {
      Init();
    }

    public Rectangle(Rectangle s)
      : base(s)
    {
      Init();
      RadiusX = s.RadiusX;
      RadiusY = s.RadiusY;
    }

    public override object Clone()
    {
      return new Rectangle(this);
    }

    void Init()
    {
      _radiusXProperty = new Property(0.0);
      _radiusYProperty = new Property(0.0);
    }


    /// <summary>
    /// Gets or sets the fill property.
    /// </summary>
    /// <value>The fill property.</value>
    public Property RadiusXProperty
    {
      get
      {
        return _radiusXProperty;
      }
      set
      {
        _radiusXProperty = value;
      }
    }

    /// <summary>
    /// Gets or sets the radius X.
    /// </summary>
    /// <value>The radius X.</value>
    public double RadiusX
    {
      get
      {
        return (double)_radiusYProperty.GetValue();
      }
      set
      {
        _radiusYProperty.SetValue(value);
      }
    }

    /// <summary>
    /// Gets or sets the radius Y property.
    /// </summary>
    /// <value>The radius Y property.</value>
    public Property RadiusYProperty
    {
      get
      {
        return _radiusYProperty;
      }
      set
      {
        _radiusYProperty = value;
      }
    }

    /// <summary>
    /// Gets or sets the radius Y.
    /// </summary>
    /// <value>The radius Y.</value>
    public double RadiusY
    {
      get
      {
        return (double)_radiusYProperty.GetValue();
      }
      set
      {
        _radiusYProperty.SetValue(value);
      }
    }




    /// <summary>
    /// Performs the layout.
    /// </summary>
    protected override void PerformLayout()
    {
      Trace.WriteLine("Rectangle.PerformLayout()");
      Free();
      double w = Width; if (w <= 0) w = ActualWidth;
      double h = Height; if (h <= 0) h = ActualHeight;
      Vector3 orgPos = new Vector3(ActualPosition.X, ActualPosition.Y, ActualPosition.Z);

      float centerX = (float)(ActualPosition.X + ActualWidth / 2);
      float centerY = (float)(ActualPosition.Y + ActualHeight / 2);
      PositionColored2Textured[] verts;
      PointF[] vertices;
      GraphicsPath path;
      if (Fill != null)
      {
        path = GetRoundedRect(new RectangleF(ActualPosition.X, ActualPosition.Y, (float)w, (float)h), (float)RadiusX, (float)RadiusY);
        vertices = ConvertPathToTriangleFan(path, (int)+(centerX), (int)(centerY));

        _vertexBufferFill = new VertexBuffer(typeof(PositionColored2Textured), vertices.Length, GraphicsDevice.Device, Usage.WriteOnly, PositionColored2Textured.Format, Pool.Default);
         verts = (PositionColored2Textured[])_vertexBufferFill.Lock(0, 0);
        unchecked
        {
          for (int i = 0; i < vertices.Length; ++i)
          {
            verts[i].X = vertices[i].X;
            verts[i].Y = vertices[i].Y;
            verts[i].Z = 1.0f;
          }
        }
        Fill.SetupBrush(this, ref verts);
        _vertexBufferFill.Unlock();
        _verticesCountFill = (verts.Length - 2);
      }
      //border brush

      ActualPosition = new Vector3(orgPos.X, orgPos.Y, orgPos.Z);
      if (Stroke != null && StrokeThickness > 0)
      {
        path = GetRoundedRect(new RectangleF(ActualPosition.X, ActualPosition.Y, (float)w, (float)h), (float)RadiusX, (float)RadiusY);
        vertices = ConvertPathToTriangleStrip(path, (int)(centerX), (int)(centerY), (float)StrokeThickness);

        _vertexBufferBorder = new VertexBuffer(typeof(PositionColored2Textured), vertices.Length, GraphicsDevice.Device, Usage.WriteOnly, PositionColored2Textured.Format, Pool.Default);
        verts = (PositionColored2Textured[])_vertexBufferBorder.Lock(0, 0);
        unchecked
        {
          for (int i = 0; i < vertices.Length; ++i)
          {
            verts[i].X = vertices[i].X;
            verts[i].Y = vertices[i].Y;
            verts[i].Z = 1.0f;
          }
        }
        Stroke.SetupBrush(this, ref verts);
        _vertexBufferBorder.Unlock();
        _verticesCountBorder = (verts.Length /3);
      }

      ActualPosition = new Vector3(orgPos.X, orgPos.Y, orgPos.Z);
      ActualWidth = w;
      ActualHeight = h;
    }

    #region Get the desired Rounded Rectangle path.
    private GraphicsPath GetRoundedRect(RectangleF baseRect, float radiusX, float radiusY)
    {
      // if corner radius is less than or equal to zero, 

      // return the original rectangle 

      if (radiusX <= 0.0f && RadiusY <= 0.0f)
      {
        GraphicsPath mPath = new GraphicsPath();
        mPath.AddRectangle(baseRect);
        mPath.CloseFigure();
        return mPath;
      }

      // if the corner radius is greater than or equal to 

      // half the width, or height (whichever is shorter) 

      // then return a capsule instead of a lozenge 

      if (radiusX >= (Math.Min(baseRect.Width, baseRect.Height)) / 2.0)
        return GetCapsule(baseRect);

      // create the arc for the rectangle sides and declare 

      // a graphics path object for the drawing 

      float diameter = radiusX * 2.0F;
      SizeF sizeF = new SizeF(diameter, diameter);
      RectangleF arc = new RectangleF(baseRect.Location, sizeF);
      GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();

      // top left arc 


      path.AddArc(arc, 180, 90);

      // top right arc 

      arc.X = baseRect.Right - diameter;
      path.AddArc(arc, 270, 90);

      // bottom right arc 

      arc.Y = baseRect.Bottom - diameter;
      path.AddArc(arc, 0, 90);

      // bottom left arc

      arc.X = baseRect.Left;
      path.AddArc(arc, 90, 90);

      path.CloseFigure();
      path.Flatten();
      return path;
    }
    #endregion

    #region Gets the desired Capsular path.
    private GraphicsPath GetCapsule(RectangleF baseRect)
    {
      float diameter;
      RectangleF arc;
      GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
      try
      {
        if (baseRect.Width > baseRect.Height)
        {
          // return horizontal capsule 

          diameter = baseRect.Height;
          SizeF sizeF = new SizeF(diameter, diameter);
          arc = new RectangleF(baseRect.Location, sizeF);
          path.AddArc(arc, 90, 180);
          arc.X = baseRect.Right - diameter;
          path.AddArc(arc, 270, 180);
        }
        else if (baseRect.Width < baseRect.Height)
        {
          // return vertical capsule 

          diameter = baseRect.Width;
          SizeF sizeF = new SizeF(diameter, diameter);
          arc = new RectangleF(baseRect.Location, sizeF);
          path.AddArc(arc, 180, 180);
          arc.Y = baseRect.Bottom - diameter;
          path.AddArc(arc, 0, 180);
        }
        else
        {
          // return circle 

          path.AddEllipse(baseRect);
        }
      }
      catch (Exception)
      {
        path.AddEllipse(baseRect);
      }
      finally
      {
        path.CloseFigure();
      }
      return path;
    }
    #endregion

  }
}
