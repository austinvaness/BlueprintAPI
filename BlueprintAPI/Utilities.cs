using Sandbox.Definitions;
using Sandbox.ModAPI;
using System;
using VRage.Game;
using VRage.ModAPI;
using VRageMath;

namespace avaness.BlueprintAPI
{
    public static class Utilities
    {
        public static bool IsServer => MyAPIGateway.Session.IsServer;
        public static bool IsDedicated => IsServer && MyAPIGateway.Utilities.IsDedicated;
        public static bool IsPlayer => !IsDedicated;

        public const long MessageId = 8016952063;

        public static Random Rand { get; } = new Random();


        public static MatrixD LocalToWorld(MatrixD local, MatrixD reference)
        {
            return local * reference;
        }

        public static MatrixD WorldToLocalNI(MatrixD world, MatrixD referenceInverted)
        {
            return world * referenceInverted;
        }

        public static MatrixD WorldToLocal(MatrixD world, MatrixD reference)
        {
            return world * MatrixD.Normalize(MatrixD.Invert(reference));
        }

        /*public static Quaternion LocalToWorld(Quaternion local, Quaternion reference)
        {
            return reference * local;
        }

        public static Quaternion WorldToLocal(Quaternion world, Quaternion reference)
        {
            return Quaternion.Inverse(reference) * world;
        }

        public static Quaternion WorldToLocalNI(Quaternion world, Quaternion referenceInverted)
        {
            return referenceInverted * world;
        }*/

        public static void GetBlockPosition(MyObjectBuilder_CubeBlock cube, MyObjectBuilder_CubeGrid grid, MyCubeBlockDefinition def, out MatrixD matrix, out Vector3D size)
        {
            float gridSize = GetGridSize(grid.GridSizeEnum);
            size = def.Size * gridSize;
            Vector3 blockSize = Vector3.Abs(Vector3.TransformNormal(size, cube.BlockOrientation));
            Vector3 localPosition = ((Vector3I)cube.Min * gridSize) - new Vector3(gridSize / 2f);

            Matrix localMatrix;
            ((MyBlockOrientation)cube.BlockOrientation).GetMatrix(out localMatrix);
            localMatrix.Translation = localPosition + (blockSize * 0.5f);

            MatrixD gridMatrix = grid.PositionAndOrientation.Value.GetMatrix();
            
            matrix = LocalToWorld(localMatrix, gridMatrix);
        }

        public static float GetGridSize(MyCubeSize size)
        {
            return size == MyCubeSize.Large ? 2.5f : 0.5f;
        }

        /// <summary>
        /// Projects a value onto another vector.
        /// </summary>
        /// <param name="guide">Must be of length 1.</param>
        public static double ScalerProjection(Vector3D value, Vector3D guide)
        {
            double returnValue = Vector3D.Dot(value, guide);
            if (double.IsNaN(returnValue))
                return 0;
            return returnValue;
        }

        /// <summary>
        /// Projects a value onto another vector.
        /// </summary>
        /// <param name="guide">Must be of length 1.</param>
        public static Vector3D VectorProjection(Vector3D value, Vector3D guide)
        {
            return ScalerProjection(value, guide) * guide;
        }

        /// <summary>
        /// Projects a value onto another vector.
        /// </summary>
        /// <param name="guide">Must be of length 1.</param>
        public static Vector3D VectorRejection(Vector3D value, Vector3D guide)
        {
            return value - VectorProjection(value, guide);
        }

        public static void Invoke(Action action, int delay = 0)
        {
            if(delay <= 0)
                MyAPIGateway.Utilities.InvokeOnGameThread(action);
            else
                MyAPIGateway.Utilities.InvokeOnGameThread(action, StartAt: MyAPIGateway.Session.GameplayFrameCounter + delay);
        }
    }
}
