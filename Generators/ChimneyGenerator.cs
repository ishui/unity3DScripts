using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BuildR2
{
  public class ChimneyGenerator
  {
    public static BuildRMesh DYNAMIC_MESH = new BuildRMesh("chimney mesh");
    public static RandomGen RGEN = new RandomGen();

    public static void Generate(Chimney chimney, GenerationOutput output, SubmeshLibrary submeshLibrary = null)
    {
      RGEN.seed = chimney.seed;
      DYNAMIC_MESH.Clear();
      if (submeshLibrary != null)
        DYNAMIC_MESH.submeshLibrary.AddRange(submeshLibrary.SURFACES.ToArray()); //DYNAMIC_MESH.submeshLibrary.Inject(ref submeshLibrary);
      else
        DYNAMIC_MESH.submeshLibrary.Add(chimney);

      submeshLibrary = DYNAMIC_MESH.submeshLibrary;

      //CASE
      Vector3 caseNoiseVector = new Vector3(chimney.noise.x * RGEN.OneRange(), chimney.noise.y * RGEN.OneRange(), chimney.noise.z * RGEN.OneRange());
      Vector3 cs0 = new Vector3(-chimney.caseSize.x * 0.5f, 0, -chimney.caseSize.z * 0.5f);
      Vector3 cs1 = new Vector3(chimney.caseSize.x * 0.5f, 0, -chimney.caseSize.z * 0.5f);
      Vector3 cs2 = new Vector3(-chimney.caseSize.x * 0.5f, 0, chimney.caseSize.z * 0.5f);
      Vector3 cs3 = new Vector3(chimney.caseSize.x * 0.5f, 0, chimney.caseSize.z * 0.5f);

      Vector3 cs4 = new Vector3(-chimney.caseSize.x * 0.5f, chimney.caseSize.y, -chimney.caseSize.z * 0.5f) + caseNoiseVector;
      Vector3 cs5 = new Vector3(chimney.caseSize.x * 0.5f, chimney.caseSize.y, -chimney.caseSize.z * 0.5f) + caseNoiseVector;
      Vector3 cs6 = new Vector3(-chimney.caseSize.x * 0.5f, chimney.caseSize.y, chimney.caseSize.z * 0.5f) + caseNoiseVector;
      Vector3 cs7 = new Vector3(chimney.caseSize.x * 0.5f, chimney.caseSize.y, chimney.caseSize.z * 0.5f) + caseNoiseVector;

      Vector2 csuv0 = new Vector2(0, 0);
      Vector2 csuv1 = new Vector2(chimney.caseSize.x, chimney.caseSize.y);
      Vector2 csuv2 = new Vector2(csuv1.x, 0);
      Vector2 csuv3 = new Vector2(csuv1.x + chimney.caseSize.z, chimney.caseSize.y);
      Vector2 csuv4 = new Vector2(csuv3.x, 0);
      Vector2 csuv5 = new Vector2(csuv3.x + chimney.caseSize.x, chimney.caseSize.y);
      Vector2 csuv6 = new Vector2(csuv5.x, 0);
      Vector2 csuv7 = new Vector2(csuv5.x + chimney.caseSize.z, chimney.caseSize.y);
      Vector2 csuv8 = new Vector2(0, 0);
      Vector2 csuv9 = new Vector2(chimney.caseSize.x, chimney.caseSize.z);

      Vector4 cst0 = new Vector4(0, 0, 1, 0);
      Vector4 cst1 = new Vector4(1, 0, 1, 0);
      Vector4 cst2 = new Vector4(0, 0, -1, 0);
      Vector4 cst3 = new Vector4(-1, 0, 0, 0);
      Vector4 cst4 = new Vector4(0, 0, 1, 0);

      int caseSubmesh = submeshLibrary.SubmeshAdd(chimney.caseSurface);
      //sides
      DYNAMIC_MESH.AddPlane(cs0, cs1, cs4, cs5, csuv0, csuv1, Vector3.back, cst0, caseSubmesh, chimney.caseSurface);
      DYNAMIC_MESH.AddPlane(cs1, cs3, cs5, cs7, csuv2, csuv3, Vector3.right, cst1, caseSubmesh, chimney.caseSurface);
      DYNAMIC_MESH.AddPlane(cs3, cs2, cs7, cs6, csuv4, csuv5, Vector3.forward, cst2, caseSubmesh, chimney.caseSurface);
      DYNAMIC_MESH.AddPlane(cs2, cs0, cs6, cs4, csuv6, csuv7, Vector3.left, cst3, caseSubmesh, chimney.caseSurface);
      //top
      DYNAMIC_MESH.AddPlane(cs4, cs5, cs6, cs7, csuv8, csuv9, Vector3.up, cst4, caseSubmesh, chimney.caseSurface);//todo calculate the values for this - don't be lazy

      //CROWN
      Vector3 crownBase = caseNoiseVector + Vector3.up * chimney.caseSize.y;
      Vector3 crownNoiseVector = new Vector3(chimney.noise.x * RGEN.OneRange(), chimney.noise.y * RGEN.OneRange(), chimney.noise.z * RGEN.OneRange());
      Vector3 cr0 = crownBase + new Vector3(-chimney.crownSize.x * 0.5f, 0, -chimney.crownSize.z * 0.5f);
      Vector3 cr1 = crownBase + new Vector3(chimney.crownSize.x * 0.5f, 0, -chimney.crownSize.z * 0.5f);
      Vector3 cr2 = crownBase + new Vector3(-chimney.crownSize.x * 0.5f, 0, chimney.crownSize.z * 0.5f);
      Vector3 cr3 = crownBase + new Vector3(chimney.crownSize.x * 0.5f, 0, chimney.crownSize.z * 0.5f);

      Vector3 cr4 = crownBase + new Vector3(-chimney.crownSize.x * 0.5f, chimney.crownSize.y, -chimney.crownSize.z * 0.5f) + crownNoiseVector;
      Vector3 cr5 = crownBase + new Vector3(chimney.crownSize.x * 0.5f, chimney.crownSize.y, -chimney.crownSize.z * 0.5f) + crownNoiseVector;
      Vector3 cr6 = crownBase + new Vector3(-chimney.crownSize.x * 0.5f, chimney.crownSize.y, chimney.crownSize.z * 0.5f) + crownNoiseVector;
      Vector3 cr7 = crownBase + new Vector3(chimney.crownSize.x * 0.5f, chimney.crownSize.y, chimney.crownSize.z * 0.5f) + crownNoiseVector;

      Vector2 cruv0 = new Vector2(0, 0);
      Vector2 cruv1 = new Vector2(chimney.crownSize.x, chimney.crownSize.y);
      Vector2 cruv2 = new Vector2(csuv1.x, 0);
      Vector2 cruv3 = new Vector2(csuv1.x + chimney.caseSize.z, chimney.crownSize.y);
      Vector2 cruv4 = new Vector2(csuv3.x, 0);
      Vector2 cruv5 = new Vector2(csuv3.x + chimney.crownSize.x, chimney.crownSize.y);
      Vector2 cruv6 = new Vector2(csuv5.x, chimney.crownSize.y);
      Vector2 cruv7 = new Vector2(csuv5.x + chimney.crownSize.z, chimney.crownSize.y);
      Vector2 cruv8 = new Vector2(0, 0);
      Vector2 cruv9 = new Vector2(chimney.crownSize.x, chimney.crownSize.z);

      Vector4 crt0 = new Vector4(0, 0, 1, 0);
      Vector4 crt1 = new Vector4(1, 0, 1, 0);
      Vector4 crt2 = new Vector4(0, 0, -1, 0);
      Vector4 crt3 = new Vector4(-1, 0, 0, 0);
      Vector4 crt4 = new Vector4(0, 0, 1, 0);

      int crownSubmesh = submeshLibrary.SubmeshAdd(chimney.crownSurface);
      DYNAMIC_MESH.AddPlane(cr0, cr1, cr4, cr5, cruv0, cruv1, Vector3.back, crt0, crownSubmesh, chimney.crownSurface);//todo calculate the values for this - don't be lazy
      DYNAMIC_MESH.AddPlane(cr1, cr3, cr5, cr7, cruv2, cruv3, Vector3.right, crt1, crownSubmesh, chimney.crownSurface);//todo calculate the values for this - don't be lazy
      DYNAMIC_MESH.AddPlane(cr3, cr2, cr7, cr6, cruv4, cruv5, Vector3.forward, crt2, crownSubmesh, chimney.crownSurface);//todo calculate the values for this - don't be lazy
      DYNAMIC_MESH.AddPlane(cr2, cr0, cr6, cr4, cruv6, cruv7, Vector3.left, crt3, crownSubmesh, chimney.crownSurface);//todo calculate the values for this - don't be lazy
      DYNAMIC_MESH.AddPlane(cr1, cr0, cr3, cr2, cruv8, cruv9, Vector3.down, crt4, crownSubmesh, chimney.crownSurface);//todo calculate the values for this - don't be lazy
      DYNAMIC_MESH.AddPlane(cr4, cr5, cr6, cr7, cruv8, cruv9, Vector3.up, crt4, crownSubmesh, chimney.crownSurface);//todo calculate the values for this - don't be lazy

      int xCount = 1;
      int zCount = 1;
      if (chimney.allowMultiple)
      {
        xCount = Mathf.FloorToInt((chimney.crownSize.x - chimney.flueSpacing) / (chimney.flueSize.x + chimney.flueSpacing));
        if (xCount < 1) xCount = 1;
        if (chimney.allowMultipleRows)
        {
          zCount = Mathf.FloorToInt((chimney.crownSize.z - chimney.flueSpacing) / (chimney.flueSize.z + chimney.flueSpacing));
          if (zCount < 1) zCount = 1;
        }
      }

      float xSpacing = (chimney.crownSize.x - chimney.flueSize.x * xCount) / (xCount + 1);
      float zSpacing = (chimney.crownSize.z - chimney.flueSize.z * zCount) / (zCount + 1);

      //FLUES
      for (int x = 0; x < xCount; x++)
      {
        for (int z = 0; z < zCount; z++)
        {
          Vector3 flueBase = cr4 + new Vector3(xSpacing + x * (chimney.flueSize.x + xSpacing) + chimney.flueSize.x * 0.5f, 0, zSpacing + z * (chimney.flueSize.z + zSpacing) + chimney.flueSize.z * 0.5f);

          float thickness = (chimney.flueSize.x + chimney.flueSize.z) * 0.05f;//10%
          float drop = chimney.flueSize.y * 0.9f;
          Vector4 topTangent = new Vector4(1, 0, 0, 0);

          Surface useFlueSurface = GenerationUtil.GetSurface(chimney.flueSurfaces, RGEN);
          int flueSubmesh = submeshLibrary.SubmeshAdd(useFlueSurface);
          int innerSubmesh = submeshLibrary.SubmeshAdd(chimney.innerSurface);

          Vector3 flueNoiseVector = new Vector3(chimney.noise.x * RGEN.OneRange(), chimney.noise.y * RGEN.OneRange(), chimney.noise.z * RGEN.OneRange());

          if (chimney.square)
          {
            Vector3 f0 = flueBase + new Vector3(-chimney.flueSize.x * 0.5f, 0, -chimney.flueSize.z * 0.5f);
            Vector3 f1 = flueBase + new Vector3(chimney.flueSize.x * 0.5f, 0, -chimney.flueSize.z * 0.5f);
            Vector3 f2 = flueBase + new Vector3(-chimney.flueSize.x * 0.5f, 0, chimney.flueSize.z * 0.5f);
            Vector3 f3 = flueBase + new Vector3(chimney.flueSize.x * 0.5f, 0, chimney.flueSize.z * 0.5f);

            Vector3 f4 = flueBase + new Vector3(-chimney.flueSize.x * 0.5f, chimney.flueSize.y, -chimney.flueSize.z * 0.5f) + flueNoiseVector;
            Vector3 f5 = flueBase + new Vector3(chimney.flueSize.x * 0.5f, chimney.flueSize.y, -chimney.flueSize.z * 0.5f) + flueNoiseVector;
            Vector3 f6 = flueBase + new Vector3(-chimney.flueSize.x * 0.5f, chimney.flueSize.y, chimney.flueSize.z * 0.5f) + flueNoiseVector;
            Vector3 f7 = flueBase + new Vector3(chimney.flueSize.x * 0.5f, chimney.flueSize.y, chimney.flueSize.z * 0.5f) + flueNoiseVector;

            Vector3 f4i = f4 + new Vector3(thickness, 0, thickness) + flueNoiseVector;
            Vector3 f5i = f5 + new Vector3(-thickness, 0, thickness) + flueNoiseVector;
            Vector3 f6i = f6 + new Vector3(thickness, 0, -thickness) + flueNoiseVector;
            Vector3 f7i = f7 + new Vector3(-thickness, 0, -thickness) + flueNoiseVector;

            Vector3 f4id = f4i + new Vector3(0, -drop, 0);
            Vector3 f5id = f5i + new Vector3(0, -drop, 0);
            Vector3 f6id = f6i + new Vector3(0, -drop, 0);
            Vector3 f7id = f7i + new Vector3(0, -drop, 0);

//            Vector2 fuv0 = new Vector2(0, 0);
//            Vector2 fuv1 = new Vector2(chimney.flueSize.x, 0);
//            Vector2 fuv2 = new Vector2(fuv1.x + chimney.flueSize.z, 0);
//            Vector2 fuv3 = new Vector2(fuv2.x + chimney.flueSize.x, 0);
//
//            Vector2 fuv4 = new Vector2(0, chimney.flueSize.y);
//            Vector2 fuv5 = new Vector2(chimney.flueSize.x, chimney.flueSize.y);
//            Vector2 fuv6 = new Vector2(fuv1.x + chimney.flueSize.z, chimney.flueSize.y);
//            Vector2 fuv7 = new Vector2(fuv2.x + chimney.flueSize.x, chimney.flueSize.y);


            //Flue Sides
            DYNAMIC_MESH.AddPlane(f0, f1, f4, f5, flueSubmesh);//todo calculate the values for this - don't be lazy
            DYNAMIC_MESH.AddPlane(f1, f3, f5, f7, flueSubmesh);//todo calculate the values for this - don't be lazy
            DYNAMIC_MESH.AddPlane(f3, f2, f7, f6, flueSubmesh);//todo calculate the values for this - don't be lazy
            DYNAMIC_MESH.AddPlane(f2, f0, f6, f4, flueSubmesh);//todo calculate the values for this - don't be lazy
                                                               //Flue Top
            DYNAMIC_MESH.AddPlaneComplex(f4, f5, f4i, f5i, Vector3.up, topTangent, flueSubmesh, useFlueSurface);//todo calculate the values for this - don't be lazy
            DYNAMIC_MESH.AddPlaneComplex(f5, f7, f5i, f7i, Vector3.up, topTangent, flueSubmesh, useFlueSurface);//todo calculate the values for this - don't be lazy
            DYNAMIC_MESH.AddPlaneComplex(f7, f6, f7i, f6i, Vector3.up, topTangent, flueSubmesh, useFlueSurface);//todo calculate the values for this - don't be lazy
            DYNAMIC_MESH.AddPlaneComplex(f6, f4, f6i, f4i, Vector3.up, topTangent, flueSubmesh, useFlueSurface);//todo calculate the values for this - don't be lazy
                                                                                                                //Flue Drop
            DYNAMIC_MESH.AddPlane(f5id, f4id, f5i, f4i, innerSubmesh);//todo calculate the values for this - don't be lazy
            DYNAMIC_MESH.AddPlane(f7id, f5id, f7i, f5i, innerSubmesh);//todo calculate the values for this - don't be lazy
            DYNAMIC_MESH.AddPlane(f6id, f7id, f6i, f7i, innerSubmesh);//todo calculate the values for this - don't be lazy
            DYNAMIC_MESH.AddPlane(f4id, f6id, f4i, f6i, innerSubmesh);//todo calculate the values for this - don't be lazy
            DYNAMIC_MESH.AddPlane(f4id, f5id, f6id, f7id, innerSubmesh);//todo calculate the values for this - don't be lazy
          }
          else
          {
            int vertCount = (chimney.segments + 1) * 2;//add an additonal so we can wrap the UVs well
            RawMeshData flueOuter = new RawMeshData(vertCount, chimney.segments * 6);
            RawMeshData flueTop = new RawMeshData(vertCount, chimney.segments * 6);
            //add additional point for the middle, bottom of the inside of the flue
            RawMeshData flueInner = new RawMeshData(vertCount + 1, chimney.segments * 9);

            //the additonal point at the bottom of the flue - added to the end of the mesh data
            flueInner.vertices[vertCount] = flueBase;
            flueInner.normals[vertCount] = Vector3.up;
            flueInner.tangents[vertCount] = new Vector4(1, 0, 0, 0);
            int indexIm = flueInner.vertCount - 1;
            float circumference = Mathf.PI * (chimney.flueSize.x + chimney.flueSize.z);

            for (int s = 0; s < chimney.segments + 1; s++)
            {
              float percent = s / (float)(chimney.segments);
              percent = (percent + (chimney.angleOffset / 360)) % 1f;

              int indexV0 = s * 2;
              int indexV1 = s * 2 + 1;
              int indexV2 = s * 2 + 2;
              int indexV3 = s * 2 + 3;
              if (s == chimney.segments - 1)
              {
                indexV2 = 0;
                indexV3 = 1;
              }

              float xa = Mathf.Sin(percent * Mathf.PI * 2) * chimney.flueSize.x * 0.5f;
              float za = Mathf.Cos(percent * Mathf.PI * 2) * chimney.flueSize.z * 0.5f;
//              float innerHalf = thickness / (chimney.flueSize.x + chimney.flueSize.z) / 2;
              float xai = Mathf.Sin(percent * Mathf.PI * 2) * chimney.flueSize.x * 0.4f;
              float zai = Mathf.Cos(percent * Mathf.PI * 2) * chimney.flueSize.z * 0.4f;

              Vector3 v0 = flueBase + new Vector3(xa, 0, za);
              Vector3 v1 = flueBase + new Vector3(xa, chimney.flueSize.y, za) + flueNoiseVector;
              Vector3 v2 = flueBase + new Vector3(xai, chimney.flueSize.y, zai) + flueNoiseVector;
              Vector3 v3 = flueBase + new Vector3(xai, chimney.flueSize.y * 0.1f, zai);

              Vector2 uv0 = new Vector2(-circumference * percent, 0);
              Vector2 uv1 = new Vector2(-circumference * percent, chimney.flueSize.y);
              Vector2 uv2 = new Vector2(-circumference * percent, chimney.flueSize.y + 0.1f);
              Vector2 uv3 = new Vector2(-circumference * percent, 0);

              int rdnFlueSurfaceIndex = RGEN.Index(chimney.flueSurfaces.Count);
              Surface flueSurface = rdnFlueSurfaceIndex != -1 ? chimney.flueSurfaces[rdnFlueSurfaceIndex] : null;


              if (flueSurface != null)
              {
                uv0 = flueSurface.CalculateUV(uv0);
                uv1 = flueSurface.CalculateUV(uv1);
                uv2 = flueSurface.CalculateUV(uv2);
                uv3 = flueSurface.CalculateUV(uv3);
              }

              flueOuter.vertices[indexV0] = v0;
              flueOuter.vertices[indexV1] = v1;
              flueOuter.uvs[indexV0] = uv0;
              flueOuter.uvs[indexV1] = uv1;

              flueTop.vertices[indexV0] = v1;
              flueTop.vertices[indexV1] = v2;
              flueTop.uvs[indexV0] = uv1;
              flueTop.uvs[indexV1] = uv2;

              flueInner.vertices[indexV0] = v2;
              flueInner.vertices[indexV1] = v3;
              flueInner.uvs[indexV0] = uv2;
              flueInner.uvs[indexV1] = uv3;

              Vector3 outerNormal = new Vector3(Mathf.Sin(percent * Mathf.PI * 2), 0, Mathf.Cos(percent * Mathf.PI * 2));
              flueOuter.normals[indexV0] = outerNormal;
              flueOuter.normals[indexV1] = outerNormal;
              flueTop.normals[indexV0] = Vector3.up;
              flueTop.normals[indexV1] = Vector3.up;
              flueInner.normals[indexV0] = -outerNormal;
              flueInner.normals[indexV1] = -outerNormal;

              if (s < chimney.segments)
              {
                int tidx0 = s * 6;
                flueOuter.triangles[tidx0 + 0] = indexV0;
                flueOuter.triangles[tidx0 + 2] = indexV1;
                flueOuter.triangles[tidx0 + 1] = indexV2;
                flueOuter.triangles[tidx0 + 3] = indexV1;
                flueOuter.triangles[tidx0 + 4] = indexV2;
                flueOuter.triangles[tidx0 + 5] = indexV3;

                flueTop.triangles[tidx0 + 0] = indexV0;
                flueTop.triangles[tidx0 + 2] = indexV1;
                flueTop.triangles[tidx0 + 1] = indexV2;
                flueTop.triangles[tidx0 + 3] = indexV1;
                flueTop.triangles[tidx0 + 4] = indexV2;
                flueTop.triangles[tidx0 + 5] = indexV3;

                int tidx0i = s * 9;
                flueInner.triangles[tidx0i + 0] = indexV0;
                flueInner.triangles[tidx0i + 2] = indexV1;
                flueInner.triangles[tidx0i + 1] = indexV2;
                flueInner.triangles[tidx0i + 3] = indexV1;
                flueInner.triangles[tidx0i + 4] = indexV2;
                flueInner.triangles[tidx0i + 5] = indexV3;

                flueInner.triangles[tidx0i + 6] = indexV1;
                flueInner.triangles[tidx0i + 7] = indexV3;
                flueInner.triangles[tidx0i + 8] = indexIm;

              }
            }

            DYNAMIC_MESH.AddData(flueOuter, flueSubmesh);
            DYNAMIC_MESH.AddData(flueTop, flueSubmesh);
            DYNAMIC_MESH.AddData(flueInner, innerSubmesh);
          }

        }
      }


      if (output.raw != null)
      {
        output.raw.Copy(DYNAMIC_MESH);
      }

      if (output.mesh != null)
      {
        output.mesh.Clear(false);
        DYNAMIC_MESH.Build(output.mesh);
      }
    }
  }
}