#region copyright
// BuildR 2.0
// Available on the Unity Asset Store https://www.assetstore.unity3d.com/#!/publisher/412
// Copyright (c) 2017 Jasper Stocker http://support.jasperstocker.com
// Support contact email@jasperstocker.com
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
#endregion

using UnityEngine;

namespace BuildR2
{
    public class ElevatorShaftGenerator
    {

        public static void Generate(ref BuildRMesh mesh, VerticalOpening opening, int actualFloor, Vector3 basePosition, float height, int wallSubmesh = -1, BuildRCollider collider = null)
        {
            //            bool lowerLanding = true;
            //            float wallDepth = 0.5f;//todo
            bool generateColldier = collider != null;
            float wallThickness = VerticalOpening.WALL_THICKNESS;
            if (collider != null)
                collider.thickness = VerticalOpening.WALL_THICKNESS;
//            bool generateMeshCollider = generateColldier && !collider.usingPrimitives;

	        SubmeshLibrary submeshLibrary = mesh.submeshLibrary;
            int externalWallSubmesh = submeshLibrary.SubmeshAdd(opening.surfaceA);
            int internalWallSubmesh = submeshLibrary.SubmeshAdd(opening.surfaceB);
			int doorFrameSubmesh = submeshLibrary.SubmeshAdd(opening.surfaceC);
            
            if (wallSubmesh != -1) externalWallSubmesh = wallSubmesh;
            if (externalWallSubmesh == -1) externalWallSubmesh = 0;
            if (internalWallSubmesh == -1) internalWallSubmesh = 0;
            if (doorFrameSubmesh == -1) doorFrameSubmesh = 0;

            //base positions
            Quaternion rotation = Quaternion.Euler(0, opening.rotation, 0);
//            Vector2Int openingSize = opening.size;
            Vector3 b0 = basePosition + rotation * new Vector3(-opening.size.vx * 0.5f, 0, -opening.size.vy * 0.5f);
            Vector3 b1 = basePosition + rotation * new Vector3(opening.size.vx * 0.5f, 0, -opening.size.vy * 0.5f);
            Vector3 b2 = basePosition + rotation * new Vector3(-opening.size.vx * 0.5f, 0, opening.size.vy * 0.5f);
            Vector3 b3 = basePosition + rotation * new Vector3(opening.size.vx * 0.5f, 0, opening.size.vy * 0.5f);

            //inner points
            Vector3 b0i = b0 + rotation * new Vector3(1, 0, 1) * wallThickness;
            Vector3 b1i = b1 + rotation * new Vector3(-1, 0, 1) * wallThickness;
            Vector3 b2i = b2 + rotation * new Vector3(1, 0, -1) * wallThickness;
            Vector3 b3i = b3 + rotation * new Vector3(-1, 0, -1) * wallThickness;
            
            //walls
            Vector3 wallUpInternal = Vector3.up * height;
            Vector3 wallUpExternal = Vector3.up * height;
            wallUpExternal.y += -wallThickness * 0.5f;
            //external
            mesh.AddPlane(b2, b0, b2 + wallUpExternal, b0 + wallUpExternal, externalWallSubmesh);
            mesh.AddPlane(b3, b2, b3 + wallUpExternal, b2 + wallUpExternal, externalWallSubmesh);
            mesh.AddPlane(b1, b3, b1 + wallUpExternal, b3 + wallUpExternal, externalWallSubmesh);
            //internal
            mesh.AddPlane(b0i, b2i, b0i + wallUpInternal, b2i + wallUpInternal, internalWallSubmesh);
            mesh.AddPlane(b2i, b3i, b2i + wallUpInternal, b3i + wallUpInternal, internalWallSubmesh);
            mesh.AddPlane(b3i, b1i, b3i + wallUpInternal, b1i + wallUpInternal, internalWallSubmesh);

            //door
            Vector3 b0d = b0 + rotation * (Vector3.right * opening.size.vx * 0.15f);
            Vector3 b1d = b1 + rotation * (Vector3.left * opening.size.vx * 0.15f);
            Vector3 doorUp = wallUpInternal * 0.85f;
                //external
            mesh.AddPlane(b0, b0d, b0 + doorUp, b0d + doorUp, externalWallSubmesh);
            mesh.AddPlane(b1d, b1, b1d + doorUp, b1 + doorUp, externalWallSubmesh);
            mesh.AddPlane(b0 + doorUp, b1 + doorUp, b0 + wallUpExternal, b1 + wallUpExternal, externalWallSubmesh);
            //internal
            Vector3 doorFrameV = rotation * new Vector3(0, 0, 1) * wallThickness;
            mesh.AddPlane(b1i, b1d + doorFrameV, b1i + doorUp, b1d + doorFrameV + doorUp, internalWallSubmesh);
            mesh.AddPlane(b0d + doorFrameV, b0i, b0d + doorFrameV + doorUp, b0i + doorUp, internalWallSubmesh);
            mesh.AddPlane(b1i + doorUp, b0i + doorUp, b1i + wallUpInternal, b0i + wallUpInternal, internalWallSubmesh);

            //door frame
            mesh.AddPlane(b0d, b1d, b0d + doorFrameV, b1d + doorFrameV, doorFrameSubmesh);
            mesh.AddPlane(b0d, b0d + doorFrameV, b0d + doorUp, b0d + doorFrameV + doorUp, doorFrameSubmesh);
            mesh.AddPlane(b1d + doorFrameV, b1d, b1d + doorFrameV + doorUp, b1d + doorUp, doorFrameSubmesh);
            mesh.AddPlane(b0d + doorFrameV + doorUp, b1d + doorFrameV + doorUp, b0d + doorUp, b1d + doorUp, doorFrameSubmesh);

            if(generateColldier)
            {
                collider.AddPlane(b2, b0, b2 + wallUpExternal, b0 + wallUpExternal);
                collider.AddPlane(b3, b2, b3 + wallUpExternal, b2 + wallUpExternal);
                collider.AddPlane(b1, b3, b1 + wallUpExternal, b3 + wallUpExternal);

                collider.AddPlane(b0, b0d, b0 + doorUp, b0d + doorUp);
                collider.AddPlane(b1d, b1, b1d + doorUp, b1 + doorUp);
                collider.AddPlane(b0 + doorUp, b1 + doorUp, b0 + wallUpExternal, b1 + wallUpExternal);

                if(!collider.usingPrimitives)
                {
                    collider.mesh.AddPlane(b0i, b2i, b0i + wallUpInternal, b2i + wallUpInternal, 0);
                    collider.mesh.AddPlane(b2i, b3i, b2i + wallUpInternal, b3i + wallUpInternal, 0);
                    collider.mesh.AddPlane(b3i, b1i, b3i + wallUpInternal, b1i + wallUpInternal, 0);

                    collider.mesh.AddPlane(b1i, b1d + doorFrameV, b1i + doorUp, b1d + doorFrameV + doorUp, 0);
                    collider.mesh.AddPlane(b0d + doorFrameV, b0i, b0d + doorFrameV + doorUp, b0i + doorUp, 0);
                    collider.mesh.AddPlane(b1i + doorUp, b0i + doorUp, b1i + wallUpInternal, b0i + wallUpInternal, 0);

                    collider.mesh.AddPlane(b0d, b1d, b0d + doorFrameV, b1d + doorFrameV, 0);
                    collider.mesh.AddPlane(b0d, b0d + doorFrameV, b0d + doorUp, b0d + doorFrameV + doorUp, 0);
                    collider.mesh.AddPlane(b1d + doorFrameV, b1d, b1d + doorFrameV + doorUp, b1d + doorUp, 0);
                    collider.mesh.AddPlane(b0d + doorFrameV + doorUp, b1d + doorFrameV + doorUp, b0d + doorUp, b1d + doorUp, 0);
                }
            }
        }
    }
}